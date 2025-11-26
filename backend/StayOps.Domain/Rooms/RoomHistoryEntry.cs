using StayOps.Domain.Hotels;
using StayOps.Domain.RoomTypes;

namespace StayOps.Domain.Rooms;

public sealed class RoomHistoryEntry
{
    private RoomHistoryEntry()
    {
        Id = Guid.Empty;
        Action = string.Empty;
        Reason = string.Empty;
    }

    private RoomHistoryEntry(
        Guid id,
        RoomId roomId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        RoomStatus? status,
        HousekeepingStatus? housekeepingStatus,
        string action,
        string? reason,
        DateTimeOffset createdAt)
    {
        Id = id;
        RoomId = roomId;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        Status = status;
        HousekeepingStatus = housekeepingStatus;
        Action = action;
        Reason = reason?.Trim() ?? string.Empty;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public RoomId RoomId { get; private set; }

    public HotelId HotelId { get; private set; }

    public RoomTypeId RoomTypeId { get; private set; }

    public RoomStatus? Status { get; private set; }

    public HousekeepingStatus? HousekeepingStatus { get; private set; }

    public string Action { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static RoomHistoryEntry StatusChanged(
        Room room,
        RoomStatus status,
        string? reason,
        DateTimeOffset occurredAt)
    {
        return new RoomHistoryEntry(
            Guid.NewGuid(),
            room.Id,
            room.HotelId,
            room.RoomTypeId,
            status,
            null,
            "StatusChanged",
            reason,
            occurredAt);
    }

    public static RoomHistoryEntry HousekeepingChanged(
        Room room,
        HousekeepingStatus housekeepingStatus,
        string? reason,
        DateTimeOffset occurredAt)
    {
        return new RoomHistoryEntry(
            Guid.NewGuid(),
            room.Id,
            room.HotelId,
            room.RoomTypeId,
            null,
            housekeepingStatus,
            "HousekeepingChanged",
            reason,
            occurredAt);
    }

    public static RoomHistoryEntry Created(Room room, DateTimeOffset occurredAt)
    {
        return new RoomHistoryEntry(
            Guid.NewGuid(),
            room.Id,
            room.HotelId,
            room.RoomTypeId,
            room.Status,
            room.HousekeepingStatus,
            "Created",
            null,
            occurredAt);
    }
}
