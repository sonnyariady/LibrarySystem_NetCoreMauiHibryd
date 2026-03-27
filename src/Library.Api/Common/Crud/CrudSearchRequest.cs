namespace Library.Api.Common.Crud;

public sealed record CrudSearchRequest(
    string? Q,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDesc = false,
    Dictionary<string, string?>? Filters = null
);
