using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TaskMind.APIs.Controllers;

public sealed class AuthController : ScaffoldControllerBase
{
    [HttpPost("auth/registrations")]
    [EndpointSummary("Register")]
    public IActionResult Register([FromBody] JsonElement request) => NotImplemented(nameof(Register));
    [HttpPost("auth/login")]
    [EndpointSummary("Login")]
    public IActionResult Login([FromBody] JsonElement request) => NotImplemented(nameof(Login));
    [HttpPost("auth/refresh-tokens")]
    [EndpointSummary("Refresh Tokens")]
    public IActionResult RefreshTokens([FromBody] JsonElement request) => NotImplemented(nameof(RefreshTokens));

    [HttpPost("auth/logout")]
    [EndpointSummary("Logout")]
    public IActionResult Logout() => NotImplemented(nameof(Logout));
    [HttpPost("auth/email-verification-requests")]
    [EndpointSummary("Request Email Verification")]
    public IActionResult RequestEmailVerification([FromBody] JsonElement request) => NotImplemented(nameof(RequestEmailVerification));
    [HttpPost("auth/email-verifications/confirm")]
    [EndpointSummary("Confirm Email Verification")]
    public IActionResult ConfirmEmailVerification([FromBody] JsonElement request) => NotImplemented(nameof(ConfirmEmailVerification));
    [HttpPost("auth/password-reset-requests")]
    [EndpointSummary("Request Password Reset")]
    public IActionResult RequestPasswordReset([FromBody] JsonElement request) => NotImplemented(nameof(RequestPasswordReset));
    [HttpPost("auth/password-reset-requests/confirm")]
    [EndpointSummary("Confirm Password Reset")]
    public IActionResult ConfirmPasswordReset([FromBody] JsonElement request) => NotImplemented(nameof(ConfirmPasswordReset));
    [HttpPost("auth/oauth/{provider}")]
    [EndpointSummary("Authenticate with an OAuth provider")]
    public IActionResult OAuth(string provider, [FromBody] JsonElement request) => NotImplemented(nameof(OAuth));

    [HttpGet("me")]
    [EndpointSummary("Get Current User")]
    public IActionResult GetCurrentUser() => NotImplemented(nameof(GetCurrentUser));

    [HttpPatch("me/profile")]
    [EndpointSummary("Update Current Profile")]
    public IActionResult UpdateCurrentProfile([FromBody] JsonElement request) => NotImplemented(nameof(UpdateCurrentProfile));
    [HttpGet("users/{userId:guid}")]
    [EndpointSummary("Get Public User")]
    public IActionResult GetPublicUser(Guid userId) => NotImplemented(nameof(GetPublicUser));
}
