using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ExpenseTracker.Tests.DatabaseTests;

public class ExpenseDatabaseTests(PostgreSqlFixture fixture) 
    : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private AppDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(fixture.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
        
        // Створюємо схему в реальному Postgres
        await _context.Database.EnsureCreatedAsync();
        
        // Очищуємо дані перед кожним тестом
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Expenses\", \"Budgets\", \"Categories\" CASCADE");
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task Budget_UniqueConstraint_ShouldThrowExceptionOnDuplicate()
    {
        // Arrange
        var category = new Category { Name = "Food", Icon = "🍔", Color = "Red" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget1 = new Budget 
        { 
            UserId = 1, CategoryId = category.Id, Month = 5, Year = 2026, MonthlyLimit = 1000 
        };
        await _context.Budgets.AddAsync(budget1);
        await _context.SaveChangesAsync();

        // Act & Assert
        var budget2 = new Budget 
        { 
            UserId = 1, CategoryId = category.Id, Month = 5, Year = 2026, MonthlyLimit = 2000 
        };

        // Перевіряємо, що реальний Postgres викине помилку унікальності (наш Index в OnModelCreating)
        await Should.ThrowAsync<DbUpdateException>(async () => 
        {
            await _context.Budgets.AddAsync(budget2);
            await _context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task Category_Delete_ShouldCascadeDeleteExpenses()
    {
        // Arrange
        var category = new Category { Name = "Transport", Icon = "🚗", Color = "Blue" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _context.Expenses.Add(new Expense 
        { 
            Amount = 50, 
            CategoryId = category.Id, 
            UserId = 1, 
            Date = DateTime.UtcNow, // ЗМІНЕНО: тепер UtcNow замість Now
            Description = "Taxi", 
            PaymentMethod = PaymentMethod.Card 
        });
        await _context.SaveChangesAsync();

        // Act
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        // Assert
        var expensesCount = await _context.Expenses.CountAsync();
        expensesCount.ShouldBe(0); 
    }
    
    [Fact]
    public async Task GetMonthlyReport_ShouldReturnCorrectAggregatedSums()
    {
        // Arrange
        var category = new Category { Name = "Food", Icon = "🍔", Color = "Red" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Додаємо кілька витрат в одну категорію
        _context.Expenses.AddRange(
            new Expense { Amount = 150.50m, CategoryId = category.Id, UserId = 1, Date = DateTime.UtcNow, Description = "Lunch", PaymentMethod = PaymentMethod.Card },
            new Expense { Amount = 49.50m, CategoryId = category.Id, UserId = 1, Date = DateTime.UtcNow, Description = "Coffee", PaymentMethod = PaymentMethod.Cash }
        );
        await _context.SaveChangesAsync();

        // Act
        var report = await _context.Expenses
            .Where(e => e.CategoryId == category.Id)
            .GroupBy(e => e.Category.Name)
            .Select(g => new { Name = g.Key, Total = g.Sum(x => x.Amount) })
            .FirstOrDefaultAsync();

        // Assert
        report.ShouldNotBeNull();
        report.Name.ShouldBe("Food");
        report.Total.ShouldBe(200.00m);
    }

    [Fact]
    public async Task Budget_ShouldAllowDifferentUsersToHaveBudgetsForSameCategory()
    {
        // Arrange
        var category = new Category { Name = "Rent", Icon = "🏠", Color = "Blue" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Юзер 1 ставить бюджет
        _context.Budgets.Add(new Budget 
        { 
            UserId = 1, CategoryId = category.Id, Month = 6, Year = 2026, MonthlyLimit = 1000 
        });

        // Юзер 2 ТАКОЖ ставить бюджет на ту саму категорію і той самий місяць
        _context.Budgets.Add(new Budget 
        { 
            UserId = 2, CategoryId = category.Id, Month = 6, Year = 2026, MonthlyLimit = 1500 
        });

        // Act
        var exception = await Record.ExceptionAsync(async () => await _context.SaveChangesAsync());

        // Assert
        exception.ShouldBeNull(); // Помилки не має бути, бо UserId різні
        var count = await _context.Budgets.CountAsync();
        count.ShouldBe(2);
    }
}