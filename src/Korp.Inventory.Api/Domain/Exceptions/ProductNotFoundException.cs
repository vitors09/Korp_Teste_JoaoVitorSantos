namespace Korp.Inventory.Api.Domain.Exceptions;

public sealed class ProductNotFoundException(Guid productId)
    : Exception($"O produto {productId} não foi encontrado.");
