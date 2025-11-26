using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Commands;

public sealed record ChangeRoomStatusCommand(
    Guid RoomId,
    RoomStatus Status,
    string? Reason);
