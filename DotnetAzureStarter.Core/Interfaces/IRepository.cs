using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.Entities;

namespace DotnetAzureStarter.Core.Interfaces;

/// <summary>
/// Generic repository interface providing standard CRUD operations for entities.
/// SaveChanges is intentionally absent — use IUnitOfWork.CommitAsync to persist changes
/// and ensure all modifications across multiple repositories commit as one transaction.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Retrieves all entities. Use GetPagedAsync for large datasets.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paginated page of entities with navigation metadata.</summary>
    Task<PagedResult<T>> GetPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    /// <summary>Retrieves an entity by its unique identifier. Returns null if not found.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Stages a new entity for insertion. Persisted on IUnitOfWork.CommitAsync.</summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Stages an entity for update. Persisted on IUnitOfWork.CommitAsync.</summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Stages an entity for deletion. Persisted on IUnitOfWork.CommitAsync.</summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Stages an entity for deletion by ID. Returns false if not found.</summary>
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
