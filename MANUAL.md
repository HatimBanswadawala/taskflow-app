# TaskFlow — Complete Build Manual

> **What is this?** A step-by-step technical journal documenting every decision, command, and concept used to build TaskFlow from scratch. Written so that even 2-3 years from now, you can pick this up and rebuild a similar project — or understand exactly what was done and why.

> **Tech Stack:** .NET 9 + React 18 + TypeScript + EF Core InMemory + SignalR + Tailwind + shadcn/ui

> **Started:** April 11, 2026
> **Author:** Hatim Banswadawala (with Claude as coding partner)

---

## Table of Contents

- [Session 1: Solution Setup & Domain Entities](#session-1-solution-setup--domain-entities)

---

## Session 1: Solution Setup & Domain Entities
**Date:** April 11, 2026
**Duration:** ~1 hour
**Goal:** Create the .NET solution with Clean Architecture and define domain entities

### Prerequisites Verified
| Tool | Version | Why Needed |
|------|---------|-----------|
| .NET SDK | 9.0.312 | Backend API framework |
| Node.js | 25.2.1 | React frontend build/dev |
| npm | 11.11.0 | Package manager for React |
| Git | 2.53.0 | Version control |

### What is Clean Architecture?

Clean Architecture separates your application into layers where **inner layers know nothing about outer layers**. Think of it like concentric circles:

```
┌──────────────────────────────────────┐
│            TaskFlow.API              │  ← Outer: HTTP endpoints, middleware
│  ┌──────────────────────────────┐    │
│  │     TaskFlow.Application     │    │  ← Use cases: MediatR handlers, DTOs
│  │  ┌──────────────────────┐    │    │
│  │  │   TaskFlow.Domain    │    │    │  ← Inner: Entities, enums, interfaces
│  │  └──────────────────────┘    │    │     (ZERO dependencies)
│  └──────────────────────────────┘    │
│       TaskFlow.Infrastructure        │  ← Outer: EF Core, JWT, repositories
│          TaskFlow.Tests              │  ← Outer: Unit tests
└──────────────────────────────────────┘
```

**Why this matters:**
- Domain layer has ZERO NuGet dependencies — pure C# classes
- If you swap EF Core for Dapper tomorrow, only Infrastructure changes
- API layer can change from Controllers to Minimal APIs without touching business logic
- This is the #1 architecture pattern interviewers ask about

### Step 1: Create Solution File

```bash
mkdir -p C:\Users\hbanswadawala\Desktop\InterviewPrep\TaskFlow
cd C:\Users\hbanswadawala\Desktop\InterviewPrep\TaskFlow
dotnet new sln -n TaskFlow
```

**What this does:** Creates `TaskFlow.sln` — the master file that Visual Studio/Rider uses to know which projects belong together. Think of it as a table of contents.

### Step 2: Create All 5 Projects

```bash
dotnet new classlib -n TaskFlow.Domain -f net9.0         # Innermost: entities, enums, interfaces
dotnet new classlib -n TaskFlow.Application -f net9.0    # Use cases: MediatR handlers, DTOs
dotnet new classlib -n TaskFlow.Infrastructure -f net9.0 # EF Core, repositories, JWT service
dotnet new webapi -n TaskFlow.API -f net9.0 --use-minimal-apis true  # Entry point: HTTP endpoints
dotnet new xunit -n TaskFlow.Tests -f net9.0             # Unit tests
```

**Why 5 separate projects instead of 1?**
- Enforces boundaries — Domain CAN'T accidentally use EF Core
- Each layer has a clear responsibility
- Swapping implementations (e.g., EF Core → Dapper) only touches Infrastructure
- This is what interviewers mean by "Clean Architecture" — separation of concerns at the project level

### Step 3: Add Projects to Solution & Wire References

```bash
# Add all to solution
dotnet sln add TaskFlow.Domain TaskFlow.Application TaskFlow.Infrastructure TaskFlow.API TaskFlow.Tests

# Set up dependency chain (Clean Architecture enforced)
dotnet add TaskFlow.Application reference TaskFlow.Domain
dotnet add TaskFlow.Infrastructure reference TaskFlow.Domain
dotnet add TaskFlow.Infrastructure reference TaskFlow.Application
dotnet add TaskFlow.API reference TaskFlow.Application
dotnet add TaskFlow.API reference TaskFlow.Infrastructure
dotnet add TaskFlow.Tests reference TaskFlow.Application
dotnet add TaskFlow.Tests reference TaskFlow.Domain
```

**Dependency rules:**
- Domain → depends on NOTHING (pure C#)
- Application → depends on Domain only
- Infrastructure → depends on Domain + Application
- API → depends on Application + Infrastructure (to wire DI)
- Tests → depends on Application + Domain (to test handlers and entities)

### Step 4: Domain Entities Created

**Files created in `TaskFlow.Domain/`:**

```
TaskFlow.Domain/
├── Entities/
│   ├── BaseEntity.cs      # Guid Id, CreatedAt, UpdatedAt — all entities inherit this
│   ├── User.cs            # FullName, Email, PasswordHash + nav props (Boards, AssignedTasks)
│   ├── Board.cs           # Name, Description, UserId (FK) + nav prop (Columns)
│   ├── Column.cs          # Name, Position (ordering), BoardId (FK) + nav prop (Tasks)
│   └── TaskItem.cs        # Title, Description, Priority, Status, DueDate, Position, ColumnId (FK), AssignedToId (FK)
├── Enums/
│   ├── Priority.cs        # Low=0, Medium=1, High=2, Urgent=3
│   └── TaskItemStatus.cs  # Todo=0, InProgress=1, Done=2, Archived=3
└── Interfaces/
    └── IRepository.cs     # Generic repo interface (GetById, GetAll, Add, Update, Delete)
```

**Key design decisions:**
1. **`BaseEntity` with `Guid Id`** — No auto-increment integers. GUIDs are better for distributed systems and don't leak info about record count.
2. **`TaskItem` not `Task`** — Avoids collision with `System.Threading.Tasks.Task`.
3. **`TaskItemStatus` not `TaskStatus`** — Same reason, avoids ambiguity.
4. **`Position` on Column and TaskItem** — Needed for drag-and-drop ordering later.
5. **`AssignedToId` is nullable (`Guid?`)** — Tasks can be unassigned.
6. **`IRepository<T>` in Domain** — Domain defines the contract, Infrastructure implements it. This is Dependency Inversion (SOLID "D").
7. **Navigation properties use `= []`** — C# 12 collection expression, cleaner than `new List<T>()`.
8. **`= null!`** on required navigation props — Tells compiler "EF Core will populate this, trust me."

### Build Verification (Step 4)

```bash
dotnet build TaskFlow.sln
# Expected: 0 errors
```

### Step 5: EF Core InMemory + Repository + Seed Data

**NuGet package installed:**
```bash
dotnet add TaskFlow.Infrastructure package Microsoft.EntityFrameworkCore.InMemory --version 9.0.*
```
Note: Must match .NET SDK major version. EF Core 10.x = .NET 10, EF Core 9.x = .NET 9.

**Files created in `TaskFlow.Infrastructure/`:**

```
TaskFlow.Infrastructure/
├── Data/
│   ├── AppDbContext.cs    # EF Core DbContext — maps entities to tables
│   └── SeedData.cs        # Pre-populates InMemory DB with demo data on startup
└── Repositories/
    └── Repository.cs      # Generic repository implementing IRepository<T>
```

**AppDbContext key configurations:**
- `User.Email` — unique index (no duplicate registrations)
- `Board → User` — cascade delete (delete user = delete all their boards)
- `Column → Board` — cascade delete (delete board = delete all columns)
- `TaskItem → Column` — cascade delete (delete column = delete all tasks)
- `TaskItem → AssignedTo` — SetNull on delete (don't delete task if assigned user is removed)
- Max lengths on all string fields (security + data integrity)

**Repository pattern:**
- Domain defines `IRepository<T>` (the contract/interface)
- Infrastructure provides `Repository<T>` (the EF Core implementation)
- This is **Dependency Inversion Principle** (SOLID "D") — the inner layer defines what it needs, the outer layer provides how
- `UpdateAsync` automatically sets `UpdatedAt = DateTime.UtcNow`

**Seed data (SeedData.cs):**
- Creates 1 demo user (demo@taskflow.app)
- Creates 1 board ("My First Project") with 3 columns (To Do, In Progress, Done)
- Creates 5 sample tasks spread across columns with different priorities
- Uses fixed GUIDs for predictable testing
- Only seeds if DB is empty (idempotent)

### Build Verification (Step 5)

```bash
dotnet build TaskFlow.sln
# Expected: 0 errors
```

### Step 6: Program.cs — DI Registration + First Endpoints

**NuGet packages added to TaskFlow.API:**
```bash
dotnet add TaskFlow.API package Microsoft.EntityFrameworkCore.InMemory --version 9.0.*
dotnet add TaskFlow.API package Swashbuckle.AspNetCore.SwaggerUI --version 7.3.*
```

**Program.cs wires up:**
1. **EF Core InMemory** — `AddDbContext<AppDbContext>` with `UseInMemoryDatabase("TaskFlowDb")`
2. **Generic Repository** — `AddScoped(typeof(IRepository<>), typeof(Repository<>))` — open generic registration
3. **OpenAPI** — .NET 9 built-in `AddOpenApi()` (no Swashbuckle for doc generation, only for UI)
4. **CORS** — Configured for React Vite dev server (`http://localhost:5173`), AllowCredentials for SignalR later
5. **Seed Data** — `SeedData.Initialize()` runs on startup inside a DI scope
6. **Swagger UI** — Available at `/swagger` in Development mode
7. **Health Check** — `GET /` returns app status
8. **Board endpoints:**
   - `GET /api/boards` — returns all boards with nested columns + tasks
   - `GET /api/boards/{id}` — returns single board by GUID

**Key concepts demonstrated:**
- Open generic DI registration (`typeof(IRepository<>)`) — registers repo for ALL entity types at once
- `CreateScope()` for seed data — can't use scoped services (DbContext) outside a request, so we create a manual scope
- Anonymous objects for response shaping — avoids circular reference issues with EF navigation properties. Will refactor to proper DTOs when MediatR is added.
- `.Include().ThenInclude()` — eager loading nested relationships
- `.OrderBy(c => c.Position)` — columns and tasks returned in correct drag-drop order

### Step 7: Launch Settings

**File:** `TaskFlow.API/Properties/launchSettings.json`

- `launchBrowser: true` — auto-opens browser on `dotnet run`
- `launchUrl: "swagger"` — opens Swagger UI directly
- **https profile** has 2 URLs: `https://7037` (primary) + `http://5170` (redirects to HTTPS via `UseHttpsRedirection`)
- **http profile** has 1 URL: `http://5170` (no SSL, simpler for dev)
- Use **https** profile for development (matches production behavior)

### Session 1 Result

API running at `https://localhost:7037/swagger` with:
- ✅ Health check endpoint returning status
- ✅ GET /api/boards returning seeded demo data (1 board, 3 columns, 5 tasks)
- ✅ GET /api/boards/{id} returning single board
- ✅ Swagger UI auto-opens on launch

---

## Session 2: MediatR CQRS + DTOs + CreateBoard Command
**Date:** April 12, 2026
**Goal:** Introduce CQRS pattern with MediatR, create proper DTOs, implement first Command with validation

### Step 1: NuGet Packages Installed

```bash
dotnet add TaskFlow.Application package MediatR --version 12.*
dotnet add TaskFlow.Application package FluentValidation.DependencyInjectionExtensions --version 11.*
```

| Package | Purpose |
|---------|---------|
| MediatR | Routes Commands/Queries to Handlers. Supports pipeline behaviors (middleware for MediatR). |
| FluentValidation | Readable validation rules. `RuleFor(x => x.Name).NotEmpty().MaximumLength(100)` |

### Step 2: What Is CQRS?

**CQRS = Command Query Responsibility Segregation**
- **Commands** = write operations (Create, Update, Delete). Change state, return Id or void.
- **Queries** = read operations (Get, List). Return data, never modify state.

**Benefits:**
- Clear intent — `CreateBoardCommand` is obviously a write
- Single Responsibility — each handler does ONE thing
- Testable — handlers are pure classes
- API layer stays thin: `app.MapPost(path, (cmd, mediator) => mediator.Send(cmd))`

### Step 3: Folder Structure — Vertical Slice

Organized by FEATURE, not by TYPE. Everything related to "Create Board" lives together:

```
TaskFlow.Application/
├── Behaviors/
│   └── ValidationBehavior.cs              # Auto-validates every command/query
├── DTOs/
│   └── BoardDto.cs                        # BoardDto, OwnerDto, ColumnDto, TaskItemDto (records)
└── Features/
    └── Boards/
        └── Commands/
            └── CreateBoard/
                ├── CreateBoardCommand.cs         # IRequest<Guid> — the data
                ├── CreateBoardCommandHandler.cs  # Where the logic lives
                └── CreateBoardCommandValidator.cs # FluentValidation rules
```

**Why Vertical Slice?** As project grows, finding code for a feature is easy. All related files are in one folder. This is what you learned about in your .NET study plan's Day-35+36+37.

### Step 4: Files Created — Explained

#### DTOs (Data Transfer Objects)
- `BoardDto`, `OwnerDto`, `ColumnDto`, `TaskItemDto` — all C# **records** (immutable, auto-equality)
- Decouple API response shape from domain entities
- If Board entity changes, DTO can stay stable = backward-compatible API

#### CreateBoardCommand (the data)
```csharp
public record CreateBoardCommand(
    string Name,
    string? Description,
    Guid UserId
) : IRequest<Guid>;
```
- Record = immutable data
- `IRequest<Guid>` = MediatR marker interface, `Guid` = the return type (new board's Id)

#### CreateBoardCommandValidator (the rules)
```csharp
RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
RuleFor(x => x.Description).MaximumLength(500);
RuleFor(x => x.UserId).NotEmpty();
```
- Runs automatically before handler (via ValidationBehavior)
- Fails = throws `ValidationException` with property name + error message

#### CreateBoardCommandHandler (the logic)
```csharp
public async Task<Guid> Handle(CreateBoardCommand request, CancellationToken ct)
{
    var board = new Board { Name = request.Name, ... };
    board.Columns = [ /* 3 default columns auto-created */ ];
    return (await _boardRepository.AddAsync(board)).Id;
}
```
- Injects `IRepository<Board>` from DI
- **Nice UX touch**: auto-creates "To Do", "In Progress", "Done" columns when board is created

### Step 5: ValidationBehavior — MediatR Pipeline Magic

`ValidationBehavior<TRequest, TResponse>` is a pipeline behavior = middleware for MediatR.

**How it works:**
1. DI injects ALL validators for the given request type
2. Before the handler runs, run all validators
3. If any fail → `throw new ValidationException(failures)`
4. Handler NEVER runs if validation fails

**Why this is powerful:**
- Zero validation code in handlers — they just focus on business logic
- Add a new Command + Validator → validation automatically wires up
- Composable — can add LoggingBehavior, PerformanceBehavior, etc. the same way

### Step 6: Program.cs — Wiring It Together

```csharp
// MediatR — scans assembly for Commands/Queries/Handlers
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateBoardCommand).Assembly));

// FluentValidation — auto-registers all validators
builder.Services.AddValidatorsFromAssembly(typeof(CreateBoardCommand).Assembly);

// Validation runs before EVERY command/query
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Step 7: Global Exception Handler (app.Use Middleware)

```csharp
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (ValidationException vex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = "Validation failed",
            status = 400,
            errors = vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        });
    }
});
```

**Key concepts:**
- `app.Use` adds middleware to the HTTP pipeline
- `await next()` passes request DOWNSTREAM to other middleware/endpoints
- Any exception thrown downstream **bubbles UP** through the call stack
- Our try/catch sits HIGH in the pipeline so it catches exceptions from anywhere
- Returns **RFC 7807 ProblemDetails** format — industry-standard API error response

### Exception Bubbling — Mental Model

The **call stack** is a LIFO (last-in, first-out) stack of function calls. When an exception is thrown:
1. Runtime checks current function — does it have a try/catch?
2. If no, pop the frame off the stack, ask the CALLER
3. Keep popping until SOMEONE catches it
4. If nobody does, app crashes (500 Internal Server Error)

**Our TaskFlow flow for a bad request:**
```
ValidationBehavior (throws)
    ↑ bubbles up
