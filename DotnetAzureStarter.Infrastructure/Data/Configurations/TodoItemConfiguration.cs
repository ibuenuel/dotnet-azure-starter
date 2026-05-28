using DotnetAzureStarter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetAzureStarter.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the TodoItem entity.
/// </summary>
internal sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Priority)
            .HasConversion<int>();

        builder.Property(t => t.DueDate)
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex(t => t.IsCompleted);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.DueDate);
    }
}
