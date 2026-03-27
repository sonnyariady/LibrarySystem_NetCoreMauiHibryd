using Library.Api.Common.Crud;

namespace Library.Api.Models;

public sealed class Member : ISearchableEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public static IReadOnlyList<string> SearchableColumns => new[] { nameof(Code), nameof(FullName), nameof(Email), nameof(Phone) };
}
