using StayOps.Domain.Users;

namespace StayOps.Application.Users.Contracts;

public sealed record UserResult(
    Guid Id,
    string Email,
    string Name,
    string Role,
    UserStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<Permission> Permissions);
