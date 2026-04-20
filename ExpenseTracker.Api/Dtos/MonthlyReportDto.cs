namespace ExpenseTracker.Api.Dtos;

public record CategoryReportDto(
    string CategoryName, 
    decimal TotalAmount, 
    decimal? BudgetLimit,     // Додано
    bool IsOverBudget         // Додано (прапорець)
);

public record MonthlySummaryDto(
    decimal TotalAmount, 
    decimal AverageExpense, 
    string TopCategory
);