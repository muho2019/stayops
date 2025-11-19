using StayOps.Domain.Identity;

namespace StayOps.Application.Identity.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);

    Task<bool> HasOtherActiveAdminsAsync(UserId excludeUserId, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
