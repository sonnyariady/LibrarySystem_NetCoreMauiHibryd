using Library.Api.Common.Crud;

namespace Library.Api.Models;

public sealed class Category : ISearchableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Book> Books { get; set; } = new List<Book>();

    public static IReadOnlyList<string> SearchableColumns => new[] { nameof(Name), nameof(Description) };
}
