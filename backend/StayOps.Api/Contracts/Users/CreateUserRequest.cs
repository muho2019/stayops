namespace StayOps.Api.Contracts.Users;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string Name,
    string? RoleName = null);
