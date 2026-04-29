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
    private readonly ApiWebApplicationFactory _factory = factory;
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
        postRes.EnsureSuccessStatusCode();

        // ВИПРАВЛЕНО: Читаємо JSON вузол і дістаємо ID з вкладеного об'єкта expense
        var responseNode = await postRes.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonNode>();
        var savedExpenseId = responseNode!["expense"]!["id"]!.GetValue<int>();

        // Act
        var delRes = await _client.DeleteAsync($"/api/expenses/{savedExpenseId}");

        // Assert
        delRes.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task CreateExpense_ShouldReturnWarning_WhenOver80PercentOfBudget()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var category = new Category { Name = "Limits", Icon = "📈", Color = "Red" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var budget = new Budget { UserId = 1, CategoryId = category.Id, MonthlyLimit = 100, Month = DateTime.UtcNow.Month, Year = DateTime.UtcNow.Year };
        db.Budgets.Add(budget);
        await db.SaveChangesAsync();

        var expense = new Expense 
        { 
            Amount = 85, // 85% від 100
            Description = "Test 80 percent limit", Date = DateTime.UtcNow, PaymentMethod = PaymentMethod.Card, CategoryId = category.Id, UserId = 1 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/expenses", expense);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Оскільки ми повертаємо анонімний об'єкт, читаємо його як JsonNode або Dictionary
        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonNode>();
        result.ShouldNotBeNull();
        
        var isNearLimit = result!["isNearLimit"]?.GetValue<bool>() ?? false;
        var warning = result["warning"]?.GetValue<string>();

        isNearLimit.ShouldBeTrue();
        warning.ShouldNotBeNull();
        warning.ShouldContain("80%");
    }
    
    [Fact]
    public async Task UpdateExpense_ShouldModifyExistingRecord()
    {
        // Arrange: Створюємо початковий запис
        var expense = new Expense 
        { 
            Amount = 50.00m, 
            Description = "Initial Description", 
            Date = DateTime.UtcNow, 
            PaymentMethod = PaymentMethod.Card,
            CategoryId = _sharedCategoryId, 
            UserId = 1 
        };
    
        var postRes = await _client.PostAsJsonAsync("/api/expenses", expense);
        postRes.EnsureSuccessStatusCode();
    
        var responseNode = await postRes.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonNode>();
        var savedExpenseId = responseNode!["expense"]!["id"]!.GetValue<int>();

        // Підготовлюємо об'єкт для оновлення (обов'язково передаємо правильний Id)
        var updatedExpense = new Expense 
        { 
            Id = savedExpenseId,
            Amount = 75.00m, 
            Description = "Updated Description", 
            Date = DateTime.UtcNow, 
            PaymentMethod = PaymentMethod.Cash,
            CategoryId = _sharedCategoryId, 
            UserId = 1 
        };

        // Act: Виконуємо PUT запит
        var putRes = await _client.PutAsJsonAsync($"/api/expenses/{savedExpenseId}", updatedExpense);

        // Assert: Перевіряємо статус та те, що дані дійсно змінилися
        putRes.EnsureSuccessStatusCode();

        var getRes = await _client.GetAsync($"/api/expenses?categoryId={_sharedCategoryId}");
        var expenses = await getRes.Content.ReadFromJsonAsync<List<Expense>>();
        var modifiedRecord = expenses!.FirstOrDefault(e => e.Id == savedExpenseId);

        modifiedRecord.ShouldNotBeNull();
        modifiedRecord.Amount.ShouldBe(75.00m);
        modifiedRecord.Description.ShouldBe("Updated Description");
        modifiedRecord.PaymentMethod.ShouldBe(PaymentMethod.Cash);
    }
}