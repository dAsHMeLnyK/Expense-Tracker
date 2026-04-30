using AutoFixture;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Validation; // Переконайтеся, що цей namespace вірний
using FluentAssertions;
using Xunit;

namespace ExpenseTracker.Tests.UnitTests;

public class ExpenseValidationTests
{
    private readonly Fixture _fixture;
    private readonly ExpenseValidator _validator;

    public ExpenseValidationTests()
    {
        _fixture = new Fixture();
        
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _validator = new ExpenseValidator();
    }

    [Fact]
    public void Expense_WithNegativeAmount_ShouldHaveError()
    {
        // Arrange
        var expense = _fixture.Build<Expense>()
            .With(e => e.Amount, -5)
            .Create();

        // Act
        var result = _validator.Validate(expense);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Expense amount must be greater than zero.");
    }

    [Fact]
    public void Expense_InFuture_ShouldHaveError()
    {
        // Arrange
        var expense = _fixture.Build<Expense>()
            .With(e => e.Date, DateTime.Now.AddDays(1))
            .Create();

        // Act
        var result = _validator.Validate(expense);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Date");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Expense date cannot be in the future.");
    }
}