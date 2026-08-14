using Korp.Billing.Api.Application;
using Korp.Billing.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InvoiceResponse>> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.CreateAsync(request.Items, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            InvoiceResponse.FromDomain(invoice));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvoiceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> List(
        CancellationToken cancellationToken)
    {
        var invoices = await invoiceService.ListAsync(cancellationToken);
        return Ok(invoices.Select(InvoiceResponse.FromDomain).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InvoiceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        return Ok(InvoiceResponse.FromDomain(invoice));
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType<CloseInvoiceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CloseInvoiceResponse>> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await invoiceService.CloseAsync(id, cancellationToken);
        return Ok(new CloseInvoiceResponse(
            InvoiceResponse.FromDomain(result.Invoice),
            result.AlreadyClosed,
            result.StockOperationAlreadyProcessed));
    }
}
