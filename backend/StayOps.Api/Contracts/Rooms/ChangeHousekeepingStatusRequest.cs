using StayOps.Domain.Rooms;

namespace StayOps.Api.Contracts.Rooms;

public sealed record ChangeHousekeepingStatusRequest(
    HousekeepingStatus HousekeepingStatus,
    string? Reason);
