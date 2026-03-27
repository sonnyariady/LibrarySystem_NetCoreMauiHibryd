using Library.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>(e =>
        {
            e.ToTable("Authors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Country).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Book>(e =>
        {
            e.ToTable("Books");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Isbn).HasMaxLength(30).IsRequired();
            e.Property(x => x.Publisher).HasMaxLength(150);
            e.HasIndex(x => x.Isbn).IsUnique();
            e.HasOne(x => x.Author).WithMany(x => x.Books).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Books).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Member>(e =>
        {
            e.ToTable("Members");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Loan>(e =>
        {
            e.ToTable("Loans");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Book).WithMany(x => x.Loans).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Member).WithMany(x => x.Loans).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
