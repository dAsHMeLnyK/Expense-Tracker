using ExpenseTracker.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Додано для IHost
using Testcontainers.PostgreSql;
using Xunit;

namespace ExpenseTracker.Tests.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    // Цей метод — ключ до вирішення проблеми в GitHub Actions (Linux)
    // Він змушує фабрику шукати ресурси в поточній папці виконання
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        return base.CreateHost(builder);
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Вказуємо середовище Testing
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Видаляємо існуючу реєстрацію DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Підключаємося до динамічного контейнера Testcontainers
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }
}