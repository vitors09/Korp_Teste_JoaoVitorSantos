namespace Korp.Billing.Api.Domain.Exceptions;

public sealed class InventoryUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
