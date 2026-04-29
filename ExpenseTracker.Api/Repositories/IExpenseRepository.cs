using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Api.Repositories;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetFilteredExpensesAsync(int? categoryId, DateTime? from, DateTime? to, PaymentMethod? method);
    Task<Expense?> GetByIdAsync(int id);
    Task<decimal> GetTotalExpensesForBudgetAsync(int categoryId, int month, int year, int userId);
    Task AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(Expense expense);
    Task<bool> ExistsAsync(int id);
}