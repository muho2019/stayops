using StayOps.Domain.Users;

namespace StayOps.Api.Contracts.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string Name,
    string Role,
    UserStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<Permission> Permissions);
