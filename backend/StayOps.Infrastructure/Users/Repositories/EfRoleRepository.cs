using Microsoft.EntityFrameworkCore;
using StayOps.Application.Users.Abstractions;
using StayOps.Domain.Users;
using StayOps.Infrastructure.Data;

namespace StayOps.Infrastructure.Users.Repositories;

public sealed class EfRoleRepository : IRoleRepository
{
    private readonly StayOpsDbContext _dbContext;

    public EfRoleRepository(StayOpsDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken)
    {
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
