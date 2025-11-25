using StayOps.Domain.Hotels;

namespace StayOps.Api.Contracts.Hotels;

public sealed record HotelResponse(
    Guid Id,
    string Code,
    string Name,
    string Timezone,
    HotelStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
