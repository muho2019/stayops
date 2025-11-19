using StayOps.Domain.Identity;

namespace StayOps.Application.Identity.Contracts;

public sealed record LoginResult(
    string Token,
    Guid UserId,
    string Name,
    string Role,
    IReadOnlyCollection<Permission> Permissions);
