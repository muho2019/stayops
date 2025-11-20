using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Api.Contracts.Auth;
using StayOps.Application.Auth.Commands;
using StayOps.Application.Auth.Services;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _authService.LoginAsync(command, cancellationToken);

        var response = new LoginResponse(
            result.UserId,
            result.Name,
            result.Role,
            result.Token,
            result.Permissions);

        return Ok(response);
    }
}
