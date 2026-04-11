using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public DateTime? DueDate { get; set; }
    public int Position { get; set; } // Order within the column (for drag-and-drop)

    // Foreign key — every task belongs to a column
    public Guid ColumnId { get; set; }
    public Column Column { get; set; } = null!;

    // Foreign key — task can be assigned to a user (optional)
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}
