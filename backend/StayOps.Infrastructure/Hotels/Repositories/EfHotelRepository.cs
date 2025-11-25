using Microsoft.EntityFrameworkCore;
using StayOps.Application.Abstractions;
using StayOps.Application.Hotels.Abstractions;
using StayOps.Domain.Hotels;
using StayOps.Infrastructure.Data;

namespace StayOps.Infrastructure.Hotels.Repositories;

public sealed class EfHotelRepository : IHotelRepository
{
    private readonly StayOpsDbContext _dbContext;

    public EfHotelRepository(StayOpsDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Hotel?> GetByIdAsync(HotelId id, CancellationToken cancellationToken)
    {
        return _dbContext.Hotels
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken, HotelId? excludeHotelId = null)
    {
        return _dbContext.Hotels
            .AnyAsync(
                h => !h.IsDeleted
                     && h.Code == code
                     && (!excludeHotelId.HasValue || h.Id != excludeHotelId.Value),
                cancellationToken);
    }

    public async Task<PagedResult<Hotel>> GetPagedAsync(HotelStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<Hotel> query = _dbContext.Hotels
            .AsNoTracking()
            .Where(h => !h.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(h => h.Status == status.Value);
        }

        int total = await query.CountAsync(cancellationToken);

        List<Hotel> items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Hotel>(items, total, page, pageSize);
    }

    public async Task AddAsync(Hotel hotel, CancellationToken cancellationToken)
    {
        _dbContext.Hotels.Add(hotel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken)
    {
        _dbContext.Hotels.Update(hotel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
