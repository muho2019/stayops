namespace StayOps.Application.Identity.Commands;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string NewPassword,
    Guid ActorUserId);
