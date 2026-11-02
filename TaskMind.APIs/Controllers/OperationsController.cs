using Microsoft.AspNetCore.Mvc;

namespace TaskMind.APIs.Controllers;

[ApiController]
[Route("")]
public sealed class OperationsController : ControllerBase
{
    [HttpGet("health/live")]
    [EndpointSummary("Check API liveness")]
    public IActionResult Liveness() => Ok(new { status = "live" });

    [HttpGet("health/ready")]
    [EndpointSummary("Check API readiness")]
    public IActionResult Readiness() =>
        Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Readiness checks are not configured",
            detail: "The API cannot report readiness until its dependency checks are implemented.");
}
