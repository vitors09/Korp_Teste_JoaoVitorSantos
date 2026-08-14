using Korp.Inventory.Api.Domain;

namespace Korp.Inventory.Api.Contracts;

public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Description,
    int Balance)
{
    public static ProductResponse FromDomain(Product product) =>
        new(product.Id, product.Code, product.Description, product.Balance);
}
