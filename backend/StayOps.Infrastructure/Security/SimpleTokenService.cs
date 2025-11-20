using StayOps.Application.Auth.Abstractions;
using StayOps.Domain.Users;

namespace StayOps.Infrastructure.Security;

/// <summary>
/// Simplified token service for development/demo. Replace with JWT implementation later.
/// </summary>
public sealed class SimpleTokenService : ITokenService
{
    public string GenerateToken(User user, Role role)
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
