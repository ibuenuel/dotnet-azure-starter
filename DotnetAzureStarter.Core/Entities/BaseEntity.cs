namespace DotnetAzureStarter.Core.Entities;

/// <summary>
/// Base entity containing common audit fields for all domain entities.
/// Uses DateTimeOffset to preserve timezone context — required for Azure multi-region deployments.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier for the entity.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the entity was created.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp when the entity was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
