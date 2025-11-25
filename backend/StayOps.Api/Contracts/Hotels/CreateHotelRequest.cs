using StayOps.Domain.Hotels;

namespace StayOps.Api.Contracts.Hotels;

public sealed record CreateHotelRequest(
    string Code,
    string Name,
    string Timezone,
    HotelStatus Status);
