using StayOps.Application.Identity;
using StayOps.Domain.Identity;
using StayOps.Infrastructure.Data;

namespace StayOps.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(StayOpsDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (!dbContext.Roles.Any())
        {
            Role admin = Role.Create(
                new RoleId(Guid.NewGuid()),
                IdentityDefaults.AdminRoleName,
                new[]
                {
                    Permission.InventoryManage,
                    Permission.ReservationsManage,
                    Permission.CheckInOut,
                    Permission.HousekeepingUpdate,
                    Permission.RatesManage,
                    Permission.ReportingView,
                    Permission.UsersManage
                });

            Role staff = Role.Create(
                new RoleId(Guid.NewGuid()),
                IdentityDefaults.StaffRoleName,
                new[]
                {
                    Permission.ReservationsManage,
                    Permission.CheckInOut,
                    Permission.HousekeepingUpdate,
                    Permission.ReportingView
                });

            dbContext.Roles.AddRange(admin, staff);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
