using FluentValidation;
using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Api.Validation;

public class BudgetValidator : AbstractValidator<Budget>
{
    public BudgetValidator()
    {
        RuleFor(b => b.MonthlyLimit)
            .GreaterThan(0)
            .WithMessage("Monthly limit must be a positive number.");

        RuleFor(b => b.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12.");

        RuleFor(b => b.Year)
            .GreaterThanOrEqualTo(2020)
            .WithMessage("Year must be 2020 or later.");
    }
}