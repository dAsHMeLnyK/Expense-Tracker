using FluentValidation;
using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Api.Validation;
public class ExpenseValidator : AbstractValidator<Expense>
{
    public ExpenseValidator()
    {
        RuleFor(e => e.Amount)
            .GreaterThan(0)
            .WithMessage("Expense amount must be greater than zero.");

        RuleFor(e => e.Date)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Expense date cannot be in the future.");

        RuleFor(e => e.Description)
            .NotEmpty()
            .MaximumLength(250)
            .WithMessage("Description is required and must be under 250 characters.");
    }
}