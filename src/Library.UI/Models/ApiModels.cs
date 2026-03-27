namespace Library.UI.Models;

public sealed record ApiResponseGeneral<T>(bool Success, string? Message, T? Data);

public sealed record PagedResultGeneral<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

public sealed record CrudSearchRequest(
    string? Q,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDesc = false,
    Dictionary<string, string?>? Filters = null);

public sealed record ExcelExportRequest(
    string? Q,
    Dictionary<string, string?>? Filters,
    string? SortBy,
    bool SortDesc,
    Dictionary<string, string>? HeaderMap,
    string? SheetName,
    string? FileName);

public sealed class ExportFileResult
{
    public string FileName { get; set; } = "export.xlsx";
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
}

public sealed class AuthorVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CategoryVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Isbn { get; set; } = "";
    public string? Publisher { get; set; }
    public int PublishYear { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class MemberVm
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class LoanVm
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "Borrowed";
    public string? Notes { get; set; }
}
