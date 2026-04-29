using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Api.Repositories;

public interface IBudgetRepository
{
    Task<IEnumerable<Budget>> GetBudgetsAsync(int month, int year);
    Task<Budget?> GetByIdAsync(int id);
    Task<Budget?> GetBudgetForExpenseAsync(int categoryId, int month, int year, int userId);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task<bool> ExistsAsync(int id);
}