using Korp.Inventory.Api.Application;
using Korp.Inventory.Api.Domain;
using Korp.Inventory.Api.Domain.Exceptions;
using Korp.Inventory.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Korp.Inventory.Api.Tests.Application;

public sealed class InventoryServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private InventoryDbContext database = null!;
    private InventoryService service = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        database = new InventoryDbContext(options);
        await database.Database.EnsureCreatedAsync();
        service = new InventoryService(database);
    }

    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task CreateProductAsync_WithRepeatedCode_ThrowsConflict()
    {
        await service.CreateProductAsync("abc-01", "Teclado", 10, CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateProductCodeException>(() =>
            service.CreateProductAsync(
                " ABC-01 ",
                "Outro teclado",
                5,
                CancellationToken.None));
    }

    [Fact]
    public async Task DebitStockAsync_WithRepeatedProduct_AggregatesQuantitiesUsingLinq()
    {
        var product = await service.CreateProductAsync(
            "ABC-01",
            "Teclado",
            10,
            CancellationToken.None);

        var items = new[]
        {
            new DebitItem(product.Id, 2),
            new DebitItem(product.Id, 3)
        };

        var result = await service.DebitStockAsync(
            Guid.NewGuid(),
            items,
            CancellationToken.None);

        Assert.Single(result.Products);
        Assert.Equal(5, result.Products[0].Balance);
    }

    [Fact]
    public async Task DebitStockAsync_WhenOneProductHasInsufficientStock_DoesNotPersistAnyDebit()
    {
        var first = Product.Create("ABC-01", "Teclado", 10);
        var second = Product.Create("ABC-02", "Mouse", 1);
        database.Products.AddRange(first, second);
        await database.SaveChangesAsync(CancellationToken.None);

        var items = new[]
        {
            new DebitItem(first.Id, 2),
            new DebitItem(second.Id, 2)
        };

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            service.DebitStockAsync(Guid.NewGuid(), items, CancellationToken.None));

        database.ChangeTracker.Clear();
        var persistedProducts = await database.Products
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .ToListAsync(CancellationToken.None);

        Assert.Equal(10, persistedProducts[0].Balance);
        Assert.Equal(1, persistedProducts[1].Balance);
    }

    [Fact]
    public async Task DebitStockAsync_WithRepeatedIdempotencyKey_DebitsOnlyOnce()
    {
        var product = await service.CreateProductAsync(
            "ABC-01",
            "Teclado",
            10,
            CancellationToken.None);
        var operationId = Guid.NewGuid();
        var items = new[] { new DebitItem(product.Id, 2) };

        var first = await service.DebitStockAsync(operationId, items, CancellationToken.None);
        database.ChangeTracker.Clear();
        var second = await service.DebitStockAsync(operationId, items, CancellationToken.None);

        Assert.False(first.AlreadyProcessed);
        Assert.True(second.AlreadyProcessed);
        Assert.Equal(8, second.Products.Single().Balance);
    }
}
