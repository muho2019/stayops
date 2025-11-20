using StayOps.Application.Auth.Commands;
using StayOps.Application.Auth.Contracts;

namespace StayOps.Application.Auth.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}
