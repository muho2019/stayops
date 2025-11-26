using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Commands;

public sealed record ChangeHousekeepingStatusCommand(
    Guid RoomId,
    HousekeepingStatus HousekeepingStatus,
    string? Reason);
