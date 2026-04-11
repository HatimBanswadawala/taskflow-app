namespace TaskFlow.Domain.Entities;

public class Board : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Foreign key — every board belongs to a user (the owner)
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // A board has multiple columns (To Do, In Progress, Done, etc.)
    public ICollection<Column> Columns { get; set; } = [];
}
