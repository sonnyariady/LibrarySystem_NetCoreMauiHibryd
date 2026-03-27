namespace Library.Api.Common.Api;

public sealed record PagedRequestGeneral(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDesc = false
);

public sealed record PagedResultGeneral<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public sealed record ApiResponseGeneral<T>(
    bool Success,
    string? Message,
    T? Data
)
{
    public static ApiResponseGeneral<T> Ok(T data, string? message = null) => new(true, message, data);
    public static ApiResponseGeneral<T> Fail(string message) => new(false, message, default);
}
