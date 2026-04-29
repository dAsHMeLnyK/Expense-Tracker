using Bogus;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Tests.IntegrationTests;

public static class DataSeeder
{
    public static async Task SeedData(AppDbContext context)
    {
        // 1. Створюємо категорії
        var categoryFaker = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0] + f.IndexGlobal)
            .RuleFor(c => c.Icon, f => "🍔")
            .RuleFor(c => c.Color, f => "#FF0000");

        var categories = categoryFaker.Generate(20);
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // 2. Створюємо бюджети
        var budgetFaker = new Faker<Budget>()
            .RuleFor(b => b.UserId, f => f.Random.Int(1, 10))
            .RuleFor(b => b.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(b => b.MonthlyLimit, f => f.Finance.Amount(500, 2000))
            .RuleFor(b => b.Month, f => f.Date.Past(1).Month)
            .RuleFor(b => b.Year, f => f.Date.Past(1).Year)
            .RuleFor(b => b.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var budgets = budgetFaker.Generate(200);
        
        // Унікалізуємо бюджети, щоб не порушити Constraints БД
        var uniqueBudgets = budgets
            .GroupBy(b => new { b.UserId, b.CategoryId, b.Month, b.Year })
            .Select(g => g.First())
            .ToList();
            
        context.Budgets.AddRange(uniqueBudgets);
        await context.SaveChangesAsync();

        // 3. Створюємо 10 000 витрат
        var expenseFaker = new Faker<Expense>()
            .RuleFor(e => e.UserId, f => f.Random.Int(1, 10))
            .RuleFor(e => e.Amount, f => f.Finance.Amount(1, 500))
            .RuleFor(e => e.Description, f => f.Lorem.Sentence(3))
            .RuleFor(e => e.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(e => e.Date, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(e => e.PaymentMethod, f => f.PickRandom<PaymentMethod>());

        var expenses = expenseFaker.Generate(10000);
        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();
    }
}