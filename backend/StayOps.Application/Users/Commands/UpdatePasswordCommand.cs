namespace StayOps.Application.Users.Commands;

public sealed record UpdatePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword);
