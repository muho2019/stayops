namespace StayOps.Application.Abstractions;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Total,
    int Page,
    int PageSize);
