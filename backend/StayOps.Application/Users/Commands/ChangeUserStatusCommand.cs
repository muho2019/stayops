namespace StayOps.Application.Users.Commands;

public sealed record ChangeUserStatusCommand(
    Guid UserId,
    bool IsActive,
    Guid ActorUserId);
