namespace Korp.Inventory.Api.Domain.Exceptions;

public sealed class SimulatedInventoryFailureException()
    : Exception("O serviço de estoque está temporariamente indisponível (falha simulada).");
