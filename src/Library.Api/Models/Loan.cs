namespace Library.Api.Models;

public sealed class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "Borrowed";
    public string? Notes { get; set; }

    public Book? Book { get; set; }
    public Member? Member { get; set; }
}
