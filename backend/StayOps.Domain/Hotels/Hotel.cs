using StayOps.Domain.Abstractions;

namespace StayOps.Domain.Hotels;

public sealed class Hotel : Entity<HotelId>
{
    private Hotel()
        : base(default)
    {
        Code = string.Empty;
        Name = string.Empty;
        Timezone = string.Empty;
    }

    private Hotel(
        HotelId id,
        string code,
        string name,
        string timezone,
        HotelStatus status,
        DateTimeOffset createdAt)
        : base(id)
    {
        Code = ValidateCode(code);
        Name = ValidateName(name);
        Timezone = ValidateTimezone(timezone);
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Timezone { get; private set; }

    public HotelStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public static Hotel Create(
        HotelId id,
        string code,
        string name,
        string timezone,
        DateTimeOffset createdAt,
        HotelStatus status = HotelStatus.Active)
    {
        return new Hotel(id, code, name, timezone, status, createdAt);
    }

    public void UpdateDetails(string code, string name, string timezone, HotelStatus status, DateTimeOffset occurredAt)
    {
        Code = ValidateCode(code);
        Name = ValidateName(name);
        Timezone = ValidateTimezone(timezone);
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
        Status = HotelStatus.Inactive;
        Touch(occurredAt);
    }

    private void Touch(DateTimeOffset occurredAt)
    {
        UpdatedAt = occurredAt;
    }

    private static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Hotel code is required.");
        }

        return code.Trim();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hotel name is required.");
        }

        return name.Trim();
    }

    private static string ValidateTimezone(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
        {
            throw new DomainException("Timezone is required.");
        }

        return timezone.Trim();
    }
}
