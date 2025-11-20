using StayOps.Domain.Users;

namespace StayOps.Api.Contracts.Auth;

public sealed record LoginResponse(
    Guid UserId,
    string Name,
    string Role,
    string Token,
    IReadOnlyCollection<Permission> Permissions);
