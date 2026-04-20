# TaskFlow — Complete Build Manual

> **What is this?** A step-by-step technical journal documenting every decision, command, and concept used to build TaskFlow from scratch. Written so that even 2-3 years from now, you can pick this up and rebuild a similar project — or understand exactly what was done and why.

> **Tech Stack:** .NET 9 + React 18 + JavaScript (JSX) + EF Core InMemory + SignalR + Tailwind + shadcn/ui
> **Note:** Frontend uses pure JavaScript (.jsx), NOT TypeScript. Resume skill focus: React + JS.

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

### Step 9: Board CRUD Complete (Update + Delete)

**Files created:**
```
Features/Boards/Commands/UpdateBoard/
    ├── UpdateBoardCommand.cs          # IRequest<bool> — Name, Description, Id
    ├── UpdateBoardCommandValidator.cs  # Id not empty, Name required, max lengths
    └── UpdateBoardCommandHandler.cs    # Finds board, updates fields, saves

Features/Boards/Commands/DeleteBoard/
    ├── DeleteBoardCommand.cs          # IRequest<bool> — just Id
    ├── DeleteBoardCommandValidator.cs  # Id not empty
    └── DeleteBoardCommandHandler.cs    # Finds board, deletes (cascade removes columns + tasks)
```

### Step 10: MediatR Queries — Replacing Anonymous Objects with DTOs

**Problem solved:** Old GET endpoints used `AppDbContext` directly in Program.cs with anonymous objects. Now they use proper MediatR queries returning typed DTOs.

**Files created:**
```
Features/Boards/Queries/GetBoards/
    ├── GetBoardsQuery.cs              # IRequest<IEnumerable<BoardDto>>
    └── GetBoardsQueryHandler.cs       # Uses DbContext + BoardMapper

Features/Boards/Queries/GetBoardById/
    ├── GetBoardByIdQuery.cs           # IRequest<BoardDto?>
    └── GetBoardByIdQueryHandler.cs    # Returns null if not found
```

**Key design pattern — Commands vs Queries use different data access:**
- Commands → use `IRepository<T>` (simple CRUD, abstracted)
- Queries → use `DbContext` directly (need `.Include()`, projections, joins)

**Clean Architecture trick for DbContext in Application layer:**
```csharp
// In Program.cs — register base DbContext as alias to AppDbContext
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
```
This is a **factory registration** — when Application asks for `DbContext`, DI gives the existing `AppDbContext` instance. Same object, different type. Application never references Infrastructure.

### Step 11: BoardMapper — DRY Mapping

**File:** `TaskFlow.Application/Mappers/BoardMapper.cs`

Extension methods (`ToDto()`) on Board, Column, TaskItem entities. Avoids duplicating the DTO mapping logic across query handlers.

```csharp
var boards = await _db.Set<Board>()...ToListAsync();
return boards.Select(b => b.ToDto()); // Clean, one line
```

### Step 12: Task CRUD — Commands + Handlers + Validators

**NuGet package added:**
```bash
dotnet add TaskFlow.Application package Microsoft.EntityFrameworkCore --version 9.0.*
```
Needed because Task handlers use `DbContext` directly (same pattern as queries).

**Files created:**
```
Features/Tasks/Commands/CreateTask/
    ├── CreateTaskCommand.cs           # Title, Description, Priority, ColumnId, AssignedToId, DueDate
    ├── CreateTaskCommandValidator.cs  # Title required, ColumnId required, DueDate must be future
    └── CreateTaskCommandHandler.cs    # Verifies column exists, auto-positions at bottom of column

Features/Tasks/Commands/UpdateTask/
    ├── UpdateTaskCommand.cs           # Id, Title, Description, Priority, DueDate, AssignedToId
    ├── UpdateTaskCommandValidator.cs  # Id + Title required, Priority valid enum
    └── UpdateTaskCommandHandler.cs    # Finds task, updates fields, sets UpdatedAt

Features/Tasks/Commands/DeleteTask/
    ├── DeleteTaskCommand.cs           # Just Id
    └── DeleteTaskCommandHandler.cs    # Finds and removes

Features/Tasks/Commands/MoveTask/
    ├── MoveTaskCommand.cs             # TaskId, TargetColumnId, NewPosition
    ├── MoveTaskCommandValidator.cs    # All required, Position >= 0
    └── MoveTaskCommandHandler.cs      # THE MOST COMPLEX HANDLER (see below)
```

