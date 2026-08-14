using Korp.Inventory.Api.Application;
using Korp.Inventory.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Inventory.Api.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockController(
    IInventoryService inventoryService,
    FailureSimulationState failureSimulation) : ControllerBase
{
    [HttpPost("debits")]
    [ProducesResponseType<DebitStockResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DebitStockResponse>> Debit(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        DebitStockRequest request,
        CancellationToken cancellationToken)
    {
        if (failureSimulation.Enabled)
        {
            throw new Domain.Exceptions.SimulatedInventoryFailureException();
        }

        var items = request.Items
            .Select(item => new DebitItem(item.ProductId, item.Quantity))
            .ToList();

        var result = await inventoryService.DebitStockAsync(
            idempotencyKey,
            items,
            cancellationToken);
        var response = new DebitStockResponse(
            result.Products.Select(ProductResponse.FromDomain).ToList(),
            result.AlreadyProcessed);

        return Ok(response);
    }
}
