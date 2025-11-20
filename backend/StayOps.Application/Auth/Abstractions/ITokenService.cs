using StayOps.Domain.Users;

namespace StayOps.Application.Auth.Abstractions;

public interface ITokenService
{
    string GenerateToken(User user, Role role);
}
