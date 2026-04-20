using Testcontainers.PostgreSql;
using Xunit;

namespace ExpenseTracker.Tests.DatabaseTests;

public class PostgreSqlFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string GetConnectionString() => Container.GetConnectionString();

    public async Task InitializeAsync() => await Container.StartAsync();

    public async Task DisposeAsync() => await Container.DisposeAsync();
}