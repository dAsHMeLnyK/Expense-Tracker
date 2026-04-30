using Bogus;
using ExpenseTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;
        
        var k6Category = new Category { Name = "Stress Test Category", Icon = "🔥", Color = "#FF0000" };
        context.Categories.Add(k6Category);
        await context.SaveChangesAsync();
        
        var categoryFaker = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0] + f.IndexGlobal)
            .RuleFor(c => c.Icon, f => "📊")
            .RuleFor(c => c.Color, f => f.Internet.Color());

        var categories = categoryFaker.Generate(19);
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var allCategories = await context.Categories.ToListAsync();
        
        var budgetFaker = new Faker<Budget>()
            .RuleFor(b => b.UserId, f => f.Random.Int(1, 10))
            .RuleFor(b => b.CategoryId, f => f.PickRandom(allCategories).Id)
            .RuleFor(b => b.MonthlyLimit, f => f.Finance.Amount(500, 2000))
            .RuleFor(b => b.Month, f => f.Date.Past(1).Month)
            .RuleFor(b => b.Year, f => f.Date.Past(1).Year)
            .RuleFor(b => b.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var budgets = budgetFaker.Generate(200);
        
        var uniqueBudgets = budgets
            .GroupBy(b => new { b.UserId, b.CategoryId, b.Month, b.Year })
            .Select(g => g.First())
            .ToList();
            
        context.Budgets.AddRange(uniqueBudgets);
        await context.SaveChangesAsync();

        var expenseFaker = new Faker<Expense>()
            .RuleFor(e => e.UserId, f => f.Random.Int(1, 10))
            .RuleFor(e => e.Amount, f => f.Finance.Amount(1, 500))
            .RuleFor(e => e.Description, f => f.Lorem.Sentence(3))
            .RuleFor(e => e.CategoryId, f => f.PickRandom(allCategories).Id)
            .RuleFor(e => e.Date, f => f.Date.Recent(60).ToUniversalTime()) 
            .RuleFor(e => e.PaymentMethod, f => f.PickRandom<PaymentMethod>());

        var expenses = expenseFaker.Generate(10000);
        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();
    }
}