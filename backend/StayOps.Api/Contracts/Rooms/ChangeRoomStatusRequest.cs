using StayOps.Domain.Rooms;

namespace StayOps.Api.Contracts.Rooms;

public sealed record ChangeRoomStatusRequest(
    RoomStatus Status,
    string? Reason);
