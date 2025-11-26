using StayOps.Domain.RoomTypes;
using System.ComponentModel.DataAnnotations;

namespace StayOps.Application.RoomTypes.Commands;

public sealed record CreateRoomTypeCommand(
    [Required] Guid HotelId,
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Required, StringLength(1000, MinimumLength = 1)] string? Description,
    [Required] int Capacity,
    [Required] decimal BaseRate,
    [Required] RoomTypeStatus Status);