MediatR internals
    ↑
mediator.Send(command)
    ↑
MapPost lambda handler
    ↑
ASP.NET Core endpoint routing
    ↑
OUR app.Use(async (context, next) => { try... })  ← CAUGHT HERE
```

**Async/await note:** Even with async code, `await` re-throws exceptions from the awaited task to the awaiting method. So try/catch around `await next()` works for async-spanning calls.

### Step 8: New Endpoint Added

```csharp
app.MapPost("/api/boards", async (CreateBoardCommand command, IMediator mediator) =>
{
    var boardId = await mediator.Send(command);
    return Results.Created($"/api/boards/{boardId}", new { id = boardId });
});
```
- ONE LINE sends to MediatR — no validation, no business logic, no DB code in the API layer
- Returns **201 Created** with Location header pointing to the new resource (REST best practice)

### Session 2 Milestone

- ✅ POST /api/boards works — creates board with 3 auto-columns
- ✅ Empty name / long description returns 400 with validation errors in ProblemDetails format
- ✅ Handler classes are tiny and testable (no validation code inside)
- ✅ CQRS + Pipeline Behaviors pattern in place

### What's Next (Session 2 cont.)
- UpdateBoardCommand, DeleteBoardCommand
- CreateTaskCommand, UpdateTaskCommand, DeleteTaskCommand, MoveTaskCommand
- Refactor existing GET endpoints to use MediatR Queries (GetBoardsQuery, GetBoardByIdQuery)
- Proper DTO responses (replace anonymous objects)

