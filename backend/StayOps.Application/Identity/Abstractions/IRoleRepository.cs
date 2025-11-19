using StayOps.Domain.Identity;

namespace StayOps.Application.Identity.Abstractions;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken);

    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task AddAsync(Role role, CancellationToken cancellationToken);
}
