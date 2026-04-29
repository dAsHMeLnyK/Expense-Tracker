using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTracker.Tests.DatabaseTests;

public class ReportingServiceDatabaseTests(PostgreSqlFixture fixture) 
    : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private AppDbContext _context = null!;
    private ReportingService _service = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(fixture.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Expenses\", \"Budgets\", \"Categories\" CASCADE");
        
        _service = new ReportingService(_context);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task GetSummary_ShouldCalculateCorrectTotals()
    {
        // Arrange
        var category = new Category { Name = "Food", Icon = "🍔", Color = "#FF0000" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _context.Expenses.AddRange(
            new Expense { Amount = 100, CategoryId = category.Id, Date = DateTime.UtcNow, Description = "Test 1", UserId = 1, PaymentMethod = PaymentMethod.Card },
            new Expense { Amount = 200, CategoryId = category.Id, Date = DateTime.UtcNow, Description = "Test 2", UserId = 1, PaymentMethod = PaymentMethod.Cash }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSummaryAsync();

        // Assert
        result.TotalAmount.Should().Be(300);
        result.AverageExpense.Should().Be(150);
        result.TopCategory.Should().Be("Food");
    }
}