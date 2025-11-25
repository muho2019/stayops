using StayOps.Application.Abstractions;
using StayOps.Application.Hotels.Commands;
using StayOps.Application.Hotels.Contracts;

namespace StayOps.Application.Hotels.Services;

public interface IHotelService
{
    Task<HotelResult> CreateHotelAsync(CreateHotelCommand command, CancellationToken cancellationToken);

    Task UpdateHotelAsync(UpdateHotelCommand command, CancellationToken cancellationToken);

    Task DeleteHotelAsync(DeleteHotelCommand command, CancellationToken cancellationToken);

    Task<HotelResult> GetHotelAsync(Guid hotelId, CancellationToken cancellationToken);

    Task<PagedResult<HotelResult>> GetHotelsAsync(ListHotelsQuery query, CancellationToken cancellationToken);
}
