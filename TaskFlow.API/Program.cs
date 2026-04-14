using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Behaviors;
using TaskFlow.Application.Features.Boards.Commands.CreateBoard;
using TaskFlow.Application.Features.Boards.Commands.DeleteBoard;
using TaskFlow.Application.Features.Boards.Commands.UpdateBoard;
using TaskFlow.Application.Features.Boards.Queries.GetBoardById;
using TaskFlow.Application.Features.Boards.Queries.GetBoards;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Application.Features.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Features.Tasks.Commands.MoveTask;
using TaskFlow.Application.Features.Tasks.Commands.UpdateTask;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 1. Register Services (Dependency Injection)
// ──────────────────────────────────────────────

// EF Core InMemory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TaskFlowDb"));

// Also register the base DbContext type so Application layer query handlers
// can inject it without referencing AppDbContext (Clean Architecture rule)
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

// Generic repository — used by Command handlers
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// MediatR — auto-discovers all Commands/Queries/Handlers in Application assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateBoardCommand).Assembly));

// FluentValidation — auto-registers all validators
builder.Services.AddValidatorsFromAssembly(typeof(CreateBoardCommand).Assembly);

// Validation pipeline — runs before EVERY command/query
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// OpenAPI/Swagger
builder.Services.AddOpenApi();

// CORS for React frontend (Vite default port)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ──────────────────────────────────────────────
// 2. Seed the InMemory Database
// ──────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Initialize(context);
}

// ──────────────────────────────────────────────
// 3. Middleware Pipeline
// ──────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TaskFlow API v1");
    });
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

// Global exception handler — RFC 7807 ProblemDetails
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ValidationException vex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = "Validation failed",
            status = 400,
            errors = vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        });
    }
});

// ──────────────────────────────────────────────
// 4. Endpoints
// ──────────────────────────────────────────────

// Health check
app.MapGet("/", () => Results.Ok(new { status = "healthy", app = "TaskFlow API", version = "1.0" }))
   .WithName("HealthCheck")
   .WithTags("Health");

// ─── Board Endpoints (now all MediatR-based) ───

// GET /api/boards — list all boards
app.MapGet("/api/boards", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetBoardsQuery())))
.WithName("GetAllBoards")
.WithTags("Boards");

// GET /api/boards/{id} — single board by Id
app.MapGet("/api/boards/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var board = await mediator.Send(new GetBoardByIdQuery(id));
    return board is null ? Results.NotFound(new { error = "Board not found" }) : Results.Ok(board);
})
.WithName("GetBoardById")
.WithTags("Boards");

// POST /api/boards — create new board (auto-creates 3 default columns)
app.MapPost("/api/boards", async (CreateBoardCommand command, IMediator mediator) =>
{
    var boardId = await mediator.Send(command);
    return Results.Created($"/api/boards/{boardId}", new { id = boardId });
})
.WithName("CreateBoard")
.WithTags("Boards");

// PUT /api/boards/{id} — update board name/description
app.MapPut("/api/boards/{id:guid}", async (Guid id, UpdateBoardCommand command, IMediator mediator) =>
{
    // Ensure URL Id matches body Id (override if needed)
    var commandWithId = command with { Id = id };
    var updated = await mediator.Send(commandWithId);
    return updated ? Results.NoContent() : Results.NotFound(new { error = "Board not found" });
})
.WithName("UpdateBoard")
.WithTags("Boards");

// DELETE /api/boards/{id} — delete board (cascades to columns + tasks)
app.MapDelete("/api/boards/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var deleted = await mediator.Send(new DeleteBoardCommand(id));
    return deleted ? Results.NoContent() : Results.NotFound(new { error = "Board not found" });
})
.WithName("DeleteBoard")
.WithTags("Boards");

// ─── Task Endpoints ───

// POST /api/tasks — create a task in a column
app.MapPost("/api/tasks", async (CreateTaskCommand command, IMediator mediator) =>
{
    var taskId = await mediator.Send(command);
    return Results.Created($"/api/tasks/{taskId}", new { id = taskId });
})
.WithName("CreateTask")
.WithTags("Tasks");

// PUT /api/tasks/{id} — update task details
app.MapPut("/api/tasks/{id:guid}", async (Guid id, UpdateTaskCommand command, IMediator mediator) =>
{
    var updated = await mediator.Send(command with { Id = id });
    return updated ? Results.NoContent() : Results.NotFound(new { error = "Task not found" });
})
.WithName("UpdateTask")
.WithTags("Tasks");

// DELETE /api/tasks/{id} — delete a task
app.MapDelete("/api/tasks/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var deleted = await mediator.Send(new DeleteTaskCommand(id));
    return deleted ? Results.NoContent() : Results.NotFound(new { error = "Task not found" });
})
.WithName("DeleteTask")
.WithTags("Tasks");

// PUT /api/tasks/{id}/move — move task to different column (drag-and-drop)
app.MapPut("/api/tasks/{id:guid}/move", async (Guid id, MoveTaskCommand command, IMediator mediator) =>
{
    var moved = await mediator.Send(command with { TaskId = id });
    return moved ? Results.NoContent() : Results.NotFound(new { error = "Task or column not found" });
})
.WithName("MoveTask")
.WithTags("Tasks");

app.Run();
