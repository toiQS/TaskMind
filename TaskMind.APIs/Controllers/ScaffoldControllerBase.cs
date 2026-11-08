using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMind.ApplicationLayer.Commons;

namespace TaskMind.APIs.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public abstract class ScaffoldControllerBase : ControllerBase
{
    protected ObjectResult NotImplemented(string operation) =>
        Problem(
            statusCode: StatusCodes.Status501NotImplemented,
            title: "Endpoint scaffold only",
            detail: $"{operation} has not been implemented.");

    protected IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var response = ApiResponse.FromServiceResult(result);
        return StatusCode(response.StatusCode, response);
    }

    protected IActionResult ToActionResult(ServiceResult result)
    {
        var response = ApiResponse.FromServiceResult(result);
        return StatusCode(response.StatusCode, response);
    }
}
