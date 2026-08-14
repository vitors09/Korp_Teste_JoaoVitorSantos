namespace Korp.Inventory.Api.Domain.Exceptions;

public sealed class ConcurrentStockUpdateException()
    : DomainRuleException("O estoque foi alterado por outra operação. Tente novamente.");
