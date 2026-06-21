# iNdex Todo — Full-Stack Mono-Repo

> **Your tasks, always in sync.**

A production-ready, cross-platform Todo application built with clean architecture on .NET 10.

---

## Repository Layout

```
iNdex-todo/
├── backend/                   ← .NET 10 REST API
│   ├── src/
│   │   ├── iNdex.Todo.Domain
│   │   ├── iNdex.Todo.Application
│   │   ├── iNdex.Todo.Infrastructure
│   │   ├── iNdex.Todo.Contracts
│   │   └── iNdex.Todo.API
│   ├── Dockerfile
│   └── iNdex.Todo.sln
│
├── mobile/                    ← .NET MAUI Blazor Hybrid (iOS / Android / Windows / macOS)
│   ├── src/
│   │   └── iNdex.Todo.Mobile
│   └── iNdex.Todo.Mobile.sln
│
├── docker-compose.yml         ← PostgreSQL + pgAdmin + API
├── .gitignore
└── README.md  ← you are here
```

---

## Tech Stack

### Backend
| Concern | Tech |
|---|---|
| Runtime | .NET 10 / ASP.NET Core Minimal APIs |
| Architecture | Clean Architecture + Vertical Slice + CQRS |
| Database | PostgreSQL 16 + EF Core 10 (Npgsql) |
| Validation | FluentValidation 11 |
| Real-time | ASP.NET Core SignalR |
| API Docs | OpenAPI + Scalar UI |
| Audit | Custom `AuditInterceptor` (soft-delete, timestamps, versioning) |

### Frontend
| Concern | Tech |
|---|---|
| Framework | .NET MAUI Blazor Hybrid |
| Components | MudBlazor 8 |
| API Client | Refit 8 (typed HTTP interfaces) |
| Real-time | SignalR Client |
| Platforms | Android · iOS · Windows · macOS · Browser |

---

## Quick Start (Local Dev)

### 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- MAUI workload (for mobile): `dotnet workload install maui`

### 2. Start the infrastructure

```bash
docker compose up -d
```

This starts:
- **PostgreSQL** on `localhost:5432`
- **pgAdmin** on `http://localhost:5050` (admin@index.local / admin)
- **API** on `http://localhost:5000`

> Migrations run automatically in Development mode on first API start.

### 3. Run just the API (without Docker)

```bash
cd backend
dotnet run --project src/iNdex.Todo.API
```

API will be available at:
- `http://localhost:5000/scalar/v1` — Scalar interactive docs
- `http://localhost:5000/openapi/v1.json` — OpenAPI spec
- `http://localhost:5000/health` — Health check

### 4. Run the mobile app

```bash
cd mobile

# Android emulator
dotnet build src/iNdex.Todo.Mobile -t:Run -f net10.0-android

# iOS simulator (Mac only)
dotnet build src/iNdex.Todo.Mobile -t:Run -f net10.0-ios

# Windows
dotnet build src/iNdex.Todo.Mobile -t:Run -f net10.0-windows10.0.19041.0
```

> Set `ApiBaseUrl` in `mobile/src/iNdex.Todo.Mobile/appsettings.json`:
> - Local dev: `https://localhost:5001`
> - Android Emulator: `https://10.0.2.2:5001`

---

## API Reference

### Users
| Method | Path | Description |
|---|---|---|
| `POST` | `/api/users` | Register a new user |
| `GET` | `/api/users/{id}` | Get user by ID |

### Todo Lists
| Method | Path | Description |
|---|---|---|
| `POST` | `/api/lists` | Create a list |
| `GET` | `/api/lists?ownerId={id}` | Get all lists for owner |
| `GET` | `/api/lists/{id}` | Get list by ID |
| `PUT` | `/api/lists/{id}` | Update a list |
| `DELETE` | `/api/lists/{id}` | Delete a list |

### Todo Tasks
| Method | Path | Description |
|---|---|---|
| `POST` | `/api/tasks` | Create a task |
| `GET` | `/api/tasks/by-list/{listId}` | Get tasks for a list |
| `GET` | `/api/tasks/{id}` | Get task by ID |
| `PUT` | `/api/tasks/{id}` | Update a task |
| `PATCH` | `/api/tasks/{id}/complete` | Complete / uncomplete |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

### System
| Method | Path | Description |
|---|---|---|
| `GET` | `/health` | Health check (DB connectivity) |
| `GET` | `/scalar/v1` | Scalar interactive docs (Dev only) |
| `GET` | `/openapi/v1.json` | OpenAPI JSON spec (Dev only) |

