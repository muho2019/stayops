using StayOps.Domain.RoomTypes;

namespace StayOps.Application.RoomTypes.Contracts;

public sealed record RoomTypeResult(
    Guid Id,
    Guid HotelId,
    string Name,
    string? Description,
    int Capacity,
    decimal BaseRate,
    RoomTypeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
