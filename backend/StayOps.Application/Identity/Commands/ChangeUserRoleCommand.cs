namespace StayOps.Application.Identity.Commands;

public sealed record ChangeUserRoleCommand(
    Guid UserId,
    Guid RoleId,
    Guid ActorUserId);
