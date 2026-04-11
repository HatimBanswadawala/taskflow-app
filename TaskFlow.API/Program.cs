using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 1. Register Services (Dependency Injection)
// ──────────────────────────────────────────────

// EF Core InMemory Database — data lives in memory, resets on app restart
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TaskFlowDb"));

// Register generic repository — any entity that extends BaseEntity gets a repo
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// OpenAPI/Swagger — .NET 9 built-in (no Swashbuckle needed!)
builder.Services.AddOpenApi();

// CORS — needed later when React frontend calls this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Needed for SignalR later
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
    // Swagger UI — visit /swagger to see all endpoints
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TaskFlow API v1");
    });
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

// ──────────────────────────────────────────────
// 4. Minimal API Endpoints
// ──────────────────────────────────────────────

// Health check — quick way to verify API is running
app.MapGet("/", () => Results.Ok(new { status = "healthy", app = "TaskFlow API", version = "1.0" }))
   .WithName("HealthCheck")
   .WithTags("Health");

// GET /api/boards — return all boards with their columns and tasks
app.MapGet("/api/boards", async (AppDbContext db) =>
{
    var boards = await db.Boards
        .Include(b => b.Columns.OrderBy(c => c.Position))
            .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
        .Include(b => b.User)
        .ToListAsync();

    // Map to anonymous object to control what gets returned (no circular refs)
    var result = boards.Select(b => new
    {
        b.Id,
        b.Name,
        b.Description,
        b.CreatedAt,
        Owner = new { b.User.Id, b.User.FullName, b.User.Email },
        Columns = b.Columns.Select(c => new
        {
            c.Id,
            c.Name,
            c.Position,
            Tasks = c.Tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                t.DueDate,
                t.Position,
                t.AssignedToId
            })
        })
    });

    return Results.Ok(result);
})
.WithName("GetAllBoards")
.WithTags("Boards");

// GET /api/boards/{id} — return a single board with everything
app.MapGet("/api/boards/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var board = await db.Boards
        .Include(b => b.Columns.OrderBy(c => c.Position))
            .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
        .Include(b => b.User)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (board is null)
        return Results.NotFound(new { error = "Board not found" });

    return Results.Ok(new
    {
        board.Id,
        board.Name,
        board.Description,
        board.CreatedAt,
        Owner = new { board.User.Id, board.User.FullName, board.User.Email },
        Columns = board.Columns.Select(c => new
        {
            c.Id,
            c.Name,
            c.Position,
            Tasks = c.Tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                t.DueDate,
                t.Position,
                t.AssignedToId
            })
        })
    });
})
.WithName("GetBoardById")
.WithTags("Boards");

app.Run();
