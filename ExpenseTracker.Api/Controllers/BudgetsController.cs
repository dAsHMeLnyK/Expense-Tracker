using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController(AppDbContext context, IBudgetService budgetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Budget>>> GetCurrentBudgets()
    {
        var now = DateTime.UtcNow;
        return await context.Budgets
            .Where(b => b.Month == now.Month && b.Year == now.Year)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Budget>> SetBudget(Budget budget)
    {
        context.Budgets.Add(budget);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCurrentBudgets), new { id = budget.Id }, budget);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBudget(int id, Budget budget)
    {
        if (id != budget.Id) return BadRequest();

        context.Entry(budget).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Budgets.Any(b => b.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }
}