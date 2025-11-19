using StayOps.Application.Identity.Commands;
using StayOps.Application.Identity.Contracts;

namespace StayOps.Application.Identity.Services;

public interface IIdentityService
{
    Task<Guid> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken);

    Task DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken);

    Task ChangeUserRoleAsync(ChangeUserRoleCommand command, CancellationToken cancellationToken);

    Task ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken);

    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}
