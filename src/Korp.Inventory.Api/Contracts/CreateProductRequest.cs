using System.ComponentModel.DataAnnotations;

namespace Korp.Inventory.Api.Contracts;

public sealed class CreateProductRequest
{
    [Required(ErrorMessage = "O código é obrigatório.")]
    [MaxLength(50, ErrorMessage = "O código deve possuir no máximo 50 caracteres.")]
    public required string Code { get; init; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição deve possuir no máximo 200 caracteres.")]
    public required string Description { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "O saldo deve ser maior ou igual a zero.")]
    public int Balance { get; init; }
}
