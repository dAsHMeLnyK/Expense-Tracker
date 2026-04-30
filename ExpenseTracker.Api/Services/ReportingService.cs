using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public interface IReportingService
{
    Task<IEnumerable<CategoryReportDto>> GetMonthlyReportAsync(int month, int year);
    Task<MonthlySummaryDto> GetSummaryAsync();
}

public class ReportingService(AppDbContext context) : IReportingService
{
    public async Task<IEnumerable<CategoryReportDto>> GetMonthlyReportAsync(int month, int year)
    {
        var query = await context.Categories
            .Select(c => new
            {
                CategoryName = c.Name,
                TotalAmount = c.Expenses
                    .Where(e => e.Date.Month == month && e.Date.Year == year)
                    .Sum(e => (decimal?)e.Amount) ?? 0,
                BudgetLimit = context.Budgets
                    .Where(b => b.CategoryId == c.Id && b.Month == month && b.Year == year)
                    .Select(b => (decimal?)b.MonthlyLimit)
                    .FirstOrDefault()
            })
            .Where(x => x.TotalAmount > 0 || x.BudgetLimit != null)
            .ToListAsync();

        return query.Select(x => new CategoryReportDto(
            x.CategoryName,
            x.TotalAmount,
            x.BudgetLimit,
            x.BudgetLimit.HasValue && x.TotalAmount > x.BudgetLimit.Value
        ));
    }

    public async Task<MonthlySummaryDto> GetSummaryAsync()
    {
        var hasExpenses = await context.Expenses.AnyAsync();
        if (!hasExpenses) return new MonthlySummaryDto(0, 0, "N/A");

        var total = await context.Expenses.SumAsync(e => e.Amount);
        var avg = await context.Expenses.AverageAsync(e => e.Amount);
        
        var topCat = await context.Expenses
            .GroupBy(e => e.Category!.Name)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .Select(g => g.Key)
            .FirstOrDefaultAsync() ?? "N/A";

        return new MonthlySummaryDto(total, avg, topCat);
    }
}