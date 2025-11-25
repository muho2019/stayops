using Microsoft.EntityFrameworkCore;
using StayOps.Domain.Users;
using StayOps.Domain.Hotels;
using StayOps.Infrastructure.Users.Configurations;
using StayOps.Infrastructure.Hotels.Configurations;

namespace StayOps.Infrastructure.Data;

public class StayOpsDbContext : DbContext
{
    public StayOpsDbContext(DbContextOptions<StayOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Hotel> Hotels => Set<Hotel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new HotelConfiguration());
    }
}
