using Korp.Billing.Api.Contracts;
using Korp.Billing.Api.Domain;

namespace Korp.Billing.Api.Application;

public interface IInvoiceService
{
    Task<Invoice> CreateAsync(
        IReadOnlyCollection<CreateInvoiceItemRequest> items,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken);

    Task<Invoice> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CloseInvoiceResult> CloseAsync(Guid id, CancellationToken cancellationToken);
}

public sealed record CloseInvoiceResult(
    Invoice Invoice,
    bool AlreadyClosed,
    bool StockOperationAlreadyProcessed);
