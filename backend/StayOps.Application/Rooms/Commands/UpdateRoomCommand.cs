using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Commands;

public sealed record UpdateRoomCommand(
    Guid RoomId,
    Guid RoomTypeId,
    string Number,
    int? Floor,
    RoomStatus Status,
    HousekeepingStatus HousekeepingStatus,
    byte[]? RowVersion);
