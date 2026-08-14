using Korp.Inventory.Api.Domain;

namespace Korp.Inventory.Api.Application;

public interface IInventoryService
{
    Task<Product> CreateProductAsync(
        string code,
        string description,
        int initialBalance,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> ListProductsAsync(CancellationToken cancellationToken);

    Task<Product> GetProductAsync(Guid productId, CancellationToken cancellationToken);

    Task<DebitStockResult> DebitStockAsync(
        Guid idempotencyKey,
        IReadOnlyCollection<DebitItem> items,
        CancellationToken cancellationToken);
}
