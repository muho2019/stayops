using StayOps.Domain.RoomTypes;

namespace StayOps.Api.Contracts.RoomTypes;

public sealed record RoomTypeResponse(
    Guid Id,
    Guid HotelId,
    string Name,
    string? Description,
    int Capacity,
    decimal BaseRate,
    RoomTypeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
