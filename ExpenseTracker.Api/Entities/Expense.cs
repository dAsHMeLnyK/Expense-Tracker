namespace ExpenseTracker.Api.Entities;

public enum PaymentMethod { Cash, Card, Transfer }
public class Expense
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; } // Бізнес-правило: > 0
    public string Description { get; set; } // AutoFixture
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime Date { get; set; } // Бізнес-правило: не майбутнє
    public PaymentMethod PaymentMethod { get; set; }
}