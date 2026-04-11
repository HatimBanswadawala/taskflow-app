namespace TaskFlow.Domain.Entities;

public class Column : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; } // Order of column on the board (0, 1, 2...)

    // Foreign key — every column belongs to a board
    public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;

    // A column contains multiple tasks
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
