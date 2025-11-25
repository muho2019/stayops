using System.ComponentModel.DataAnnotations;
using StayOps.Domain.Hotels;

namespace StayOps.Api.Contracts.Hotels;

public sealed record CreateHotelRequest(
    [Required, StringLength(50, MinimumLength = 1)] string Code,
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Required, StringLength(100, MinimumLength = 1)] string Timezone,
    [Required] HotelStatus Status);
