using StayOps.Application.Abstractions;
using StayOps.Application.Hotels.Abstractions;
using StayOps.Application.RoomTypes.Abstractions;
using StayOps.Application.RoomTypes.Commands;
using StayOps.Application.RoomTypes.Contracts;
using StayOps.Domain.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Domain.RoomTypes;

namespace StayOps.Application.RoomTypes.Services;

public sealed class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IDateTimeProvider _clock;

    public RoomTypeService(IRoomTypeRepository roomTypeRepository, IHotelRepository hotelRepository, IDateTimeProvider clock)
    {
        _roomTypeRepository = roomTypeRepository ?? throw new ArgumentNullException(nameof(roomTypeRepository));
        _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<RoomTypeResult> CreateAsync(CreateRoomTypeCommand command, CancellationToken cancellationToken)
    {
        Hotel hotel = await GetHotel(command.HotelId, cancellationToken);
        if (hotel.Status == HotelStatus.Inactive)
        {
            throw new DomainException("Cannot create room type for an inactive hotel.");
        }

        if (await _roomTypeRepository.NameExistsAsync(hotel.Id, command.Name, cancellationToken))
        {
            throw new ConflictException("Room type name already exists for this hotel.");
        }

        var roomType = RoomType.Create(
            new RoomTypeId(Guid.NewGuid()),
            hotel.Id,
            command.Name,
            command.Description,
            command.Capacity,
            command.BaseRate,
            _clock.UtcNow,
            command.Status);

        await _roomTypeRepository.AddAsync(roomType, cancellationToken);
        return Map(roomType);
    }

    public async Task UpdateAsync(UpdateRoomTypeCommand command, CancellationToken cancellationToken)
    {
        RoomType roomType = await GetRoomType(command.RoomTypeId, cancellationToken);

        if (await _roomTypeRepository.NameExistsAsync(roomType.HotelId, command.Name, cancellationToken, roomType.Id))
        {
            throw new ConflictException("Room type name already exists for this hotel.");
        }

        roomType.UpdateDetails(
            command.Name,
            command.Description,
            command.Capacity,
            command.BaseRate,
            command.Status,
            _clock.UtcNow);

        await _roomTypeRepository.UpdateAsync(roomType, cancellationToken);
    }

    public async Task ChangeStatusAsync(ChangeRoomTypeStatusCommand command, CancellationToken cancellationToken)
    {
        RoomType roomType = await GetRoomType(command.RoomTypeId, cancellationToken);
        roomType.ChangeStatus(command.Status, _clock.UtcNow);
        await _roomTypeRepository.UpdateAsync(roomType, cancellationToken);
    }

    public async Task DeleteAsync(DeleteRoomTypeCommand command, CancellationToken cancellationToken)
    {
        RoomType roomType = await GetRoomType(command.RoomTypeId, cancellationToken);
        roomType.Delete(_clock.UtcNow);
        await _roomTypeRepository.UpdateAsync(roomType, cancellationToken);
    }

    public async Task<RoomTypeResult> GetAsync(Guid roomTypeId, CancellationToken cancellationToken)
    {
        RoomType roomType = await GetRoomType(roomTypeId, cancellationToken);
        return Map(roomType);
    }

    public async Task<PagedResult<RoomTypeResult>> GetListAsync(ListRoomTypesQuery query, CancellationToken cancellationToken)
    {
        PagedResult<RoomType> roomTypes = await _roomTypeRepository.GetByHotelAsync(
            new HotelId(query.HotelId),
            query.Status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = roomTypes.Items.Select(Map).ToArray();
        return new PagedResult<RoomTypeResult>(items, roomTypes.Total, roomTypes.Page, roomTypes.PageSize);
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

    private static RoomTypeResult Map(RoomType roomType)
    {
        return new RoomTypeResult(
            roomType.Id.Value,
            roomType.HotelId.Value,
            roomType.Name,
            roomType.Description,
            roomType.Capacity,
            roomType.BaseRate,
            roomType.Status,
            roomType.CreatedAt,
            roomType.UpdatedAt);
    }
}
