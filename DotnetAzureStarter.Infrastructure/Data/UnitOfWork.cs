using DotnetAzureStarter.Core.Interfaces;
using DotnetAzureStarter.Infrastructure.Repositories;

namespace DotnetAzureStarter.Infrastructure.Data;

/// <summary>
/// EF Core implementation of IUnitOfWork.
/// Owns all repositories and ensures all staged changes commit atomically via SaveChangesAsync.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private ITodoRepository? _todos;

    /// <summary>Initializes the unit of work with the shared database context.</summary>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public ITodoRepository Todos => _todos ??= new TodoRepository(_context);

    /// <inheritdoc/>
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc/>
    public void Dispose() => _context.Dispose();
}
