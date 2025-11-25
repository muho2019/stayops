using StayOps.Application.Abstractions;
using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Abstractions;

public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(HotelId id, CancellationToken cancellationToken, bool includeDeleted = false);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken, HotelId? excludeHotelId = null, bool includeDeleted = false);

    Task<PagedResult<Hotel>> GetPagedAsync(HotelStatus? status, int page, int pageSize, CancellationToken cancellationToken, bool includeDeleted = false);

    Task AddAsync(Hotel hotel, CancellationToken cancellationToken);

    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken);
}
