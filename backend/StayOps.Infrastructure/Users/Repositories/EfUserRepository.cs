using Microsoft.EntityFrameworkCore;
using StayOps.Application.Users;
using StayOps.Application.Users.Abstractions;
using StayOps.Domain.Users;
using StayOps.Infrastructure.Data;

namespace StayOps.Infrastructure.Users.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly StayOpsDbContext _dbContext;

    public EfUserRepository(StayOpsDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public Task<bool> HasOtherActiveAdminsAsync(UserId excludeUserId, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Join(
                _dbContext.Roles.AsNoTracking(),
                user => user.RoleId,
                role => role.Id,
                (user, role) => new { user, role })
            .AnyAsync(
                x => x.user.Id != excludeUserId
                     && x.user.Status == UserStatus.Active
                     && x.role.Name == UserRoleDefaults.AdminRoleName,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
