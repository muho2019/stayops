using StayOps.Domain.Users;

namespace StayOps.Application.Auth.Contracts;

public sealed record LoginResult(
    string Token,
    Guid UserId,
    string Name,
    string Role,
    IReadOnlyCollection<Permission> Permissions);
