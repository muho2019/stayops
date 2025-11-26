using StayOps.Domain.Rooms;

namespace StayOps.Api.Contracts.Rooms;

public sealed record UpdateRoomRequest(
    Guid RoomTypeId,
    string Number,
    int? Floor,
    RoomStatus Status,
    HousekeepingStatus HousekeepingStatus,
    string? RowVersion);
