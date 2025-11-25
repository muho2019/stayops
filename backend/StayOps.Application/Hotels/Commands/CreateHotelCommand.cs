using StayOps.Domain.Hotels;
using System.ComponentModel.DataAnnotations;

namespace StayOps.Application.Hotels.Commands;

public sealed record CreateHotelCommand(
    [Required, StringLength(50, MinimumLength = 1)] string Code,
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Required, StringLength(100, MinimumLength = 1)] string Timezone,
    [Required] HotelStatus Status);
