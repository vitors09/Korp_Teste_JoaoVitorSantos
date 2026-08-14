using Korp.Inventory.Api.Domain;

namespace Korp.Inventory.Api.Application;

public sealed record DebitStockResult(
    IReadOnlyList<Product> Products,
    bool AlreadyProcessed);
