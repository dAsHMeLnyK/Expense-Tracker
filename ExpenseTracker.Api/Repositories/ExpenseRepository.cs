using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Repositories;

public class ExpenseRepository(AppDbContext context) : IExpenseRepository
{
    public async Task<IEnumerable<Expense>> GetFilteredExpensesAsync(int? categoryId, DateTime? from, DateTime? to, PaymentMethod? method)
    {
        var query = context.Expenses.AsQueryable();

        if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId);
        if (from.HasValue) query = query.Where(e => e.Date >= from);
        if (to.HasValue) query = query.Where(e => e.Date <= to);
        if (method.HasValue) query = query.Where(e => e.PaymentMethod == method);

        return await query.ToListAsync();
    }

    public async Task<Expense?> GetByIdAsync(int id) => 
        await context.Expenses.FindAsync(id);

    public async Task<decimal> GetTotalExpensesForBudgetAsync(int categoryId, int month, int year, int userId)
    {
        return await context.Expenses
            .Where(e => e.CategoryId == categoryId && e.Date.Month == month && e.Date.Year == year && e.UserId == userId)
            .SumAsync(e => e.Amount);
    }

    public async Task AddAsync(Expense expense)
    {
        context.Expenses.Add(expense);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Expense expense)
    {
        context.Entry(expense).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Expense expense)
    {
        context.Expenses.Remove(expense);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) => 
        await context.Expenses.AnyAsync(e => e.Id == id);
}