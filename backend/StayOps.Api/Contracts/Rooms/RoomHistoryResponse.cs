using StayOps.Domain.Rooms;

namespace StayOps.Api.Contracts.Rooms;

public sealed record RoomHistoryResponse(
    Guid Id,
    Guid RoomId,
    RoomStatus? Status,
    HousekeepingStatus? HousekeepingStatus,
    string Action,
    string? Reason,
    DateTimeOffset CreatedAt);
