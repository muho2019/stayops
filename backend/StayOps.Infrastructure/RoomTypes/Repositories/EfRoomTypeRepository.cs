using Microsoft.EntityFrameworkCore;
using StayOps.Application.Abstractions;
using StayOps.Application.RoomTypes.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Domain.RoomTypes;
using StayOps.Infrastructure.Data;

namespace StayOps.Infrastructure.RoomTypes.Repositories;

public sealed class EfRoomTypeRepository : IRoomTypeRepository
{
    private readonly StayOpsDbContext _dbContext;

    public EfRoomTypeRepository(StayOpsDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<RoomType?> GetByIdAsync(RoomTypeId id, CancellationToken cancellationToken, bool includeDeleted = false)
    {
        IQueryable<RoomType> query = includeDeleted
            ? _dbContext.RoomTypes.IgnoreQueryFilters()
            : _dbContext.RoomTypes;

        return _dbContext.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<PagedResult<RoomType>> GetByHotelAsync(
        HotelId hotelId,
        RoomTypeStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        bool includeDeleted = false)
    {
        IQueryable<RoomType> query = includeDeleted
            ? _dbContext.RoomTypes.IgnoreQueryFilters()
            : _dbContext.RoomTypes;

        query = query
            .AsNoTracking()
            .Where(rt => !rt.IsDeleted && rt.HotelId == hotelId);

        if (status.HasValue)
        {
            query = query.Where(rt => rt.Status == status.Value);
        }

        int total = await query.CountAsync(cancellationToken);

        List<RoomType> items = await query
            .OrderBy(rt => rt.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomType>(items, total, page, pageSize);
    }

    public Task<bool> NameExistsAsync(
        HotelId hotelId,
        string name,
        CancellationToken cancellationToken,
        RoomTypeId? excludeRoomTypeId = null,
        bool includeDeleted = false)
    {
        string normalizedName = name.Trim();

        IQueryable<RoomType> query = includeDeleted
            ? _dbContext.RoomTypes.IgnoreQueryFilters()
            : _dbContext.RoomTypes;

        query = query.Where(rt => rt.HotelId == hotelId && rt.Name == normalizedName);

        if (excludeRoomTypeId is not null)
        {
            query = query.Where(rt => rt.Id != excludeRoomTypeId);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(RoomType roomType, CancellationToken cancellationToken)
    {
        _dbContext.RoomTypes.Add(roomType);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RoomType roomType, CancellationToken cancellationToken)
    {
        _dbContext.RoomTypes.Update(roomType);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
