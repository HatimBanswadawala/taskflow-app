namespace TaskFlow.Domain.Entities;

/// <summary>
/// Base class for all entities — provides Id and audit timestamps.
/// Every entity in TaskFlow inherits this.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
