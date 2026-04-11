using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Data;

/// <summary>
/// Seeds the InMemory database with demo data on app startup.
/// Since InMemory DB resets on every restart, this ensures
/// the app always has something to show.
/// </summary>
public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // Don't seed if data already exists
        if (context.Users.Any()) return;

        // Demo user (password: "Demo123!" — hashed later when we add auth)
        var demoUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "Demo User",
            Email = "demo@taskflow.app",
            PasswordHash = "placeholder-will-be-hashed-later"
        };

        context.Users.Add(demoUser);

        // Demo board
        var board = new Board
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "My First Project",
            Description = "A sample Kanban board to get you started",
            UserId = demoUser.Id
        };

        context.Boards.Add(board);

        // Three default columns
        var todoColumn = new Column
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
            Name = "To Do",
            Position = 0,
            BoardId = board.Id
        };

        var inProgressColumn = new Column
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
            Name = "In Progress",
            Position = 1,
            BoardId = board.Id
        };

        var doneColumn = new Column
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333303"),
            Name = "Done",
            Position = 2,
            BoardId = board.Id
        };

        context.Columns.AddRange(todoColumn, inProgressColumn, doneColumn);

        // Sample tasks
        context.TaskItems.AddRange(
            new TaskItem
            {
                Title = "Set up project structure",
                Description = "Create Clean Architecture solution with all layers",
                Priority = Priority.High,
                Status = TaskItemStatus.Done,
                Position = 0,
                ColumnId = doneColumn.Id,
                AssignedToId = demoUser.Id
            },
            new TaskItem
            {
                Title = "Implement JWT authentication",
                Description = "Add login/register endpoints with JWT token generation",
                Priority = Priority.High,
                Status = TaskItemStatus.InProgress,
                Position = 0,
                ColumnId = inProgressColumn.Id,
                AssignedToId = demoUser.Id
            },
            new TaskItem
            {
                Title = "Build React dashboard",
                Description = "Create the main dashboard page with board overview",
                Priority = Priority.Medium,
                Status = TaskItemStatus.Todo,
                Position = 0,
                ColumnId = todoColumn.Id,
                AssignedToId = demoUser.Id
            },
            new TaskItem
            {
                Title = "Add drag-and-drop",
                Description = "Implement task card dragging between columns using @dnd-kit",
                Priority = Priority.Medium,
                Status = TaskItemStatus.Todo,
                Position = 1,
                ColumnId = todoColumn.Id
            },
            new TaskItem
            {
                Title = "Deploy to production",
                Description = "Deploy React to Vercel and .NET API to Render",
                Priority = Priority.Urgent,
                Status = TaskItemStatus.Todo,
                DueDate = DateTime.UtcNow.AddDays(30),
                Position = 2,
                ColumnId = todoColumn.Id
            }
        );

        context.SaveChanges();
    }
}
