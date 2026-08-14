using Korp.Billing.Api.Domain;

namespace Korp.Billing.Api.Contracts;

public sealed record InvoiceItemResponse(
    Guid ProductId,
    string ProductCode,
    string ProductDescription,
    int Quantity);

public sealed record InvoiceResponse(
    Guid Id,
    int Number,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    string? LastProcessingError,
    IReadOnlyList<InvoiceItemResponse> Items)
{
    public static InvoiceResponse FromDomain(Invoice invoice) => new(
        invoice.Id,
        invoice.Number,
        invoice.Status == InvoiceStatus.Open ? "Aberta" : "Fechada",
        invoice.CreatedAtUtc,
        invoice.ClosedAtUtc,
        invoice.LastProcessingError,
        invoice.Items
            .OrderBy(item => item.ProductCode)
            .Select(item => new InvoiceItemResponse(
                item.ProductId,
                item.ProductCode,
                item.ProductDescription,
                item.Quantity))
            .ToList());
}

public sealed record CloseInvoiceResponse(
    InvoiceResponse Invoice,
    bool AlreadyClosed,
    bool StockOperationAlreadyProcessed);
