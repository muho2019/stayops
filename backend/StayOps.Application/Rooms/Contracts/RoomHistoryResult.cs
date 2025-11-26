using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Contracts;

public sealed record RoomHistoryResult(
    Guid Id,
    Guid RoomId,
    RoomStatus? Status,
    HousekeepingStatus? HousekeepingStatus,
    string Action,
    string? Reason,
    DateTimeOffset CreatedAt);
