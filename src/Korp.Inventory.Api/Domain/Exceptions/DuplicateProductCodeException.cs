namespace Korp.Inventory.Api.Domain.Exceptions;

public sealed class DuplicateProductCodeException(string code)
    : Exception($"Já existe um produto cadastrado com o código {code}.");
