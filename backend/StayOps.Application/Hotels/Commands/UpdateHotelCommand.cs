using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Commands;

public sealed record UpdateHotelCommand(
    Guid HotelId,
    string Code,
    string Name,
    string Timezone,
    HotelStatus Status);
