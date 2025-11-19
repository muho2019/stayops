namespace StayOps.Application.Identity.Commands;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string Name,
    Guid RoleId,
    Guid ActorUserId);
