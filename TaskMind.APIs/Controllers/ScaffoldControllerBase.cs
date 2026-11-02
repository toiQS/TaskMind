using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
