namespace Library.Api.Common.Excel;

public sealed record ExcelExportRequest(
    string? Q,
    Dictionary<string, string?>? Filters,
    string? SortBy,
    bool SortDesc,
    Dictionary<string, string>? HeaderMap,
    string? SheetName,
    string? FileName
);
