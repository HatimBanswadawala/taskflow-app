namespace TaskFlow.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation properties — a user can own multiple boards and be assigned tasks
    public ICollection<Board> Boards { get; set; } = [];
    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
}
