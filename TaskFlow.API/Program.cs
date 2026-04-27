using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TaskFlow.Application.Behaviors;
using TaskFlow.Application.Features.Auth.Commands.Login;
using TaskFlow.Application.Features.Auth.Commands.Register;
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
using TaskFlow.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 0. Serilog — Structured Logging
// ──────────────────────────────────────────────
// Replaces default Microsoft logger. Outputs to:
//   - Console (developer tail-friendly)
//   - logs/taskflow-YYYYMMDD.log (rolling daily file)
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/taskflow-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// ──────────────────────────────────────────────
// 1. Register Services (Dependency Injection)
// ──────────────────────────────────────────────

// EF Core InMemory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TaskFlowDb"));
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

// Repository + Auth services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// MediatR + FluentValidation
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateBoardCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateBoardCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// OpenAPI/Swagger — with JWT bearer auth button in Swagger UI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Add security scheme definition
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token (without 'Bearer ' prefix)"
        };

        // Apply security requirement globally — shows the lock icon on all endpoints
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

// CORS for React frontend — config-driven
// Local dev defaults to localhost:5173. Production reads from AllowedOrigins env var
// (comma-separated). Example: "http://localhost:5173,https://taskflow-by-hatim.vercel.app"
var allowedOrigins = builder.Configuration["AllowedOrigins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    SeedData.Initialize(context, hasher);
}

// ──────────────────────────────────────────────
// 3. Middleware Pipeline
// ──────────────────────────────────────────────

// Swagger UI enabled in all environments — useful for portfolio API exploration
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "TaskFlow API v1");
});

// Auto-log every HTTP request: method, path, status code, duration
app.UseSerilogRequestLogging();

app.UseCors("AllowReactApp");
// Skip HTTPS redirect in Development so React proxy (HTTP) works
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();

// Global exception handler
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
    catch (UnauthorizedAccessException)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = "Authentication failed",
            status = 401,
            errors = new[] { new { PropertyName = "credentials", ErrorMessage = "Invalid email or password" } }
        });
    }
    catch (InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = "Conflict",
            status = 409,
            errors = new[] { new { PropertyName = "request", ErrorMessage = ex.Message } }
        });
    }
});

// ──────────────────────────────────────────────
// 4. Endpoints
// ──────────────────────────────────────────────

// Health check (public)
app.MapGet("/", () => Results.Ok(new { status = "healthy", app = "TaskFlow API", version = "1.0" }))
   .WithName("HealthCheck")
   .WithTags("Health");

// ─── Auth Endpoints (public — no auth required) ───

app.MapPost("/api/auth/register", async (RegisterCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Ok(result);
})
.WithName("Register")
.WithTags("Auth")
.AllowAnonymous();

app.MapPost("/api/auth/login", async (LoginCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Ok(result);
})
.WithName("Login")
.WithTags("Auth")
.AllowAnonymous();

// ─── Board Endpoints (protected — JWT required) ───

app.MapGet("/api/boards", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetBoardsQuery())))
.WithName("GetAllBoards")
.WithTags("Boards")
.RequireAuthorization();

app.MapGet("/api/boards/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var board = await mediator.Send(new GetBoardByIdQuery(id));
    return board is null ? Results.NotFound(new { error = "Board not found" }) : Results.Ok(board);
})
.WithName("GetBoardById")
.WithTags("Boards")
.RequireAuthorization();

app.MapPost("/api/boards", async (CreateBoardCommand command, IMediator mediator) =>
{
    var boardId = await mediator.Send(command);
    return Results.Created($"/api/boards/{boardId}", new { id = boardId });
})
.WithName("CreateBoard")
.WithTags("Boards")
.RequireAuthorization();

app.MapPut("/api/boards/{id:guid}", async (Guid id, UpdateBoardCommand command, IMediator mediator) =>
{
    var updated = await mediator.Send(command with { Id = id });
    return updated ? Results.NoContent() : Results.NotFound(new { error = "Board not found" });
})
.WithName("UpdateBoard")
.WithTags("Boards")
.RequireAuthorization();

app.MapDelete("/api/boards/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var deleted = await mediator.Send(new DeleteBoardCommand(id));
    return deleted ? Results.NoContent() : Results.NotFound(new { error = "Board not found" });
})
.WithName("DeleteBoard")
.WithTags("Boards")
.RequireAuthorization();

// ─── Task Endpoints (protected — JWT required) ───

app.MapPost("/api/tasks", async (CreateTaskCommand command, IMediator mediator) =>
{
    var taskId = await mediator.Send(command);
    return Results.Created($"/api/tasks/{taskId}", new { id = taskId });
})
.WithName("CreateTask")
.WithTags("Tasks")
.RequireAuthorization();

app.MapPut("/api/tasks/{id:guid}", async (Guid id, UpdateTaskCommand command, IMediator mediator) =>
{
    var updated = await mediator.Send(command with { Id = id });
    return updated ? Results.NoContent() : Results.NotFound(new { error = "Task not found" });
})
.WithName("UpdateTask")
.WithTags("Tasks")
.RequireAuthorization();

app.MapDelete("/api/tasks/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var deleted = await mediator.Send(new DeleteTaskCommand(id));
    return deleted ? Results.NoContent() : Results.NotFound(new { error = "Task not found" });
})
.WithName("DeleteTask")
.WithTags("Tasks")
.RequireAuthorization();

app.MapPut("/api/tasks/{id:guid}/move", async (Guid id, MoveTaskCommand command, IMediator mediator) =>
{
    var moved = await mediator.Send(command with { TaskId = id });
    return moved ? Results.NoContent() : Results.NotFound(new { error = "Task or column not found" });
})
.WithName("MoveTask")
.WithTags("Tasks")
.RequireAuthorization();

app.Run();
