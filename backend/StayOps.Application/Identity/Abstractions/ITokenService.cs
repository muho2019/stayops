using StayOps.Domain.Identity;

namespace StayOps.Application.Identity.Abstractions;

public interface ITokenService
{
    string GenerateToken(User user, Role role);
}
