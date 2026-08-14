using Korp.Billing.Api.Application;
using Korp.Billing.Api.Contracts;
using Korp.Billing.Api.Domain;
using Korp.Billing.Api.Domain.Exceptions;
using Korp.Billing.Api.Infrastructure;
using Korp.Billing.Api.Integration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Korp.Billing.Api.Tests.Application;

public sealed class InvoiceServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly Guid productId = Guid.NewGuid();
    private BillingDbContext database = null!;
    private FakeInventoryClient inventoryClient = null!;
    private InvoiceService service = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlite(connection)
            .Options;

        database = new BillingDbContext(options);
        await database.Database.EnsureCreatedAsync();
        inventoryClient = new FakeInventoryClient(
            new InventoryProduct(productId, "PROD-01", "Produto", 10));
        service = new InvoiceService(database, inventoryClient);
    }

    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_GeneratesSequentialNumbers()
    {
        var items = new[]
        {
            new CreateInvoiceItemRequest { ProductId = productId, Quantity = 1 }
        };

        var first = await service.CreateAsync(items, CancellationToken.None);
        var second = await service.CreateAsync(items, CancellationToken.None);

        Assert.Equal(1, first.Number);
        Assert.Equal(2, second.Number);
        Assert.Equal(InvoiceStatus.Open, second.Status);
    }

    [Fact]
    public async Task CloseAsync_WhenInventoryFails_KeepsInvoiceOpenForRetry()
    {
        var invoice = await service.CreateAsync(
            [new CreateInvoiceItemRequest { ProductId = productId, Quantity = 2 }],
            CancellationToken.None);
        inventoryClient.ShouldFail = true;

        await Assert.ThrowsAsync<InventoryUnavailableException>(() =>
            service.CloseAsync(invoice.Id, CancellationToken.None));

        database.ChangeTracker.Clear();
        var persisted = await database.Invoices.SingleAsync();
        Assert.Equal(InvoiceStatus.Open, persisted.Status);
        Assert.NotNull(persisted.LastProcessingError);
    }

    [Fact]
    public async Task CloseAsync_WhenRetried_UsesInvoiceIdAsIdempotencyKey()
    {
        var invoice = await service.CreateAsync(
            [new CreateInvoiceItemRequest { ProductId = productId, Quantity = 2 }],
            CancellationToken.None);

        var result = await service.CloseAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(InvoiceStatus.Closed, result.Invoice.Status);
        Assert.Equal(invoice.Id, inventoryClient.LastIdempotencyKey);
    }

    private sealed class FakeInventoryClient(InventoryProduct product) : IInventoryClient
    {
        public bool ShouldFail { get; set; }
        public Guid LastIdempotencyKey { get; private set; }

        public Task<IReadOnlyList<InventoryProduct>> ListProductsAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<InventoryProduct>>([product]);

        public Task<StockDebitResult> DebitStockAsync(
            Guid idempotencyKey,
            IReadOnlyCollection<StockDebitItem> items,
            CancellationToken cancellationToken)
        {
            LastIdempotencyKey = idempotencyKey;

            if (ShouldFail)
            {
                throw new InventoryUnavailableException("Estoque indisponível para teste.");
            }

            return Task.FromResult(new StockDebitResult(false));
        }
    }
}
