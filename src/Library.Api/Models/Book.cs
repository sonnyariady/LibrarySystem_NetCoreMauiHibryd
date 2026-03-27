using Library.Api.Common.Crud;

namespace Library.Api.Models;

public sealed class Book : ISearchableEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int PublishYear { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Author? Author { get; set; }
    public Category? Category { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public static IReadOnlyList<string> SearchableColumns => new[] { nameof(Title), nameof(Isbn), nameof(Publisher) };
}
