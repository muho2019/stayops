using StayOps.Application.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Domain.RoomTypes;

namespace StayOps.Application.RoomTypes.Abstractions;

public interface IRoomTypeRepository
{
    Task<RoomType?> GetByIdAsync(RoomTypeId id, CancellationToken cancellationToken, bool includeDeleted = false);

    Task<PagedResult<RoomType>> GetByHotelAsync(
        HotelId hotelId,
        RoomTypeStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken, 
        bool includeDeleted = false);

    Task<bool> NameExistsAsync(
        HotelId hotelId,
        string name,
        CancellationToken cancellationToken,
        RoomTypeId? excludeRoomTypeId = null,
        bool includeDeleted = false);

    Task AddAsync(RoomType roomType, CancellationToken cancellationToken);

    Task UpdateAsync(RoomType roomType, CancellationToken cancellationToken);
}
