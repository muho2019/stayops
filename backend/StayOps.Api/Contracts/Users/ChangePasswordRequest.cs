namespace StayOps.Api.Contracts.Users;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
