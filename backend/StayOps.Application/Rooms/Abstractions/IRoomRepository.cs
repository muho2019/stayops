using StayOps.Application.Abstractions;
using StayOps.Application.Rooms.Contracts;
using StayOps.Domain.Hotels;
using StayOps.Domain.Rooms;
using StayOps.Domain.RoomTypes;

namespace StayOps.Application.Rooms.Abstractions;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(RoomId roomId, CancellationToken cancellationToken, bool includeDeleted = false);

    Task<PagedResult<Room>> GetByHotelAsync(
        HotelId hotelId,
        RoomStatus? status,
        RoomTypeId? roomTypeId,
        HousekeepingStatus? housekeepingStatus,
        string? number,
        int? floor,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        bool includeDeleted = false);

    Task<bool> NumberExistsAsync(HotelId hotelId, string number, CancellationToken cancellationToken, RoomId? excludeRoomId = null, bool includeDeleted = false);

    Task AddAsync(Room room, CancellationToken cancellationToken);

    Task UpdateAsync(Room room, byte[]? expectedRowVersion, CancellationToken cancellationToken);

    Task AddHistoryAsync(RoomHistoryEntry entry, CancellationToken cancellationToken);

    Task<PagedResult<RoomHistoryEntry>> GetHistoryAsync(RoomId roomId, int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RoomSummaryItem>> GetSummaryAsync(HotelId hotelId, CancellationToken cancellationToken);
}
