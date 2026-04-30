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
    [InlineData(80, 100, true)]
    [InlineData(85, 100, true)]
    [InlineData(79, 100, false)]
    [InlineData(120, 100, true)]
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