using StayOps.Application.Abstractions;
using StayOps.Application.Hotels.Abstractions;
using StayOps.Application.RoomTypes.Abstractions;
using StayOps.Application.Rooms.Abstractions;
using StayOps.Application.Rooms.Commands;
using StayOps.Application.Rooms.Contracts;
using StayOps.Domain.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Domain.Rooms;
using StayOps.Domain.RoomTypes;

namespace StayOps.Application.Rooms.Services;

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IDateTimeProvider _clock;

    public RoomService(
        IRoomRepository roomRepository,
        IRoomTypeRepository roomTypeRepository,
        IHotelRepository hotelRepository,
        IDateTimeProvider clock)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _roomTypeRepository = roomTypeRepository ?? throw new ArgumentNullException(nameof(roomTypeRepository));
        _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<RoomResult> CreateAsync(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        Hotel hotel = await GetHotel(command.HotelId, cancellationToken);
        if (hotel.Status == HotelStatus.Inactive)
        {
            throw new DomainException("Hotel is inactive.");
        }

        RoomType roomType = await GetRoomType(command.RoomTypeId, cancellationToken);
        if (roomType.HotelId != hotel.Id)
        {
            throw new DomainException("Room type does not belong to the hotel.");
        }

        if (roomType.Status == RoomTypeStatus.Inactive)
        {
            throw new DomainException("Inactive room types cannot be used to create rooms.");
        }

        if (await _roomRepository.NumberExistsAsync(hotel.Id, command.Number, cancellationToken))
        {
            throw new ConflictException("Room number already exists for this hotel.");
        }

        var room = Room.Create(
            new RoomId(Guid.NewGuid()),
            hotel.Id,
            roomType.Id,
            command.Number,
            command.Floor,
            command.Status,
            command.HousekeepingStatus,
            _clock.UtcNow);

        await _roomRepository.AddAsync(room, cancellationToken);
        await _roomRepository.AddHistoryAsync(RoomHistoryEntry.Created(room, _clock.UtcNow), cancellationToken);

        return Map(room);
    }

    public async Task UpdateAsync(UpdateRoomCommand command, CancellationToken cancellationToken)
    {
        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new DomainException("Row version is required.");
        }

        Room room = await GetRoom(command.RoomId, cancellationToken);
        RoomType roomType = await GetRoomType(command.RoomTypeId, cancellationToken);

        if (roomType.HotelId != room.HotelId)
        {
            throw new DomainException("Room type does not belong to the same hotel.");
        }

        if (roomType.Status == RoomTypeStatus.Inactive)
        {
            throw new DomainException("Inactive room types cannot be assigned.");
        }

        if (await _roomRepository.NumberExistsAsync(room.HotelId, command.Number, cancellationToken, room.Id))
        {
            throw new ConflictException("Room number already exists for this hotel.");
        }

        bool statusChanged = room.Status != command.Status;
        bool housekeepingChanged = room.HousekeepingStatus != command.HousekeepingStatus;

        room.UpdateDetails(roomType.Id, command.Number, command.Floor, command.Status, command.HousekeepingStatus, _clock.UtcNow);
        await _roomRepository.UpdateAsync(room, command.RowVersion, cancellationToken);

        if (statusChanged)
        {
            await _roomRepository.AddHistoryAsync(RoomHistoryEntry.StatusChanged(room, room.Status, null, _clock.UtcNow), cancellationToken);
        }

        if (housekeepingChanged)
        {
            await _roomRepository.AddHistoryAsync(RoomHistoryEntry.HousekeepingChanged(room, room.HousekeepingStatus, null, _clock.UtcNow), cancellationToken);
        }
    }

    public async Task ChangeStatusAsync(ChangeRoomStatusCommand command, CancellationToken cancellationToken)
    {
        Room room = await GetRoom(command.RoomId, cancellationToken);
        if (room.Status == command.Status)
        {
            return;
        }

        room.ChangeStatus(command.Status, _clock.UtcNow);
        await _roomRepository.UpdateAsync(room, null, cancellationToken);
        await _roomRepository.AddHistoryAsync(RoomHistoryEntry.StatusChanged(room, room.Status, command.Reason, _clock.UtcNow), cancellationToken);
    }

    public async Task ChangeHousekeepingStatusAsync(ChangeHousekeepingStatusCommand command, CancellationToken cancellationToken)
    {
        Room room = await GetRoom(command.RoomId, cancellationToken);
        if (room.HousekeepingStatus == command.HousekeepingStatus)
        {
            return;
        }

        room.ChangeHousekeepingStatus(command.HousekeepingStatus, _clock.UtcNow);
        await _roomRepository.UpdateAsync(room, null, cancellationToken);
        await _roomRepository.AddHistoryAsync(RoomHistoryEntry.HousekeepingChanged(room, room.HousekeepingStatus, command.Reason, _clock.UtcNow), cancellationToken);
    }

    public async Task DeleteAsync(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        Room room = await GetRoom(command.RoomId, cancellationToken);
        room.Delete(_clock.UtcNow);
        await _roomRepository.UpdateAsync(room, null, cancellationToken);
        await _roomRepository.AddHistoryAsync(RoomHistoryEntry.StatusChanged(room, room.Status, "Deleted", _clock.UtcNow), cancellationToken);
    }

    public async Task<RoomResult> GetAsync(Guid roomId, CancellationToken cancellationToken)
    {
        Room room = await GetRoom(roomId, cancellationToken);
        return Map(room);
    }

    public async Task<PagedResult<RoomResult>> GetListAsync(ListRoomsQuery query, CancellationToken cancellationToken)
    {
        PagedResult<Room> rooms = await _roomRepository.GetByHotelAsync(
            new HotelId(query.HotelId),
            query.Status,
            query.RoomTypeId.HasValue ? new RoomTypeId(query.RoomTypeId.Value) : null,
            query.HousekeepingStatus,
            string.IsNullOrWhiteSpace(query.Number) ? null : query.Number,
            query.Floor,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = rooms.Items.Select(Map).ToArray();
        return new PagedResult<RoomResult>(items, rooms.Total, rooms.Page, rooms.PageSize);
    }

    public async Task<IReadOnlyCollection<RoomSummaryItem>> GetSummaryAsync(Guid hotelId, CancellationToken cancellationToken)
    {
        await GetHotel(hotelId, cancellationToken);
        return await _roomRepository.GetSummaryAsync(new HotelId(hotelId), cancellationToken);
    }

    public async Task<PagedResult<RoomHistoryResult>> GetHistoryAsync(Guid hotelId, Guid roomId, int page, int pageSize, CancellationToken cancellationToken)
    {
        Room room = await GetRoom(roomId, cancellationToken);
        if (room.HotelId != new HotelId(hotelId))
        {
            throw new DomainException("Room does not belong to the hotel.");
        }

        PagedResult<RoomHistoryEntry> history = await _roomRepository.GetHistoryAsync(room.Id, page, pageSize, cancellationToken);
        var items = history.Items
            .Select(entry => new RoomHistoryResult(
                entry.Id,
                entry.RoomId.Value,
                entry.Status,
                entry.HousekeepingStatus,
                entry.Action,
                string.IsNullOrWhiteSpace(entry.Reason) ? null : entry.Reason,
                entry.CreatedAt))
            .ToArray();

        return new PagedResult<RoomHistoryResult>(items, history.Total, history.Page, history.PageSize);
    }

    private async Task<Room> GetRoom(Guid roomId, CancellationToken cancellationToken)
    {
        Room? room = await _roomRepository.GetByIdAsync(new RoomId(roomId), cancellationToken);
        if (room is null || room.IsDeleted)
        {
            throw new NotFoundException("Room not found.");
        }

        return room;
    }

    private async Task<RoomType> GetRoomType(Guid roomTypeId, CancellationToken cancellationToken)
    {
        RoomType? roomType = await _roomTypeRepository.GetByIdAsync(new RoomTypeId(roomTypeId), cancellationToken);
        if (roomType is null || roomType.IsDeleted)
        {
            throw new NotFoundException("Room type not found.");
        }

        return roomType;
    }

    private async Task<Hotel> GetHotel(Guid hotelId, CancellationToken cancellationToken)
    {
        Hotel? hotel = await _hotelRepository.GetByIdAsync(new HotelId(hotelId), cancellationToken);
        if (hotel is null || hotel.IsDeleted)
        {
            throw new NotFoundException("Hotel not found.");
        }

        return hotel;
    }

    private static RoomResult Map(Room room)
    {
        string rowVersion = room.RowVersion is { Length: > 0 }
            ? Convert.ToBase64String(room.RowVersion)
            : string.Empty;

        return new RoomResult(
            room.Id.Value,
            room.HotelId.Value,
            room.RoomTypeId.Value,
            room.Number,
            room.Floor,
            room.Status,
            room.HousekeepingStatus,
            room.CreatedAt,
            room.UpdatedAt,
            rowVersion);
    }
}
