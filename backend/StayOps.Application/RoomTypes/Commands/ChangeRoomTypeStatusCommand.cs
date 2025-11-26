using StayOps.Domain.RoomTypes;

namespace StayOps.Application.RoomTypes.Commands;

public sealed record ChangeRoomTypeStatusCommand(
    Guid RoomTypeId,
    RoomTypeStatus Status);
