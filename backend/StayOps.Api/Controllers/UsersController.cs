using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayOps.Api.Contracts.Users;
using StayOps.Application.Users;
using StayOps.Application.Users.Commands;
using StayOps.Application.Users.Contracts;
using StayOps.Application.Users.Services;

namespace StayOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.Password,
            request.Name,
            request.RoleName,
            GetActorId());

        Guid userId = await _userService.CreateUserAsync(command, cancellationToken);
        UserResult createdUser = await _userService.GetUserAsync(userId, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = userId }, Map(createdUser));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserStatus(
        Guid id,
        [FromBody] ChangeUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserStatusCommand(id, request.IsActive, GetActorId());
        await _userService.ChangeUserStatusAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserRoleCommand(id, request.RoleName, GetActorId());
        await _userService.ChangeUserRoleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("me/password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        Guid actorId = GetActorId();
        if (actorId == Guid.Empty)
        {
            return Forbid();
        }

        var command = new UpdatePasswordCommand(actorId, request.CurrentPassword, request.NewPassword);
        await _userService.ChangePasswordAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        UserResult user = await _userService.GetUserAsync(id, cancellationToken);
        return Ok(Map(user));
    }

    [HttpGet]
    [Authorize(Roles = UserRoleDefaults.AdminRoleName)]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<UserResult> users = await _userService.GetUsersAsync(cancellationToken);
        var response = users.Select(Map);
        return Ok(response);
    }

    private Guid GetActorId()
    {
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.TryParse(userIdClaim, out Guid value) ? value : Guid.Empty;
    }

    private static UserResponse Map(UserResult result)
    {
        return new UserResponse(
            result.Id,
            result.Email,
            result.Name,
            result.Role,
            result.Status,
            result.CreatedAt,
            result.UpdatedAt,
            result.Permissions);
    }
}
