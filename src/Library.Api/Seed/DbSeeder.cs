using Library.Api.Data;
using Library.Api.Models;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Authors.AnyAsync()) return;

        var authors = new[]
        {
            new Author { Name = "Robert C. Martin", Country = "USA" },
            new Author { Name = "Martin Fowler", Country = "UK" },
            new Author { Name = "Andrew Hunt", Country = "USA" }
        };

        var categories = new[]
        {
            new Category { Name = "Software Engineering", Description = "Buku rekayasa perangkat lunak" },
            new Category { Name = "Architecture", Description = "Buku arsitektur aplikasi" },
            new Category { Name = "Programming", Description = "Buku bahasa pemrograman" }
        };

        db.Authors.AddRange(authors);
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var books = new[]
        {
            new Book { Title = "Clean Code", Isbn = "9780132350884", AuthorId = authors[0].Id, CategoryId = categories[0].Id, Publisher = "Prentice Hall", PublishYear = 2008, Stock = 3, IsAvailable = true },
            new Book { Title = "Refactoring", Isbn = "9780134757599", AuthorId = authors[1].Id, CategoryId = categories[1].Id, Publisher = "Addison-Wesley", PublishYear = 2018, Stock = 5, IsAvailable = true },
            new Book { Title = "The Pragmatic Programmer", Isbn = "9780135957059", AuthorId = authors[2].Id, CategoryId = categories[2].Id, Publisher = "Addison-Wesley", PublishYear = 2019, Stock = 4, IsAvailable = true }
        };

        var members = new[]
        {
            new Member { Code = "MBR-001", FullName = "Budi Santoso", Email = "budi@example.com", Phone = "08123456789", Address = "Jakarta", IsActive = true },
            new Member { Code = "MBR-002", FullName = "Siti Aminah", Email = "siti@example.com", Phone = "08234567890", Address = "Bandung", IsActive = true }
        };

        db.Books.AddRange(books);
        db.Members.AddRange(members);
        await db.SaveChangesAsync();
    }
}
