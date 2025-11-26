using StayOps.Domain.Rooms;

namespace StayOps.Api.Contracts.Rooms;

public sealed record RoomResponse(
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
