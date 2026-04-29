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

        // ВИПРАВЛЕНО: динамічне обчислення часу
        RuleFor(e => e.Date)
            .Must(date => date <= DateTime.UtcNow)
            .WithMessage("Expense date cannot be in the future.");

        RuleFor(e => e.Description)
            .NotEmpty()
            .MaximumLength(250)
            .WithMessage("Description is required and must be under 250 characters.");
    }
}