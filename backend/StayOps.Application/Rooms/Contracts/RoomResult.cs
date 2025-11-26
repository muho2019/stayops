using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Contracts;

public sealed record RoomResult(
    Guid Id,
    Guid HotelId,
    Guid RoomTypeId,
    string Number,
    int? Floor,
    RoomStatus Status,
    HousekeepingStatus HousekeepingStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string RowVersion);
