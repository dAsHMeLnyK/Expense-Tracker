using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportingService reportingService) : ControllerBase
{
    [HttpGet("monthly")]
    public async Task<ActionResult<IEnumerable<CategoryReportDto>>> GetMonthlyReport(int month, int year)
    {
        var report = await reportingService.GetMonthlyReportAsync(month, year);
        return Ok(report);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<MonthlySummaryDto>> GetSummary()
    {
        var summary = await reportingService.GetSummaryAsync();
        return Ok(summary);
    }
}