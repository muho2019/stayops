namespace StayOps.Api.Contracts;

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Total,
    int Page,
    int PageSize);