**MoveTaskCommandHandler — drag-and-drop backend logic:**
1. Validates task and target column exist
2. If moving to different column: reorders source column (close the gap)
3. Reorders target column (make room at new position)
4. Updates task's ColumnId and Position
5. **Smart UX:** Auto-updates task Status based on target column name:
   - Drop in "To Do" → `TaskItemStatus.Todo`
   - Drop in "In Progress" → `TaskItemStatus.InProgress`
   - Drop in "Done" → `TaskItemStatus.Done`
   - Custom column → keeps current status

### Step 13: Program.cs — All Endpoints Now MediatR-Based

```
Board Endpoints:
  GET    /api/boards          → mediator.Send(new GetBoardsQuery())
  GET    /api/boards/{id}     → mediator.Send(new GetBoardByIdQuery(id))
  POST   /api/boards          → mediator.Send(command)
  PUT    /api/boards/{id}     → mediator.Send(command with { Id = id })
  DELETE /api/boards/{id}     → mediator.Send(new DeleteBoardCommand(id))

Task Endpoints:
  POST   /api/tasks           → mediator.Send(command)
  PUT    /api/tasks/{id}      → mediator.Send(command with { Id = id })
  DELETE /api/tasks/{id}      → mediator.Send(new DeleteTaskCommand(id))
  PUT    /api/tasks/{id}/move → mediator.Send(command with { TaskId = id })
```

**Every endpoint is 1-3 lines.** All business logic lives in handlers. API layer is pure routing.

**REST conventions used:**
- POST → 201 Created with Location header
- PUT/DELETE → 204 NoContent on success
- Not found → 404 with error object
- Validation failure → 400 with ProblemDetails (handled by global middleware)

### Session 2 Result

- ✅ Full Board CRUD (Create, Read, Update, Delete)
- ✅ Full Task CRUD (Create, Update, Delete, Move)
- ✅ MediatR CQRS pattern across all endpoints
- ✅ FluentValidation on all commands (auto via pipeline behavior)
- ✅ Proper DTOs replacing anonymous objects
- ✅ BoardMapper for DRY entity-to-DTO conversion
- ✅ Drag-and-drop backend (MoveTask with auto-status update)
- ✅ 9 total API endpoints, all Swagger-testable

---

## Session 3: JWT Authentication
**Date:** April 12, 2026
**Goal:** Complete auth system — register, login, JWT tokens, protected endpoints

### Step 1: Auth Interfaces in Domain

```
TaskFlow.Domain/Interfaces/
├── IPasswordHasher.cs     # Hash(password) + Verify(password, hash)
└── IJwtTokenService.cs    # GenerateToken(user) → JWT string
```

Domain defines WHAT it needs (interfaces). Infrastructure provides HOW (BCrypt + JWT).

### Step 2: Infrastructure Implementations

```
TaskFlow.Infrastructure/Services/
├── PasswordHasher.cs      # BCrypt with workFactor 12 (~250ms per hash)
└── JwtTokenService.cs     # Builds JWT with claims, signs with HMAC-SHA256
```

**NuGet packages added to Infrastructure:**
```bash
dotnet add TaskFlow.Infrastructure package BCrypt.Net-Next --version 4.*
dotnet add TaskFlow.Infrastructure package System.IdentityModel.Tokens.Jwt --version 8.*
dotnet add TaskFlow.Infrastructure package Microsoft.Extensions.Configuration.Abstractions --version 9.0.*
```

**PasswordHasher:** BCrypt is "slow by design" — makes brute-force impractical. Salt is included in the hash automatically.

**JwtTokenService — GenerateToken() flow:**
1. Build claims (NameIdentifier, Email, Name, custom "uid")
2. Create signing key from appsettings `Jwt:Key` (SymmetricSecurityKey)
3. Create credentials (HMAC-SHA256 algorithm)
4. Build JwtSecurityToken (issuer, audience, claims, 24hr expiry, credentials)
5. Serialize to string with JwtSecurityTokenHandler

