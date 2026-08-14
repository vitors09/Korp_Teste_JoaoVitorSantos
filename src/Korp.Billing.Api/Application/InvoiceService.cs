using System.Data;
using Korp.Billing.Api.Contracts;
using Korp.Billing.Api.Domain;
using Korp.Billing.Api.Domain.Exceptions;
using Korp.Billing.Api.Infrastructure;
using Korp.Billing.Api.Integration;
using Microsoft.EntityFrameworkCore;

namespace Korp.Billing.Api.Application;

public sealed class InvoiceService(
    BillingDbContext database,
    IInventoryClient inventoryClient) : IInvoiceService
{
    public async Task<Invoice> CreateAsync(
        IReadOnlyCollection<CreateInvoiceItemRequest> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0 || items.Any(item => item.ProductId == Guid.Empty || item.Quantity <= 0))
        {
            throw new BillingRuleException(
                "Cada item deve possuir um produto válido e uma quantidade maior que zero.");
        }

        var quantitiesByProduct = items
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

        var inventoryProducts = await inventoryClient.ListProductsAsync(cancellationToken);
        var productsById = inventoryProducts.ToDictionary(product => product.Id);

        var missingProduct = quantitiesByProduct.Keys.FirstOrDefault(id => !productsById.ContainsKey(id));
        if (missingProduct != Guid.Empty)
        {
            throw new BillingRuleException($"O produto '{missingProduct}' não existe no estoque.");
        }

        var snapshots = quantitiesByProduct
            .Select(pair =>
            {
                var product = productsById[pair.Key];
                return new ProductSnapshot(product.Id, product.Code, product.Description, pair.Value);
            })
            .OrderBy(product => product.Code)
            .ToList();

        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var lastNumber = await database.Invoices
            .Select(invoice => (int?)invoice.Number)
            .MaxAsync(cancellationToken) ?? 0;

        var invoice = Invoice.Create(lastNumber + 1, snapshots);
        database.Invoices.Add(invoice);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return invoice;
    }

    public async Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken) =>
        await database.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync(cancellationToken);

    public async Task<Invoice> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await database.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(invoice => invoice.Id == id, cancellationToken)
        ?? throw new InvoiceNotFoundException(id);

    public async Task<CloseInvoiceResult> CloseAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await database.Invoices
            .Include(current => current.Items)
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken)
            ?? throw new InvoiceNotFoundException(id);

        if (invoice.Status == InvoiceStatus.Closed)
        {
            return new CloseInvoiceResult(invoice, true, true);
        }

        try
        {
            var debit = await inventoryClient.DebitStockAsync(
                invoice.Id,
                invoice.Items
                    .Select(item => new StockDebitItem(item.ProductId, item.Quantity))
                    .ToList(),
                cancellationToken);

            invoice.Close();
            await database.SaveChangesAsync(cancellationToken);
            return new CloseInvoiceResult(invoice, false, debit.AlreadyProcessed);
        }
        catch (InventoryUnavailableException exception)
        {
            invoice.RegisterProcessingFailure(exception.Message);
            await database.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
