using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses(
        [FromQuery] int? categoryId, 
        [FromQuery] DateTime? from, 
        [FromQuery] DateTime? to,
        [FromQuery] PaymentMethod? method) // Додано фільтр за методом оплати
    {
        var query = context.Expenses.AsQueryable();

        if (categoryId.HasValue) 
            query = query.Where(e => e.CategoryId == categoryId);
        if (from.HasValue) 
            query = query.Where(e => e.Date >= from);
        if (to.HasValue) 
            query = query.Where(e => e.Date <= to);
        if (method.HasValue)
            query = query.Where(e => e.PaymentMethod == method);

        return await query.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        context.Expenses.Add(expense);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExpenses), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutExpense(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();

        context.Entry(expense).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Expenses.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await context.Expenses.FindAsync(id);
        if (expense == null) return NotFound();

        context.Expenses.Remove(expense);
        await context.SaveChangesAsync();

        return NoContent();
    }
}