**JWT token structure:** `HEADER.PAYLOAD.SIGNATURE` (Base64 encoded, NOT encrypted — anyone can read claims, but nobody can tamper without the secret key)

### Step 3: appsettings.json — JWT Configuration

```json
"Jwt": {
    "Key": "TaskFlow-Super-Secret-Key-That-Is-At-Least-32-Characters-Long-2026",
    "Issuer": "TaskFlow.API",
    "Audience": "TaskFlow.Client"
}
```
In production, Key comes from Azure Key Vault / env vars — never committed. Fine for InMemory demo.

### Step 4: Auth Commands (Register + Login)

```
TaskFlow.Application/Features/Auth/
├── Commands/Register/
│   ├── RegisterCommand.cs          # FullName, Email, Password → AuthResponseDto
│   ├── RegisterCommandValidator.cs # Email format, password strength (uppercase + number + 6 chars)
│   └── RegisterCommandHandler.cs   # Check duplicate email, hash password, save user, generate token
└── Commands/Login/
    ├── LoginCommand.cs             # Email, Password → AuthResponseDto
    ├── LoginCommandValidator.cs    # Email + password not empty
    └── LoginCommandHandler.cs      # Find user, verify BCrypt hash, generate token
```

**AuthResponseDto:** `{ Token, UserId, FullName, Email }` — frontend gets everything it needs in one response.

**Security:** Both "email not found" and "wrong password" return same message `"Invalid email or password"` — prevents email enumeration attacks.

