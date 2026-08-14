using System.ComponentModel.DataAnnotations;

namespace Korp.Inventory.Api.Contracts;

public sealed class DebitStockItemRequest
{
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantity { get; init; }
}
