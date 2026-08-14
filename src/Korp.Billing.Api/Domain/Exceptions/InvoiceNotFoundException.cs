namespace Korp.Billing.Api.Domain.Exceptions;

public sealed class InvoiceNotFoundException(Guid id)
    : BillingRuleException($"A nota fiscal '{id}' não foi encontrada.");