### Step 5: JWT Validation (AddJwtBearer) — How Token Checking Works

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... TokenValidationParameters ... });
```

**Per-request validation flow:**
1. Read `Authorization: Bearer <token>` header
2. Split token into HEADER.PAYLOAD.SIGNATURE
3. **Signature check:** Recalculate HMAC-SHA256(HEADER + PAYLOAD, our_secret_key), compare with SIGNATURE. Mismatch = tampered = 401
4. **Lifetime check:** Read `exp` claim, compare with UtcNow. Expired = 401
5. **Issuer check:** Read `iss` claim, compare with "TaskFlow.API". Mismatch = 401
6. **Audience check:** Read `aud` claim, compare with "TaskFlow.Client". Mismatch = 401
7. All passed → extract claims → set HttpContext.User → request proceeds

**Middleware order matters:**
```csharp
app.UseAuthentication();  // First: reads token, sets User identity
app.UseAuthorization();   // Second: checks if User has access to endpoint
```
Reverse = everything gets 401 (authorization checks before identity is established).

### Step 6: Protected Endpoints

- `AllowAnonymous()` — auth/register, auth/login, health check (public)
- `RequireAuthorization()` — all board + task endpoints (JWT required)

### Step 7: Global Exception Handler — Expanded

Added catch blocks for:
- `ValidationException` → 400 Bad Request (FluentValidation)
- `UnauthorizedAccessException` → 401 Unauthorized (bad credentials)
- `InvalidOperationException` → 409 Conflict (duplicate email, column not found)

### Step 8: Swagger Authorize Button

.NET 9 built-in OpenAPI doesn't auto-add the Authorize button. Added `AddDocumentTransformer` to configure Bearer security scheme in the OpenAPI document.

```bash
dotnet add TaskFlow.API package Microsoft.AspNetCore.OpenApi --version 9.0.*
```

### Step 9: SeedData Updated

Demo user now uses real BCrypt hash instead of placeholder string. `SeedData.Initialize()` accepts `IPasswordHasher` parameter.

### Session 3 Result

- ✅ POST /api/auth/register — creates user, returns JWT
- ✅ POST /api/auth/login — verifies password, returns JWT
- ✅ GET /api/boards without token → 401
- ✅ GET /api/boards with token → 200 + data
- ✅ Swagger Authorize button works
- ✅ Duplicate email → 409 Conflict
- ✅ Bad password → 401 with generic message
- ✅ Password validation (uppercase, number, 6+ chars)
- ✅ Demo user login works (demo@taskflow.app / Demo123!)

---

## Session 4: React Scaffolding (Vite + JavaScript + Tailwind)
**Date:** April 14, 2026
**Goal:** Set up React frontend with clean structure, dark mode, and API client ready

### Key Decision: JavaScript over TypeScript
Initially scaffolded with TypeScript, then switched to pure JavaScript. Reason: Hatim's resume focus is React + JS. Files use `.jsx` extension. React study booklets (Q1-Q36) will be updated to JS later.

### Step 1: Scaffold Vite Project
```powershell
cd C:\Users\hbanswadawala\Desktop\InterviewPrep\TaskFlow
npm create vite@latest client -- --template react
cd client
npm install
```
Creates `client/` folder with React 18 + Vite + JavaScript baseline.

### Step 2: Install Tailwind CSS v4
```bash
npm install tailwindcss @tailwindcss/vite
```
Tailwind v4 (2025 release) — no config files needed. Uses native Vite plugin and `@import "tailwindcss"` in CSS.

### Step 3: Configure vite.config.js
Added 3 things:
- **Tailwind plugin** — processes Tailwind at build time
- **Path alias `@`** — clean imports (`import X from '@/components/X'` instead of `../../X`)
- **API proxy** — `/api/*` forwards to `https://localhost:7037`, avoids CORS entirely in dev

### Step 4: Update index.css
- `@import "tailwindcss"` — brings in all utility classes
- `@custom-variant dark (&:where(.dark, .dark *))` — registers `dark:` prefix to work with `.dark` class on `<html>` (Tailwind v4 default is prefers-color-scheme, we need class-based)

### Step 5: App.jsx — Landing Page + Dark Mode Toggle
Replaced Vite default with TaskFlow landing:
- Header with logo + dark/light toggle
- Hero with gradient title
- 3 feature cards (composition pattern — reusable `<FeatureCard />`)
- Status banner
- Footer

**React concepts used:**
- `useState` with lazy initializer (reads localStorage on mount)
- `useEffect` with `[darkMode]` dependency (syncs DOM class when state changes)
- Props destructuring: `function FeatureCard({ icon, title, description })`
- Event handler: `onClick={() => setDarkMode(!darkMode)}`
- Conditional rendering: `{darkMode ? '☀ Light' : '🌙 Dark'}`
- Component reuse: `<FeatureCard />` called 3 times with different props

### Step 6: Core Dependencies Installed
```bash
npm install react-router-dom axios @tanstack/react-query @microsoft/signalr clsx tailwind-merge lucide-react
```

| Package | Purpose | Used in Session |
|---------|---------|-----------------|
| react-router-dom | Routing, protected routes | 5 |
| axios | HTTP client with JWT interceptors | 5 |
| @tanstack/react-query | Server state management | 6 |
| @microsoft/signalr | Real-time WebSocket client | 10 |
| clsx | Conditional class names | All UI |
| tailwind-merge | Dedupe conflicting Tailwind classes | All UI |
| lucide-react | Icon library | All UI |

### Step 7: Folder Structure Started
```
client/src/
├── App.jsx              # Root component (landing page)
├── main.jsx             # Entry point (createRoot)
├── index.css            # Global styles + Tailwind
├── lib/
│   └── utils.js         # cn() helper for combining Tailwind classes
└── services/
    └── apiClient.js     # Axios with JWT interceptors
```

### Step 8: apiClient.js — Axios with JWT Auto-Attach
Key pattern: two interceptors handle auth automatically.
- **Request interceptor** — reads JWT from localStorage, adds `Authorization: Bearer <token>` header to every request
- **Response interceptor** — on 401, clears localStorage and redirects to `/login`

Result: components never need to manually attach tokens or handle auth expiry — it's all automatic.

### Session 4 Result
- ✅ React app runs at `http://localhost:5173`
- ✅ Dark/light mode toggle working with localStorage persistence
- ✅ Tailwind v4 configured (class-based dark mode)
- ✅ Path alias `@` configured
- ✅ API proxy to .NET backend (no CORS issues)
- ✅ 7 core dependencies installed
- ✅ Axios client with JWT auto-attach + auto-redirect on 401
- ✅ Utils helper ready

---

## Session 5: React Router + Auth Pages
**Date:** April 15, 2026
**Goal:** Build Login/Register pages with routing, AuthContext, Protected Routes

### Files Created
- `src/context/AuthContext.jsx` — createContext + AuthProvider + useAuth custom hook
- `src/components/ProtectedRoute.jsx` — redirects to /login if not authed
- `src/pages/Login.jsx` — login form with controlled inputs, calls AuthContext.login()
- `src/pages/Register.jsx` — register form, 3 fields with password rules
- `src/pages/Dashboard.jsx` — placeholder landing page after login
- `src/App.jsx` — rewired with BrowserRouter + Routes + AuthProvider wrapping

### React Concepts Used
- Context API (createContext, useContext, Provider)
- Custom hooks (useAuth)
- React Router (BrowserRouter, Routes, Route, Link, Navigate, useNavigate)
- Controlled inputs with single state object + computed property names
- async/await with try/catch/finally in event handlers
- Short-circuit and ternary conditional rendering

### HTTPS Fix
Changed Vite proxy target to `http://localhost:5170` (matches default .NET HTTP profile).
Conditionally disabled `UseHttpsRedirection()` in Development so proxy works.

---

## Session 6: Dashboard + Boards List + TanStack Query
**Date:** April 16, 2026
**Goal:** Real boards data with TanStack Query, create/delete board UI, theme toggle

### Files Created
- `src/main.jsx` — added QueryClientProvider with default options (staleTime, retry)
- `src/services/boardApi.js` — boardApi.getAll/getById/create/update/delete
- `src/components/Button.jsx` — reusable button with variant/size props
- `src/components/Modal.jsx` — dialog with backdrop, Esc key close, stopPropagation
- `src/components/CreateBoardModal.jsx` — form modal with useMutation
- `src/components/ThemeToggle.jsx` — Sun/Moon icon toggle with localStorage persistence
- `src/pages/BoardDetail.jsx` — placeholder page for /boards/:id route
- `src/pages/Dashboard.jsx` — rewrote to show boards grid with useQuery

### TanStack Query Concepts
- `useQuery({ queryKey, queryFn })` — for GET requests
- `useMutation({ mutationFn, onSuccess, onError })` — for POST/PUT/DELETE
- `queryClient.invalidateQueries({ queryKey })` — triggers refetch after mutation
- `isPending` — loading state during mutation
- staleTime — how long cached data is "fresh"

### UI States Handled
- Loading (spinner with "Loading boards...")
- Error (red alert with error message)
- Empty (dashed border card with "Create Board" CTA)
- Data (responsive grid: 1 col mobile, 2 col tablet, 3 col desktop)
- Hover (delete icon appears on card hover using `group-hover`)

---

## Session 7: Board Detail View — Columns + Tasks
**Date:** April 17, 2026
**Goal:** Build the actual Kanban view with task CRUD

### Files Created
- `src/services/taskApi.js` — taskApi.create/update/delete/move
- `src/lib/taskHelpers.js` — PRIORITY enum, PRIORITY_TO_VALUE map, PRIORITY_STYLES, formatDueDate, isOverdue
- `src/components/TaskCard.jsx` — single task display with priority badge, due date, hover actions
- `src/components/Column.jsx` — column with task list + Add Task button
- `src/components/TaskModal.jsx` — combined Create/Edit modal (task=null → create, task={...} → edit)
- `src/pages/BoardDetail.jsx` — rewritten with full board layout

### Patterns Used
- **Lifting state up** — modal state in BoardDetail (parent), passed callbacks to children
- **Same modal, two modes** — `task` prop being null vs object switches Create/Edit
- **Pass-through callbacks** — `onAddTask/onEditTask/onDeleteTask` flow Parent → Column → TaskCard
- **TanStack Query invalidation** — every mutation invalidates `['board', id]` to refetch board with all changes
- **Conditional UI states** — Loading, Error, Data with proper guards
- **Horizontal scroll** — `overflow-x-auto` for many columns, `flex-shrink-0` keeps columns from squishing

### Backend Integration
- GET /api/boards/:id — fetches board with all nested columns + tasks
- POST /api/tasks — create task in column
- PUT /api/tasks/:id — update task
- DELETE /api/tasks/:id — delete task
- (PUT /api/tasks/:id/move — wired but used in Session 9 for drag-drop)

### Result
- Click board card → opens board view
- 3 default columns visible (To Do, In Progress, Done)
- Seeded tasks display with priority badges
- Add task per column with title/description/priority/due date
- Edit task with same modal (prefilled values)
- Delete task with confirm dialog
- All updates auto-refresh via TanStack Query invalidation

---

## Session 8: Dashboard Stats + Debounced Search + UI Polish
**Date:** April 18-19, 2026
**Goal:** Polish UX — stats on dashboard, task search with debounce, animations, visual refinement

### Files Created
- `src/components/DashboardStats.jsx` — 4 metric cards (Boards, Total Tasks, Completed, Overdue)
- `src/hooks/useDebounce.js` — custom hook delaying value updates after inactivity
- `src/components/ThemeToggle.jsx` — Sun/Moon icon toggle (added earlier, used here too)

### Files Updated
- `src/pages/Dashboard.jsx` — mounted DashboardStats above boards grid
- `src/pages/BoardDetail.jsx` — added search bar with useDebounce + useMemo filtering
- `src/index.css` — added `@keyframes shake`, `@keyframes fadeIn` + `.animate-shake`, `.animate-fade-in` utility classes
- `src/pages/Login.jsx` + `Register.jsx` — shake animation on error messages, added `text-slate-900 dark:text-slate-50` for dark mode visibility fix
- `src/pages/BoardDetail.jsx` + `Dashboard.jsx` — added sticky headers, backdrop blur, shadows
- `src/components/TaskCard.jsx`, `Column.jsx`, `CreateBoardModal.jsx`, `TaskModal.jsx` — contrast improvements for dark mode

### React Concepts Used
- **Custom hook** (useDebounce) starting with `use` prefix — rule of hooks
- **useMemo** — cache expensive filter computation, only recompute when deps change
- **flatMap** — flatten nested arrays (`boards → columns → tasks`)
- **Timer + cleanup in useEffect** — classic debounce pattern
- **useEffect cleanup function** — cancel previous timer when value changes rapidly

### useDebounce Pattern
```js
export function useDebounce(value, delayMs = 300) {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    return () => clearTimeout(timer)  // cleanup cancels previous timer
  }, [value, delayMs])
  return debounced
}
```

### UI/UX Improvements (Evolution)
1. **First pass** — Plain gradient backgrounds (too washed out per user feedback)
2. **Second pass** — Blue+cyan gradient backgrounds (`from-blue-50 via-sky-100/50 to-cyan-50`)
3. **Third pass** — Added colored shadows (`shadow-blue-200/40`), hover lift (`hover:-translate-y-1`)
4. **Experimented with purple dark mode** — User reverted: felt off-brand
5. **Final** — Consistent cyan/blue accent in both light and dark modes

### Card Elevation Strategy
- **Light mode:** white cards + `shadow-lg shadow-blue-200/40` on colored background
- **Dark mode:** `bg-slate-800` cards (lighter than `bg-slate-950` page) for depth
- Hover: `hover:shadow-xl hover:-translate-y-1` for lift effect

### Animation Patterns
```css
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-6px); }
  75% { transform: translateX(6px); }
}
.animate-shake { animation: shake 0.3s ease-in-out; }
```
- Applied to error divs in Login/Register for immediate visual feedback on validation failure
- Triggers every time a new error string mounts (key-based re-render would repeat)

### Session 8 Result
- ✅ Dashboard stats: 4 metric cards with icons (LayoutGrid, Clock, CheckCircle, AlertTriangle)
- ✅ Task search: type to filter, 300ms debounce, match count text, X button to clear
- ✅ useDebounce hook ready for reuse (future use: search, filter, auto-save)
- ✅ Shake animation on login/register errors
- ✅ Light mode: blue/cyan gradient backgrounds with white cards popping
- ✅ Dark mode: clean slate-based with proper contrast layers
- ✅ Sticky headers with frosted glass (`backdrop-blur-md`)
- ✅ Form inputs: inset look (`bg-slate-50` → focus:`bg-white`)
- ✅ Cards hover with colored shadow and subtle lift

---

## Session 9: (Next)
**Planned:** Drag-and-drop tasks between columns using @dnd-kit — the marquee feature for the portfolio demo

