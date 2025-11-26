using StayOps.Domain.Abstractions;
using StayOps.Domain.Hotels;

namespace StayOps.Domain.RoomTypes;

public sealed class RoomType : Entity<RoomTypeId>
{
    private RoomType()
        : base(default)
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    private RoomType(
        RoomTypeId id,
        HotelId hotelId,
        string name,
        string? description,
        int capacity,
        decimal baseRate,
        RoomTypeStatus status,
        DateTimeOffset createdAt)
        : base(id)
    {
        HotelId = hotelId;
        Name = ValidateName(name);
        Description = description?.Trim() ?? string.Empty;
        Capacity = ValidateCapacity(capacity);
        BaseRate = ValidateBaseRate(baseRate);
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public HotelId HotelId { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public int Capacity { get; private set; }

    public decimal BaseRate { get; private set; }

    public RoomTypeStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public static RoomType Create(
        RoomTypeId id,
        HotelId hotelId,
        string name,
        string? description,
        int capacity,
        decimal baseRate,
        DateTimeOffset createdAt,
        RoomTypeStatus status = RoomTypeStatus.Active)
    {
        return new RoomType(id, hotelId, name, description, capacity, baseRate, status, createdAt);
    }

    public void UpdateDetails(string name, string? description, int capacity, decimal baseRate, RoomTypeStatus status, DateTimeOffset occurredAt)
    {
        Name = ValidateName(name);
        Description = description?.Trim() ?? string.Empty;
        Capacity = ValidateCapacity(capacity);
        BaseRate = ValidateBaseRate(baseRate);
        Status = status;
        Touch(occurredAt);
    }

    public void ChangeStatus(RoomTypeStatus status, DateTimeOffset occurredAt)
    {
        Status = status;
        Touch(occurredAt);
    }

    public void Delete(DateTimeOffset occurredAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        Status = RoomTypeStatus.Inactive;
        Touch(occurredAt);
    }

    private void Touch(DateTimeOffset occurredAt)
    {
        UpdatedAt = occurredAt;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Room type name is required.");
        }

        return name.Trim();
    }

    private static int ValidateCapacity(int capacity)
    {
        if (capacity is < 1 or > 10)
        {
            throw new DomainException("Capacity must be between 1 and 10.");
        }

        return capacity;
    }

    private static decimal ValidateBaseRate(decimal baseRate)
    {
        if (baseRate < 0)
        {
            throw new DomainException("Base rate must be greater than or equal to zero.");
        }

        return baseRate;
    }
}
