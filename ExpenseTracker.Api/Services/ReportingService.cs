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
        var expenses = await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date.Month == month && e.Date.Year == year)
            .GroupBy(e => e.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        var budgets = await context.Budgets
            .Where(b => b.Month == month && b.Year == year)
            .ToListAsync();

        return expenses.Select(e =>
        {
            var budget = budgets.FirstOrDefault(b => b.CategoryId == e.Category.Id);
            var limit = budget?.MonthlyLimit ?? 0;
            return new CategoryReportDto(
                e.Category.Name, 
                e.Total, 
                limit > 0 ? limit : null,
                limit > 0 && e.Total > limit
            );
        });
    }

    public async Task<MonthlySummaryDto> GetSummaryAsync()
    {
        var expenses = await context.Expenses.Include(e => e.Category).ToListAsync();
        if (!expenses.Any()) return new MonthlySummaryDto(0, 0, "N/A");

        var total = expenses.Sum(e => e.Amount);
        var avg = expenses.Average(e => e.Amount);
        var topCat = expenses
            .GroupBy(e => e.Category.Name)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .First().Key;

        return new MonthlySummaryDto(total, avg, topCat);
    }
}