using DotnetAzureStarter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetAzureStarter.Infrastructure.Data;

/// <summary>
/// EF Core database context. All entity access goes through this class.
/// Direct usage outside of the Infrastructure layer is forbidden — use IUnitOfWork.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <inheritdoc/>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Todo items table.</summary>
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
