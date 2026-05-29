namespace DotnetAzureStarter.Core.Interfaces;

/// <summary>
/// Manages transactions across multiple repositories.
/// Ensures all staged changes are committed or rolled back together as one atomic unit.
/// Controllers and services call CommitAsync once after all repository operations are staged.
/// Add one property per aggregate root repository for your domain.
/// </summary>
public interface IUnitOfWork : IDisposable
{

    // TODO: Replace with your own domain's repository — this is a sample.
    /// <summary>Todo item repository. Replace with your own aggregate root repositories.</summary>
    ITodoRepository Todos { get; }

    /// <summary>
    /// Persists all staged changes across all repositories in a single database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Number of entities written to the data store.</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
