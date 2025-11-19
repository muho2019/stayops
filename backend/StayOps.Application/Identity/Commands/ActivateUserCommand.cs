namespace StayOps.Application.Identity.Commands;

public sealed record ActivateUserCommand(
    Guid UserId,
    Guid ActorUserId);
