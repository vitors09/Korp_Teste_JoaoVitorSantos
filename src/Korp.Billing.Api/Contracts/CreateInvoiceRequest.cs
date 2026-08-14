using System.ComponentModel.DataAnnotations;

namespace Korp.Billing.Api.Contracts;

public sealed class CreateInvoiceRequest
{
    [Required(ErrorMessage = "Os produtos são obrigatórios.")]
    [MinLength(1, ErrorMessage = "A nota deve possuir pelo menos um produto.")]
    public required IReadOnlyList<CreateInvoiceItemRequest> Items { get; init; }
}

public sealed class CreateInvoiceItemRequest
{
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantity { get; init; }
}
