namespace StayOps.Application.Identity.Commands;

public sealed record DeactivateUserCommand(
    Guid UserId,
    Guid ActorUserId);
