using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Commands;

public sealed record ListRoomsQuery(
    Guid HotelId,
    RoomStatus? Status,
    Guid? RoomTypeId,
    HousekeepingStatus? HousekeepingStatus,
    string? Number,
    int? Floor,
    int Page = 1,
    int PageSize = 50);
