using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year })
            .IsUnique();
        
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Expenses)
            .WithOne(e => e.Category)
            .OnDelete(DeleteBehavior.Cascade);
    }
}