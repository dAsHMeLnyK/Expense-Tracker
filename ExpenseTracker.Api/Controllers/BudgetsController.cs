using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController(IBudgetRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Budget>>> GetCurrentBudgets([FromQuery] int? month, [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var budgets = await repository.GetBudgetsAsync(targetMonth, targetYear);
        return Ok(budgets);
    }

    [HttpPost]
    public async Task<ActionResult<Budget>> SetBudget(Budget budget)
    {
        try
        {
            await repository.AddAsync(budget);
            return CreatedAtAction(nameof(GetCurrentBudgets), new { id = budget.Id }, budget);
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { Message = "A budget for this category has already been set for this month." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBudget(int id, Budget budget)
    {
        if (id != budget.Id) return BadRequest();

        try
        {
            await repository.UpdateAsync(budget);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await repository.ExistsAsync(id)) return NotFound();
            throw;
        }

        return NoContent();
    }
}