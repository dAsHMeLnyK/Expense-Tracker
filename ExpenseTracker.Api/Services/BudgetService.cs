namespace ExpenseTracker.Api.Services;

public interface IBudgetService
{
    bool IsNearLimit(decimal currentAmount, decimal limit);
    bool IsOverLimit(decimal currentAmount, decimal limit);
}

public class BudgetService : IBudgetService
{
    public bool IsNearLimit(decimal currentAmount, decimal limit) => 
        limit > 0 && currentAmount >= limit * 0.8m;

    public bool IsOverLimit(decimal currentAmount, decimal limit) => 
        currentAmount > limit;
}