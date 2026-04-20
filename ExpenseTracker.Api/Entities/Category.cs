namespace ExpenseTracker.Api.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }  // AutoFixture
    public string Icon { get; set; }  // AutoFixture
    public string Color { get; set; } // AutoFixture
    public List<Expense> Expenses { get; set; } = new();
    public List<Budget> Budgets { get; set; } = new();
}