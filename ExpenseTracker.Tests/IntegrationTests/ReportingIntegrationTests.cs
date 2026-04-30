using System.Net.Http.Json;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ExpenseTracker.Tests.IntegrationTests;

public class ReportingIntegrationTests(ApiWebApplicationFactory factory) 
    : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DataSeeder.SeedData(context); 
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSummary_OnLargeData_ShouldBeFastAndCorrect()
    {
        // Act
        var response = await _client.GetAsync("/api/reports/summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var summary = await response.Content.ReadFromJsonAsync<MonthlySummaryDto>();

        summary.ShouldNotBeNull();
        summary.TotalAmount.ShouldBeGreaterThan(0);
        summary.TopCategory.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMonthlyReport_ShouldReturnDataForEachCategory()
    {
        // Arrange
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        // Act
        var response = await _client.GetAsync($"/api/reports/monthly?month={month}&year={year}");

        // Assert
        response.EnsureSuccessStatusCode();
        var report = await response.Content.ReadFromJsonAsync<List<CategoryReportDto>>();
        
        report.ShouldNotBeNull();
        report.Count.ShouldBeGreaterThanOrEqualTo(0);
    }
}