using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Core.Interfaces;
using DotnetAzureStarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetAzureStarter.Infrastructure.Repositories;

/// <summary>
/// Base repository implementing standard CRUD operations via EF Core.
/// Extend this class for entity-specific repositories rather than reimplementing common operations.
/// No SaveChanges is called here — all persistence goes through IUnitOfWork.CommitAsync.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity.</typeparam>
public abstract class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>The EF Core database context shared with the UnitOfWork.</summary>
    protected readonly AppDbContext _context;

    /// <summary>Initializes the repository with the shared database context.</summary>
    protected GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<PagedResult<T>> GetPagedAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FindAsync([id], cancellationToken);

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        _context.Set<T>().Remove(entity);
        return true;
    }
}
