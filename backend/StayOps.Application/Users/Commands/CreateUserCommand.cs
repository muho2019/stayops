namespace StayOps.Application.Users.Commands;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string Name,
    string? RoleName,
    Guid ActorUserId);
