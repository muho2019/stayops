using Microsoft.EntityFrameworkCore;
using StayOps.Domain.Users;
using StayOps.Infrastructure.Users.Configurations;

namespace StayOps.Infrastructure.Data;

public class StayOpsDbContext : DbContext
{
    public StayOpsDbContext(DbContextOptions<StayOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }
}
