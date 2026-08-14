using System.ComponentModel.DataAnnotations;

namespace Korp.Inventory.Api.Contracts;

public sealed class DebitStockRequest
{
    [Required(ErrorMessage = "Os itens da baixa são obrigatórios.")]
    [MinLength(1, ErrorMessage = "A baixa deve possuir pelo menos um item.")]
    public required IReadOnlyList<DebitStockItemRequest> Items { get; init; }
}
