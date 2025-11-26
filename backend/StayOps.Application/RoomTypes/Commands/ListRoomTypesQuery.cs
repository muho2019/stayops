using StayOps.Domain.RoomTypes;

namespace StayOps.Application.RoomTypes.Commands;

public sealed record ListRoomTypesQuery(
    Guid HotelId,
    RoomTypeStatus? Status,
    int Page = 1,
    int PageSize = 50);
