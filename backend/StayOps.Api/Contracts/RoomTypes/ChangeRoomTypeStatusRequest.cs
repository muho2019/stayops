using StayOps.Domain.RoomTypes;

namespace StayOps.Api.Contracts.RoomTypes;

public sealed record ChangeRoomTypeStatusRequest(RoomTypeStatus Status);
