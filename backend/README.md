# iNdex Todo API

A production-ready .NET 10 REST API for the iNdex Todo application, built with **Clean Architecture**, **Vertical Slice Architecture**, and **CQRS** principles.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core Minimal APIs |
| Database | PostgreSQL + Entity Framework Core 10 (Npgsql) |
| Validation | FluentValidation |
| Real-Time | SignalR |
| API Docs | OpenAPI + Scalar UI |
| Architecture | Clean Architecture + Vertical Slice + CQRS |

---

## Solution Structure

```
iNdex.Todo/
├── src/
│   ├── iNdex.Todo.Domain          # Entities, Enums, Errors — no dependencies
│   ├── iNdex.Todo.Application     # Vertical slice features, CQRS handlers, validators
│   ├── iNdex.Todo.Infrastructure  # EF Core, Repositories, SignalR, AuditInterceptor
│   ├── iNdex.Todo.Contracts       # Request/Response records (shared DTOs)
│   └── iNdex.Todo.API             # Minimal API endpoints, Program.cs, Middleware
├── tests/
│   └── (test projects go here)
├── .github/workflows/
│   ├── backend-ci.yml
│   └── backend-deploy.yml
└── README.md
```

### Dependency Flow

```
API → Infrastructure → Application → Domain
         ↑___________________________|
               (Contracts shared)
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 15+](https://www.postgresql.org/)

### 1. Configure the database

Edit `src/iNdex.Todo.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=index_todo;Username=postgres;Password=yourpassword"
  }
}
```

### 2. Run migrations

```bash
dotnet ef migrations add InitialCreate -p src/iNdex.Todo.Infrastructure -s src/iNdex.Todo.API
dotnet ef database update -p src/iNdex.Todo.Infrastructure -s src/iNdex.Todo.API
```

> In Development mode, migrations are applied automatically on startup.

### 3. Run the API

```bash
cd src/iNdex.Todo.API
dotnet run
```

---

## API Reference

| Method | Path | Description |
|---|---|---|
| POST | `/api/users` | Register a user |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/lists` | Create a todo list |
| GET | `/api/lists?ownerId={id}` | Get all lists for owner |
| GET | `/api/lists/{id}` | Get list by ID |
| PUT | `/api/lists/{id}` | Update a list |
| DELETE | `/api/lists/{id}` | Delete a list |
| POST | `/api/tasks` | Create a task |
| GET | `/api/tasks/by-list/{listId}` | Get tasks for a list |
| GET | `/api/tasks/{id}` | Get task by ID |
| PUT | `/api/tasks/{id}` | Update a task |
| PATCH | `/api/tasks/{id}/complete` | Mark task complete/incomplete |
| DELETE | `/api/tasks/{id}` | Delete a task |
| GET | `/health` | Health check |

### Interactive Docs (Scalar UI)

Available in Development at: `http://localhost:5000/scalar/v1`

OpenAPI JSON spec: `http://localhost:5000/openapi/v1.json`

---

## Real-Time (SignalR)

Connect to: `ws://localhost:5000/hubs/todo?userId={userId}`

### Client Events (server → client)

| Event | Payload | Description |
|---|---|---|
| `TaskCreated` | `TodoTaskResponse` | A new task was created |
| `TaskUpdated` | `TodoTaskResponse` | A task was updated |
| `TaskDeleted` | `Guid` | A task was deleted |
| `ListUpdated` | `TodoListResponse` | A list was updated |

### Hub Methods (client → server)

| Method | Args | Description |
|---|---|---|
| `JoinList` | `listId: string` | Subscribe to a specific list's updates |
| `LeaveList` | `listId: string` | Unsubscribe from a list |

---

## Architecture Patterns

### Vertical Slice Feature Structure

Each feature is fully self-contained:

```
Application/Features/TodoTasks/CreateTodoTask/
  ├── CreateTodoTaskHandler.cs    ← Business logic + IHandler<TReq, TRes>
  └── CreateTodoTaskValidator.cs  ← FluentValidation rules
```

### CQRS

- **Commands** (mutate state): `Create`, `Update`, `Delete`, `CompleteTask`
- **Queries** (read state): `GetAll`, `GetById`, `GetByList`
- All handlers return `Result<T>` — no exceptions for business failures

### Soft Deletes

All entities use soft delete via the `AuditInterceptor`. When `DbSet.Remove()` is called, the interceptor intercepts it and sets `IsDeleted = true` instead. Global query filters exclude soft-deleted records automatically.

---

## Adding a New Feature

1. Create folder: `Application/Features/NewFeature/ActionName/`
2. Add `ActionNameHandler.cs` implementing `IHandler<TRequest, TResponse>`
3. Add `ActionNameValidator.cs` extending `AbstractValidator<TRequest>`
4. Register handler in `ApplicationServiceRegistration.cs`
5. Add endpoint method to the relevant `*Endpoints.cs` file in the API layer

---

## Branding

Primary colors (from the iNdex Todo logo):

| Color | Hex |
|---|---|
| Electric Blue | `#00AEEF` |
| Vibrant Green | `#39D353` |
| Golden Orange | `#F5A623` |
| Dark Background | `#0F172A` |

Light and Dark mode are user-toggled and persisted via `UserSettings`.
