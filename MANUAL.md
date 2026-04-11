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

## Session 2: (Next)
**Planned:** MediatR CQRS setup + proper DTOs + Board/Task CRUD endpoints (Create, Update, Delete)

