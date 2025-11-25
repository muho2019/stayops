using StayOps.Domain.Hotels;

namespace StayOps.Application.Hotels.Commands;

public sealed record ListHotelsQuery(
    HotelStatus? Status,
    int Page = 1,
    int PageSize = 50);
