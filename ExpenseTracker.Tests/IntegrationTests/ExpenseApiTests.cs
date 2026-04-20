using System.Net.Http.Json;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ExpenseTracker.Tests.IntegrationTests;

public class ExpenseApiTests(ApiWebApplicationFactory factory) 
    : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private int _sharedCategoryId;

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Створюємо категорію один раз для обох тестів у класі
        var category = new Category { Name = "TestCategory", Icon = "🔍", Color = "#000" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        
        _sharedCategoryId = category.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAndFilterExpenses_ShouldWorkCorrectly()
    {
        // Arrange
        var expense = new Expense
        {
            Amount = 123.45m,
            Description = "Integration Test Expense",
            Date = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Card,
            CategoryId = _sharedCategoryId,
            UserId = 1
        };

        // Act
        var postResponse = await _client.PostAsJsonAsync("/api/expenses", expense);
        postResponse.EnsureSuccessStatusCode();

        // Assert
        var getResponse = await _client.GetAsync($"/api/expenses?categoryId={_sharedCategoryId}");
        var expenses = await getResponse.Content.ReadFromJsonAsync<List<Expense>>();

        expenses.ShouldNotBeNull();
        expenses.Any(e => e.Description == "Integration Test Expense").ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteExpense_ShouldRemoveFromDatabase()
    {
        // Arrange
        var expense = new Expense 
        { 
            Amount = 10.00m, 
            Description = "To Delete", 
            Date = DateTime.UtcNow, 
            PaymentMethod = PaymentMethod.Cash,
            CategoryId = _sharedCategoryId, 
            UserId = 1 
        };
        
        var postRes = await _client.PostAsJsonAsync("/api/expenses", expense);
        var savedExpense = await postRes.Content.ReadFromJsonAsync<Expense>();

        // Act
        var delRes = await _client.DeleteAsync($"/api/expenses/{savedExpense!.Id}");

        // Assert
        delRes.EnsureSuccessStatusCode();
    }
}