namespace StayOps.Api.Contracts.Rooms;

public sealed record RoomSummaryResponse(
    Guid RoomTypeId,
    string RoomTypeName,
    int Total,
    int Active,
    int OutOfService,
    int Dirty);
