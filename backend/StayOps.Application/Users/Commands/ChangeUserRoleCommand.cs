namespace StayOps.Application.Users.Commands;

public sealed record ChangeUserRoleCommand(
    Guid UserId,
    string RoleName,
    Guid ActorUserId);
