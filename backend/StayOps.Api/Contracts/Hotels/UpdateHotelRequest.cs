using StayOps.Domain.Hotels;

namespace StayOps.Api.Contracts.Hotels;

public sealed record UpdateHotelRequest(
    string Code,
    string Name,
    string Timezone,
    HotelStatus Status);
