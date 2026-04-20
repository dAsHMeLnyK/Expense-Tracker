using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTracker.Tests.UnitTests;

public class ReportingServiceTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetSummary_ShouldCalculateCorrectTotals()
    {
        // Arrange
        var context = GetDatabaseContext();
        var category = new Category 
        { 
            Id = 1, 
            Name = "Food", 
            Icon = "fast-food-outline",
            Color = "#FF0000" 
        };
        context.Expenses.AddRange(
            new Expense { Amount = 100, Category = category, Date = DateTime.Now, Description = "Test 1", UserId = 1 },
            new Expense { Amount = 200, Category = category, Date = DateTime.Now, Description = "Test 2", UserId = 1 }
        );
        await context.SaveChangesAsync();

        var service = new ReportingService(context);

        // Act
        var result = await service.GetSummaryAsync();

        // Assert
        result.TotalAmount.Should().Be(300);
        result.AverageExpense.Should().Be(150);
        result.TopCategory.Should().Be("Food");
    }
}