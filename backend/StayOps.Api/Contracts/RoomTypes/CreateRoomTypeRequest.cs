using StayOps.Domain.RoomTypes;

namespace StayOps.Api.Contracts.RoomTypes;

public sealed record CreateRoomTypeRequest(
    Guid HotelId,
    string Name,
    string? Description,
    int Capacity,
    decimal BaseRate,
    RoomTypeStatus Status);
