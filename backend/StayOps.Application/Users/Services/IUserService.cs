using StayOps.Application.Users.Commands;
using StayOps.Application.Users.Contracts;

namespace StayOps.Application.Users.Services;

public interface IUserService
{
    Task<Guid> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task ChangeUserStatusAsync(ChangeUserStatusCommand command, CancellationToken cancellationToken);

    Task ChangeUserRoleAsync(ChangeUserRoleCommand command, CancellationToken cancellationToken);

    Task ChangePasswordAsync(UpdatePasswordCommand command, CancellationToken cancellationToken);

    Task<UserResult> GetUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserResult>> GetUsersAsync(CancellationToken cancellationToken);
}
