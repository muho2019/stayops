using StayOps.Application.Abstractions;
using StayOps.Application.Hotels.Abstractions;
using StayOps.Application.Hotels.Commands;
using StayOps.Application.Hotels.Contracts;
using StayOps.Domain.Abstractions;
using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Services;

public sealed class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IDateTimeProvider _clock;

    public HotelService(IHotelRepository hotelRepository, IDateTimeProvider clock)
    {
        _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<HotelResult> CreateHotelAsync(CreateHotelCommand command, CancellationToken cancellationToken)
    {
        if (await _hotelRepository.CodeExistsAsync(command.Code, cancellationToken))
        {
            throw new ConflictException("Hotel code already exists.");
        }

        var hotel = Hotel.Create(
            new HotelId(Guid.NewGuid()),
            command.Code,
            command.Name,
            command.Timezone,
            _clock.UtcNow,
            command.Status);

        await _hotelRepository.AddAsync(hotel, cancellationToken);

        return Map(hotel);
    }

    public async Task UpdateHotelAsync(UpdateHotelCommand command, CancellationToken cancellationToken)
    {
        Hotel hotel = await GetHotelInternal(command.HotelId, cancellationToken);

        if (await _hotelRepository.CodeExistsAsync(command.Code, cancellationToken, hotel.Id))
        {
            throw new ConflictException("Hotel code already exists.");
        }

        hotel.UpdateDetails(command.Code, command.Name, command.Timezone, command.Status, _clock.UtcNow);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);
    }

    public async Task DeleteHotelAsync(DeleteHotelCommand command, CancellationToken cancellationToken)
    {
        Hotel hotel = await GetHotelInternal(command.HotelId, cancellationToken);
        hotel.Delete(_clock.UtcNow);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);
    }

    public async Task<HotelResult> GetHotelAsync(Guid hotelId, CancellationToken cancellationToken)
    {
        Hotel hotel = await GetHotelInternal(hotelId, cancellationToken);
        return Map(hotel);
    }

    public async Task<PagedResult<HotelResult>> GetHotelsAsync(ListHotelsQuery query, CancellationToken cancellationToken)
    {
        PagedResult<Hotel> hotels = await _hotelRepository.GetPagedAsync(query.Status, query.Page, query.PageSize, cancellationToken);
        var items = hotels.Items.Select(Map).ToArray();
        return new PagedResult<HotelResult>(items, hotels.Total, hotels.Page, hotels.PageSize);
    }

    private async Task<Hotel> GetHotelInternal(Guid hotelId, CancellationToken cancellationToken)
    {
        Hotel? hotel = await _hotelRepository.GetByIdAsync(new HotelId(hotelId), cancellationToken);
        if (hotel is null || hotel.IsDeleted)
        {
            throw new NotFoundException("Hotel not found.");
        }

        return hotel;
    }

    private static HotelResult Map(Hotel hotel)
    {
        return new HotelResult(
            hotel.Id.Value,
            hotel.Code,
            hotel.Name,
            hotel.Timezone,
            hotel.Status,
            hotel.CreatedAt,
            hotel.UpdatedAt);
    }
}
