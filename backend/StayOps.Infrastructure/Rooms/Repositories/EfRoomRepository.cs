using Microsoft.EntityFrameworkCore;
using StayOps.Application.Abstractions;
using StayOps.Application.Rooms.Abstractions;
using StayOps.Application.Rooms.Contracts;
using StayOps.Domain.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Domain.Rooms;
using StayOps.Domain.RoomTypes;
using StayOps.Infrastructure.Data;

namespace StayOps.Infrastructure.Rooms.Repositories;

public sealed class EfRoomRepository : IRoomRepository
{
    private readonly StayOpsDbContext _dbContext;

    public EfRoomRepository(StayOpsDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Room?> GetByIdAsync(RoomId roomId, CancellationToken cancellationToken, bool includeDeleted = false)
    {
        IQueryable<Room> query = includeDeleted
            ? _dbContext.Rooms.IgnoreQueryFilters()
            : _dbContext.Rooms;

        return query.FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
    }

    public async Task<PagedResult<Room>> GetByHotelAsync(
        HotelId hotelId,
        RoomStatus? status,
        RoomTypeId? roomTypeId,
        HousekeepingStatus? housekeepingStatus,
        string? number,
        int? floor,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        bool includeDeleted = false)
    {
        IQueryable<Room> query = includeDeleted
            ? _dbContext.Rooms
                .IgnoreQueryFilters()
            : _dbContext.Rooms;

        query = query
            .AsNoTracking()
            .Where(r => r.HotelId == hotelId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (roomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        if (housekeepingStatus.HasValue)
        {
            query = query.Where(r => r.HousekeepingStatus == housekeepingStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(number))
        {
            query = query.Where(r => r.Number.Contains(number));
        }

        if (floor.HasValue)
        {
            query = query.Where(r => r.Floor == floor.Value);
        }

        int total = await query.CountAsync(cancellationToken);
        List<Room> items = await query
            .OrderBy(r => r.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Room>(items, total, page, pageSize);
    }

    public Task<bool> NumberExistsAsync(HotelId hotelId, string number, CancellationToken cancellationToken, RoomId? excludeRoomId = null, bool includeDeleted = false)
    {
        var query = includeDeleted
            ? _dbContext.Rooms.IgnoreQueryFilters()
            : _dbContext.Rooms;

        return query
            .AnyAsync(
                r =>  r.HotelId == hotelId
                     && r.Number == number
                     && (!excludeRoomId.HasValue || r.Id != excludeRoomId.Value),
                cancellationToken);
    }

    public async Task AddAsync(Room room, CancellationToken cancellationToken)
    {
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Room room, byte[]? expectedRowVersion, CancellationToken cancellationToken)
    {
        if (expectedRowVersion is not null)
        {
            _dbContext.Entry(room)
                .Property(r => r.RowVersion)
                .OriginalValue = expectedRowVersion;
        }

        try
        {
            _dbContext.Rooms.Update(room);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("Concurrency conflict detected.", ex);
        }
    }

    public async Task AddHistoryAsync(RoomHistoryEntry entry, CancellationToken cancellationToken)
    {
        _dbContext.RoomHistoryEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<RoomHistoryEntry>> GetHistoryAsync(RoomId roomId, int page, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<RoomHistoryEntry> query = _dbContext.RoomHistoryEntries
            .AsNoTracking()
            .Where(h => h.RoomId == roomId);

        int total = await query.CountAsync(cancellationToken);
        List<RoomHistoryEntry> items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomHistoryEntry>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyCollection<RoomSummaryItem>> GetSummaryAsync(HotelId hotelId, CancellationToken cancellationToken)
    {
        var query = from room in _dbContext.Rooms.AsNoTracking()
                    join roomType in _dbContext.RoomTypes.AsNoTracking()
                        on room.RoomTypeId equals roomType.Id
                    where !room.IsDeleted
                          && !roomType.IsDeleted
                          && room.HotelId == hotelId
                    group new { room, roomType } by new { room.RoomTypeId, roomType.Name }
            into g
                    select new RoomSummaryItem(
                        g.Key.RoomTypeId.Value,
                        g.Key.Name,
                        g.Count(),
                        g.Count(x => x.room.Status == RoomStatus.Active),
                        g.Count(x => x.room.Status == RoomStatus.OutOfService),
                        g.Count(x => x.room.HousekeepingStatus == HousekeepingStatus.Dirty));

        return await query.ToListAsync(cancellationToken);
    }
}
