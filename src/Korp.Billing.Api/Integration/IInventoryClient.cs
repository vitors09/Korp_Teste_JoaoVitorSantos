using Korp.Billing.Api.Domain;

namespace Korp.Billing.Api.Integration;

public interface IInventoryClient
{
    Task<IReadOnlyList<InventoryProduct>> ListProductsAsync(
        CancellationToken cancellationToken);

    Task<StockDebitResult> DebitStockAsync(
        Guid idempotencyKey,
        IReadOnlyCollection<StockDebitItem> items,
        CancellationToken cancellationToken);
}

public sealed record InventoryProduct(
    Guid Id,
    string Code,
    string Description,
    int Balance);

public sealed record StockDebitItem(Guid ProductId, int Quantity);

public sealed record StockDebitResult(bool AlreadyProcessed);
