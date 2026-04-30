namespace ExpenseTracker.Api.Dtos;

public record CategoryReportDto(
    string CategoryName, 
    decimal TotalAmount, 
    decimal? BudgetLimit,
    bool IsOverBudget
);

public record MonthlySummaryDto(
    decimal TotalAmount, 
    decimal AverageExpense, 
    string TopCategory
);