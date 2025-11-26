using StayOps.Domain.Rooms;

namespace StayOps.Api.Contracts.Rooms;

public sealed record CreateRoomRequest(
    Guid HotelId,
    Guid RoomTypeId,
    string Number,
    int? Floor,
    RoomStatus Status,
    HousekeepingStatus? HousekeepingStatus = null);
