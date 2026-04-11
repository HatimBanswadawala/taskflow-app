namespace TaskFlow.Domain.Enums;

/// <summary>
/// Named TaskItemStatus (not TaskStatus) to avoid conflict with System.Threading.Tasks.TaskStatus
/// </summary>
public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    Archived = 3
}
