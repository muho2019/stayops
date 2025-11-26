using StayOps.Domain.RoomTypes;

namespace StayOps.Application.RoomTypes.Commands;

public sealed record UpdateRoomTypeCommand(
    Guid RoomTypeId,
    string Name,
    string? Description,
    int Capacity,
    decimal BaseRate,
    RoomTypeStatus Status);
