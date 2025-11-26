using StayOps.Domain.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Domain.RoomTypes;

namespace StayOps.Domain.Rooms;

public sealed class Room : Entity<RoomId>
{
    private Room()
        : base(default)
    {
        Number = string.Empty;
        RowVersion = Array.Empty<byte>();
    }

    private Room(
        RoomId id,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        string number,
        int? floor,
        RoomStatus status,
        HousekeepingStatus housekeepingStatus,
        DateTimeOffset createdAt)
        : base(id)
    {
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        Number = ValidateNumber(number);
        Floor = floor;
        Status = status;
        HousekeepingStatus = housekeepingStatus;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        RowVersion = Array.Empty<byte>();
    }

    public HotelId HotelId { get; }

    public RoomTypeId RoomTypeId { get; private set; }

    public string Number { get; private set; }

    public int? Floor { get; private set; }

    public RoomStatus Status { get; private set; }

    public HousekeepingStatus HousekeepingStatus { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public byte[] RowVersion { get; private set; }

    public static Room Create(
        RoomId id,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        string number,
        int? floor,
        RoomStatus status,
        HousekeepingStatus housekeepingStatus,
        DateTimeOffset createdAt)
    {
        return new Room(id, hotelId, roomTypeId, number, floor, status, housekeepingStatus, createdAt);
    }

    public void UpdateDetails(RoomTypeId roomTypeId, string number, int? floor, RoomStatus status, HousekeepingStatus housekeepingStatus, DateTimeOffset occurredAt)
    {
        RoomTypeId = roomTypeId;
        Number = ValidateNumber(number);
        Floor = floor;
        Status = status;
        HousekeepingStatus = housekeepingStatus;
        Touch(occurredAt);
    }

    public void ChangeStatus(RoomStatus status, DateTimeOffset occurredAt)
    {
        Status = status;
        Touch(occurredAt);
    }

    public void ChangeHousekeepingStatus(HousekeepingStatus housekeepingStatus, DateTimeOffset occurredAt)
    {
        HousekeepingStatus = housekeepingStatus;
        Touch(occurredAt);
    }

    public void Delete(DateTimeOffset occurredAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        Status = RoomStatus.Inactive;
        Touch(occurredAt);
    }

    private void Touch(DateTimeOffset occurredAt)
    {
        UpdatedAt = occurredAt;
    }

    private static string ValidateNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("Room number is required.");
        }

        return number.Trim();
    }
}
