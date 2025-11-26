using StayOps.Domain.Rooms;

namespace StayOps.Application.Rooms.Commands;

public sealed record CreateRoomCommand(
    Guid HotelId,
    Guid RoomTypeId,
    string Number,
    int? Floor,
    RoomStatus Status,
    HousekeepingStatus HousekeepingStatus);
