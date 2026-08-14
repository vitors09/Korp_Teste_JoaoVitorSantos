using Korp.Inventory.Api.Domain.Exceptions;

namespace Korp.Inventory.Api.Domain;

public sealed class Product
{
    private Product()
    {
    }

    private Product(Guid id, string code, string description, int balance)
    {
        Id = id;
        Code = code;
        Description = description;
        Balance = balance;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Balance { get; private set; }
    public int Version { get; private set; }

    public static Product Create(string code, string description, int initialBalance)
    {
        var normalizedCode = NormalizeCode(code);
        var normalizedDescription = description?.Trim() ?? string.Empty;

        if (normalizedCode.Length == 0)
        {
            throw new DomainRuleException("O código do produto é obrigatório.");
        }

        if (normalizedCode.Length > 50)
        {
            throw new DomainRuleException("O código do produto deve possuir no máximo 50 caracteres.");
        }

        if (normalizedDescription.Length == 0)
        {
            throw new DomainRuleException("A descrição do produto é obrigatória.");
        }

        if (normalizedDescription.Length > 200)
        {
            throw new DomainRuleException("A descrição do produto deve possuir no máximo 200 caracteres.");
        }

        if (initialBalance < 0)
        {
            throw new DomainRuleException("O saldo inicial não pode ser negativo.");
        }

        return new Product(Guid.NewGuid(), normalizedCode, normalizedDescription, initialBalance);
    }

    public void Debit(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("A quantidade da baixa deve ser maior que zero.");
        }

        if (quantity > Balance)
        {
            throw new InsufficientStockException(Id, Code, Balance, quantity);
        }

        Balance -= quantity;
        Version++;
    }

    public static string NormalizeCode(string? code) =>
        code?.Trim().ToUpperInvariant() ?? string.Empty;
}
