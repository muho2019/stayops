using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Contracts;

public sealed record HotelResult(
    Guid Id,
    string Code,
    string Name,
    string Timezone,
    HotelStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
