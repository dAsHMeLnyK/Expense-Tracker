using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Repositories;

public class BudgetRepository(AppDbContext context) : IBudgetRepository
{
    public async Task<IEnumerable<Budget>> GetBudgetsAsync(int month, int year)
    {
        return await context.Budgets
            .Where(b => b.Month == month && b.Year == year)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int id) => 
        await context.Budgets.FindAsync(id);

    public async Task<Budget?> GetBudgetForExpenseAsync(int categoryId, int month, int year, int userId)
    {
        return await context.Budgets.FirstOrDefaultAsync(b => 
            b.CategoryId == categoryId && 
            b.Month == month && 
            b.Year == year && 
            b.UserId == userId);
    }

    public async Task AddAsync(Budget budget)
    {
        context.Budgets.Add(budget);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Budget budget)
    {
        context.Entry(budget).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) => 
        await context.Budgets.AnyAsync(b => b.Id == id);
}