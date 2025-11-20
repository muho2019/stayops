using Microsoft.EntityFrameworkCore;
using StayOps.Application.Users;
using StayOps.Domain.Users;
using StayOps.Infrastructure.Data;
using StayOps.Infrastructure.Security;

namespace StayOps.Infrastructure.Users;

public static class UserSeeder
{
    private const string DefaultAdminEmail = "admin@stayops.local";
    private const string DefaultAdminPassword = "stayops@1234";

    public static async Task SeedAsync(
        StayOpsDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedRolesAsync(dbContext, cancellationToken);
        await SeedAdminUserAsync(dbContext, cancellationToken);
    }

    private static async Task SeedRolesAsync(StayOpsDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Roles.AnyAsync(cancellationToken))
        {
            return;
        }

        Role admin = Role.Create(
            new RoleId(Guid.NewGuid()),
            UserRoleDefaults.AdminRoleName,
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
            UserRoleDefaults.StaffRoleName,
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

    private static async Task SeedAdminUserAsync(StayOpsDbContext dbContext, CancellationToken cancellationToken)
    {
        Email email = Email.Create(DefaultAdminEmail);
        bool adminExists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (adminExists)
        {
            return;
        }

        Role adminRole = await dbContext.Roles
            .AsNoTracking()
            .FirstAsync(r => r.Name == UserRoleDefaults.AdminRoleName, cancellationToken);

        var passwordHasher = new Pbkdf2PasswordHasher();
        string passwordHash = passwordHasher.Hash(DefaultAdminPassword);

        User adminUser = User.Create(
            new UserId(Guid.NewGuid()),
            email,
            passwordHash,
            "StayOps Administrator",
            adminRole.Id,
            DateTimeOffset.UtcNow,
            new UserId(Guid.Empty));

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
