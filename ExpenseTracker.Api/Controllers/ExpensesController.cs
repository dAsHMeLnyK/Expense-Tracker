using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Repositories;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(
    IExpenseRepository expenseRepository, 
    IBudgetRepository budgetRepository, 
    IBudgetService budgetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses(
        [FromQuery] int? categoryId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] PaymentMethod? method)
    {
        var expenses = await expenseRepository.GetFilteredExpensesAsync(categoryId, from, to, method);
        return Ok(expenses);
    }

    [HttpPost]
    public async Task<ActionResult> CreateExpense(Expense expense)
    {
        await expenseRepository.AddAsync(expense);

        // Перевірка бізнес-правил щодо бюджету
        var budget = await budgetRepository.GetBudgetForExpenseAsync(
            expense.CategoryId, expense.Date.Month, expense.Date.Year, expense.UserId);

        bool isNearLimit = false;
        bool isOverBudget = false;
        string? warningMessage = null;

        if (budget != null)
        {
            var totalExpenses = await expenseRepository.GetTotalExpensesForBudgetAsync(
                expense.CategoryId, expense.Date.Month, expense.Date.Year, expense.UserId);

            isNearLimit = budgetService.IsNearLimit(totalExpenses, budget.MonthlyLimit);
            isOverBudget = budgetService.IsOverLimit(totalExpenses, budget.MonthlyLimit);

            if (isOverBudget) warningMessage = "Warning: Budget limit exceeded!";
            else if (isNearLimit) warningMessage = "Warning: 80% of budget limit reached!";
        }

        var response = new { Expense = expense, IsNearLimit = isNearLimit, IsOverBudget = isOverBudget, Warning = warningMessage };
        return CreatedAtAction(nameof(GetExpenses), new { id = expense.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutExpense(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();

        try 
        { 
            await expenseRepository.UpdateAsync(expense); 
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await expenseRepository.ExistsAsync(id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await expenseRepository.GetByIdAsync(id);
        if (expense == null) return NotFound();
        
        await expenseRepository.DeleteAsync(expense);
        return NoContent();
    }
}