namespace StayOps.Application.Rooms.Contracts;

public sealed record RoomSummaryItem(
    Guid RoomTypeId,
    string RoomTypeName,
    int Total,
    int Active,
    int OutOfService,
    int Dirty);
