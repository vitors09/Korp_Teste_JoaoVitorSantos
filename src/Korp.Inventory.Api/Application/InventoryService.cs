using Korp.Inventory.Api.Domain;
using Korp.Inventory.Api.Domain.Exceptions;
using Korp.Inventory.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Korp.Inventory.Api.Application;

public sealed class InventoryService(InventoryDbContext database) : IInventoryService
{
    public async Task<Product> CreateProductAsync(
        string code,
        string description,
        int initialBalance,
        CancellationToken cancellationToken)
    {
        var normalizedCode = Product.NormalizeCode(code);

        if (await database.Products.AnyAsync(
                product => product.Code == normalizedCode,
                cancellationToken))
        {
            throw new DuplicateProductCodeException(normalizedCode);
        }

        var product = Product.Create(code, description, initialBalance);
        database.Products.Add(product);
        await database.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<IReadOnlyList<Product>> ListProductsAsync(CancellationToken cancellationToken) =>
        await database.Products
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .ToListAsync(cancellationToken);

    public async Task<Product> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken) =>
        await database.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == productId, cancellationToken)
        ?? throw new ProductNotFoundException(productId);

    public async Task<DebitStockResult> DebitStockAsync(
        Guid idempotencyKey,
        IReadOnlyCollection<DebitItem> items,
        CancellationToken cancellationToken)
    {
        if (idempotencyKey == Guid.Empty)
        {
            throw new DomainRuleException("A chave de idempotência é obrigatória.");
        }

        if (items.Count == 0)
        {
            throw new DomainRuleException("A baixa deve possuir pelo menos um produto.");
        }

        if (items.Any(item => item.ProductId == Guid.Empty || item.Quantity <= 0))
        {
            throw new DomainRuleException(
                "Cada item deve possuir um produto válido e uma quantidade maior que zero.");
        }

        var quantitiesByProduct = items
            .GroupBy(item => item.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => item.Quantity));

        var productIds = quantitiesByProduct.Keys.ToArray();

        if (await database.StockOperations.AsNoTracking().AnyAsync(
                operation => operation.IdempotencyKey == idempotencyKey,
                cancellationToken))
        {
            var alreadyUpdatedProducts = await database.Products
                .AsNoTracking()
                .Where(product => productIds.Contains(product.Id))
                .OrderBy(product => product.Code)
                .ToListAsync(cancellationToken);

            return new DebitStockResult(alreadyUpdatedProducts, true);
        }

        var products = await database.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        var missingProductId = productIds
            .Except(products.Select(product => product.Id))
            .FirstOrDefault();

        if (missingProductId != Guid.Empty)
        {
            throw new ProductNotFoundException(missingProductId);
        }

        foreach (var product in products)
        {
            product.Debit(quantitiesByProduct[product.Id]);
        }

        database.StockOperations.Add(StockOperation.Create(idempotencyKey));

        try
        {
            await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrentStockUpdateException();
        }
        catch (DbUpdateException)
        {
            database.ChangeTracker.Clear();

            if (await database.StockOperations.AsNoTracking().AnyAsync(
                    operation => operation.IdempotencyKey == idempotencyKey,
                    cancellationToken))
            {
                var concurrentlyUpdatedProducts = await database.Products
                    .AsNoTracking()
                    .Where(product => productIds.Contains(product.Id))
                    .OrderBy(product => product.Code)
                    .ToListAsync(cancellationToken);

                return new DebitStockResult(concurrentlyUpdatedProducts, true);
            }

            throw;
        }

        return new DebitStockResult(
            products.OrderBy(product => product.Code).ToList(),
            false);
    }
}
