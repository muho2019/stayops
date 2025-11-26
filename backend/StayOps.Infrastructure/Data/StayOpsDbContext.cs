using Microsoft.EntityFrameworkCore;
using StayOps.Domain.Users;
using StayOps.Domain.Hotels;
using StayOps.Domain.Rooms;
using StayOps.Domain.RoomTypes;
using StayOps.Infrastructure.Users.Configurations;
using StayOps.Infrastructure.Hotels.Configurations;
using StayOps.Infrastructure.Rooms.Configurations;
using StayOps.Infrastructure.RoomTypes.Configurations;

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

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<RoomHistoryEntry> RoomHistoryEntries => Set<RoomHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new HotelConfiguration());
        modelBuilder.ApplyConfiguration(new RoomTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new RoomHistoryConfiguration());
    }
}
