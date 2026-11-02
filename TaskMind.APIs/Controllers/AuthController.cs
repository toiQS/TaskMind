using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths;

namespace TaskMind.APIs.Controllers;

public sealed class AuthController : ScaffoldControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/registrations")]
    [EndpointSummary("Register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/login")]
    [EndpointSummary("Login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/refresh-tokens")]
    [EndpointSummary("Refresh Tokens")]
    public async Task<IActionResult> RefreshTokens(
        [FromBody] RefreshTokensCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [Authorize]
    [HttpPost("auth/logout")]
    [EndpointSummary("Logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
            return Unauthorized();
        return ToActionResult(await _sender.Send(new LogoutCommand(accountId), cancellationToken));
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/email-verification-requests")]
    [EndpointSummary("Request Email Verification")]
    public async Task<IActionResult> RequestEmailVerification(
        [FromBody] RequestEmailVerificationCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/email-verifications/confirm")]
    [EndpointSummary("Confirm Email Verification")]
    public async Task<IActionResult> ConfirmEmailVerification(
        [FromBody] ConfirmEmailVerificationCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/password-reset-requests")]
    [EndpointSummary("Request Password Reset")]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/password-reset-requests/confirm")]
    [EndpointSummary("Confirm Password Reset")]
    public async Task<IActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("auth/oauth/{provider}")]
    [EndpointSummary("Authenticate with an OAuth provider")]
    public IActionResult OAuth(string provider) =>
        Problem(
            statusCode: StatusCodes.Status501NotImplemented,
            title: "OAuth provider is not configured",
            detail: $"OAuth provider '{provider}' has not been selected for this deployment.");

    [Authorize]
    [HttpGet("me")]
    [EndpointSummary("Get Current User")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryGetAccountId(out var accountId))
            return Unauthorized();
        return ToActionResult(await _sender.Send(new GetCurrentAccountQuery(accountId), cancellationToken));
    }

    [Authorize]
    [HttpPatch("me/profile")]
    [EndpointSummary("Update Current Profile")]
    public IActionResult UpdateCurrentProfile() => NotImplemented(nameof(UpdateCurrentProfile));

    [AllowAnonymous]
    [HttpGet("users/{userId:guid}")]
    [EndpointSummary("Get Public User")]
    public IActionResult GetPublicUser(Guid userId) => NotImplemented(nameof(GetPublicUser));

    private bool TryGetAccountId(out Guid accountId)
    {
        var claimValue = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claimValue, out accountId);
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        var response = ApiResponse.FromServiceResult(result);
        return StatusCode(response.StatusCode, response);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var response = ApiResponse.FromServiceResult(result);
        return StatusCode(response.StatusCode, response);
    }
}
