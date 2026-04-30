namespace ExpenseTracker.Api.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }
    public List<Expense> Expenses { get; set; } = new();
    public List<Budget> Budgets { get; set; } = new();
}