---

## Real-Time (SignalR)

**Endpoint:** `ws://localhost:5000/hubs/todo?userId={userId}`

### Server → Client events
| Event | Payload | Trigger |
|---|---|---|
| `TaskCreated` | `TodoTaskResponse` | New task added to user's list |
| `TaskUpdated` | `TodoTaskResponse` | Task edited or completed |
| `TaskDeleted` | `Guid` | Task deleted |
| `ListUpdated` | `TodoListResponse` | List metadata changed |

### Client → Server methods
| Method | Args | Effect |
|---|---|---|
| `JoinList` | `listId: string` | Subscribe to a specific list room |
| `LeaveList` | `listId: string` | Unsubscribe from list room |

---

## Database Migrations

```bash
cd backend

# Create a new migration
dotnet ef migrations add <MigrationName> \
  -p src/iNdex.Todo.Infrastructure \
  -s src/iNdex.Todo.API

# Apply to database
dotnet ef database update \
  -p src/iNdex.Todo.Infrastructure \
  -s src/iNdex.Todo.API
```

---

## Architecture — Backend

```
┌──────────────────────────────────────────────────────────────────┐
│ WebAPI (Minimal Endpoints)                                        │
│  UserEndpoints · TodoListEndpoints · TodoTaskEndpoints           │
└───────────────────────────────────┬──────────────────────────────┘
                                    │ IHandler<TReq, TRes>
┌───────────────────────────────────▼──────────────────────────────┐
│ Application (Vertical Slices — CQRS)                             │
│  Features/TodoLists/{Create,GetAll,GetById,Update,Delete}        │
│  Features/TodoTasks/{Create,GetByList,GetById,Update,Complete,Delete}│
│  Features/Users/{Register,GetById}                               │
│  Common/Result<T>  ·  IHandler<T,R>  ·  IRepository<T>          │
└──────────┬────────────────────────────────────────┬─────────────┘
           │                                        │
┌──────────▼──────────┐              ┌──────────────▼───────────────┐
│ Domain               │              │ Infrastructure                │
│  Entities (15)       │              │  EF Core + Npgsql             │
│  Enums               │              │  AuditInterceptor             │
│  Error type          │              │  Repositories                 │
│  BaseEntity          │              │  UnitOfWork                   │
└─────────────────────┘              │  SignalR (TodoHub)             │
                                     └──────────────────────────────┘
```

---

## Architecture — Frontend

```
MauiProgram.cs (DI root)
  ├── MudBlazor Services
  ├── Refit HTTP Clients  →  IUserApi / ITodoListApi / ITodoTaskApi
  ├── AppState (singleton)
  ├── TodoRealtimeService (SignalR)
  ├── NotificationService
  └── SyncQueueService

Components/
  App.razor         ← MudThemeProvider + Router
  Layout/
    MainLayout       ← AppBar (logo, avatar, dark-mode, SignalR status)
                        + Drawer nav
  Pages/
    Dashboard        ← Stats + recent lists
    Login / Register
    TodoLists        ← CRUD grid of lists
    TodoListDetail   ← Task list inside a list (+ live SignalR)
    AllTasks         ← Cross-list flat view + search + priority filter
    Settings         ← Profile, dark mode, about
  Shared/
    StatCard         ← Reusable metric card
    TaskCard         ← Single task row (checkbox, priority, due date, menu)
    ListFormDialog   ← Create/edit list (MudDialog)
    TaskFormDialog   ← Create/edit task (MudDialog)
    ConnectionStatus ← Live SignalR indicator icon
```

---

## Branding

| Token | Hex | Usage |
|---|---|---|
| Electric Blue | `#00AEEF` | Primary, links, "iNdex" text |
| Vibrant Green | `#39D353` | Secondary, checkmarks, success |
| Golden Orange | `#F5A623` | Tertiary, "Todo" text, FAB accents |
| Danger Red | `#EF4444` | Errors, critical priority |
| Dark BG | `#0F172A` | App background (dark mode) |
| Surface | `#1E293B` | Cards, drawers (dark mode) |
| Font | Inter | All UI text |

---

## CI/CD

| Workflow | Trigger | Jobs |
|---|---|---|
| `ci.yml` | Push / PR to main/develop | Backend build+test · Android build · Docker build |
| `backend-deploy.yml` | Push to `publish` branch | Publish .NET API artefact |
| `mobile-release.yml` | Tag `v*` | Release APK (Android) + MSIX (Windows) |
