using ExpenseTracker.Api.Services;
using FluentAssertions;
using Xunit;

namespace ExpenseTracker.Tests.UnitTests;

public class BudgetServiceTests
{
    private readonly BudgetService _budgetService;

    public BudgetServiceTests()
    {
        _budgetService = new BudgetService();
    }

    [Theory]
    [InlineData(80, 100, true)]  // Рівно 80%
    [InlineData(85, 100, true)]  // Більше 80%
    [InlineData(79, 100, false)] // Менше 80%
    [InlineData(120, 100, true)] // Перевищення 100% також є "близько до ліміту"
    public void IsNearLimit_ShouldReturnExpectedResult(decimal current, decimal limit, bool expected)
    {
        // Act
        var result = _budgetService.IsNearLimit(current, limit);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsOverLimit_ShouldReturnTrue_WhenCurrentIsGreater()
    {
        // Act
        var result = _budgetService.IsOverLimit(101, 100);

        // Assert
        result.Should().BeTrue();
    }
}