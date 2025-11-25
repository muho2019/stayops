using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Commands;

public sealed record CreateHotelCommand(
    string Code,
    string Name,
    string Timezone,
    HotelStatus Status);
