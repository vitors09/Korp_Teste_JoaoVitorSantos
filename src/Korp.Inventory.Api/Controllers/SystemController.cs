using Korp.Inventory.Api.Application;
using Korp.Inventory.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Inventory.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController(FailureSimulationState failureSimulation) : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", service = "inventory" });

    [HttpGet("failure-simulation")]
    public IActionResult GetFailureSimulation() =>
        Ok(new FailureSimulationRequest(failureSimulation.Enabled));

    [HttpPut("failure-simulation")]
    public IActionResult SetFailureSimulation(FailureSimulationRequest request)
    {
        failureSimulation.SetEnabled(request.Enabled);
        return Ok(new FailureSimulationRequest(failureSimulation.Enabled));
    }
}
