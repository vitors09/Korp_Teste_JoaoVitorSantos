namespace Korp.Inventory.Api.Domain.Exceptions;

public sealed class InsufficientStockException(
    Guid productId,
    string productCode,
    int availableBalance,
    int requestedQuantity)
    : DomainRuleException(
        $"O produto {productCode} possui saldo {availableBalance}, mas a baixa solicitou {requestedQuantity}.")
{
    public Guid ProductId { get; } = productId;
    public int AvailableBalance { get; } = availableBalance;
    public int RequestedQuantity { get; } = requestedQuantity;
}
