namespace Korp.Inventory.Api.Contracts;

public sealed record DebitStockResponse(
    IReadOnlyList<ProductResponse> UpdatedProducts,
    bool AlreadyProcessed);
