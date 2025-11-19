using System.ComponentModel;

namespace StayOps.Domain.Identity;

[Flags]
public enum Permission
{
    None = 0,
    InventoryManage = 1 << 0,
    ReservationsManage = 1 << 1,
    CheckInOut = 1 << 2,
    HousekeepingUpdate = 1 << 3,
    RatesManage = 1 << 4,
    ReportingView = 1 << 5,
    UsersManage = 1 << 6
}
