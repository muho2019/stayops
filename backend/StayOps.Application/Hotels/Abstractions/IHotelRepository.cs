using StayOps.Application.Abstractions;
using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Abstractions;

public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(HotelId id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken, HotelId? excludeHotelId = null);

    Task<PagedResult<Hotel>> GetPagedAsync(HotelStatus? status, int page, int pageSize, CancellationToken cancellationToken);

    Task AddAsync(Hotel hotel, CancellationToken cancellationToken);

    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken);
}
