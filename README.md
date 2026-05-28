# dotnet-azure-starter

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Azure](https://img.shields.io/badge/Azure-App%20Service%20%2B%20SQL-0078D4)](https://azure.microsoft.com)

**Production-ready ASP.NET Core 10 boilerplate** implementing Clean Architecture, the Result Pattern, and Unit of Work — ready to clone and build on.

> Built as a reference implementation for Senior Engineers and a starting point for new projects. Every pattern here has a reason; nothing is added speculatively.

> **Status:** Infrastructure & full CRUD API complete (Phase 3 of 7). Docker Compose is ready for local SQL Server. Azure and CI/CD are planned — see [Roadmap](#roadmap).

---

## Why this template?

Most boilerplates either under-engineer (no structure beyond `Controllers/`) or over-engineer (CQRS, MediatR, event sourcing from day one). This one sits in the middle:

- **Structured enough** to scale to a real product without refactoring everything
- **Simple enough** to understand in an afternoon and explain to a team
- **Opinionated** — the choices are made, documented, and enforced by the compiler

---

## Architecture

Clean Architecture with four strict layers. Dependencies flow inward only.

```
                 ┌─────────────────────────────────┐
                 │        API (Presentation)       │
                 │  Controllers · Middleware       │
                 └───────────┬────────────┬────────┘
                             │            │
                     depends on      depends on
                             │            │
                             ▼            ▼
              ┌──────────────────┐   ┌──────────────────────┐
              │  Infrastructure  │   │  Core (Domain)       │
              │  EF Core · Repos │   │  Entities · DTOs     │
              │  Services · UoW  │   │  Interfaces          │
              └──────────────────┘   │  Result · Error      │
                        │            │  ApiResponse         │
                   depends on        │  zero ext. deps      │
                        └───────────►└──────────────────────┘
```

| Allowed dependency | Forbidden |
|---|---|
| Api → Core | Core → Infrastructure |
| Api → Infrastructure | Core → Api |
| Infrastructure → Core | Infrastructure → Api |

Architecture tests (NetArchTest) will enforce these rules at build time in Phase 7 — a misplaced `using` will become a failing test, not a code review comment.

---

## Key Patterns

### Result Pattern — no exceptions for business failures

```csharp
// Service returns a typed result, not an exception
public async Task<Result<TodoResponseDto>> GetByIdAsync(Guid id)
{
    var entity = await _unitOfWork.Todos.GetByIdAsync(id);
    return entity is null
        ? Result<TodoResponseDto>.Failure(Error.NotFound($"Todo {id} not found"))
        : Result<TodoResponseDto>.Success(entity.ToDto());
}

// Controller maps the result to an HTTP response — no try/catch
return result.Match(
    onSuccess: dto  => Ok(ApiResponse<TodoResponseDto>.Ok(dto)),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound   => NotFound(ApiResponse<TodoResponseDto>.Fail(error)),
        ErrorType.Validation => UnprocessableEntity(ApiResponse<TodoResponseDto>.Fail(error)),
        _                    => StatusCode(500, ApiResponse<TodoResponseDto>.Fail(error))
    });
```

### Consistent API Envelope

Every endpoint returns the same JSON shape — success or failure:

```json
// 200 OK
{ "success": true,  "data": { "id": "...", "title": "..." }, "traceId": "abc123" }

// 404 Not Found
{ "success": false, "errorCode": "NOT_FOUND", "errorMessage": "Todo ... not found", "traceId": "abc123" }
```

### Unit of Work — atomic transactions

```csharp
// One CommitAsync — either both changes persist or neither does
await _unitOfWork.Todos.AddAsync(newTodo, ct);
await _unitOfWork.CommitAsync(ct);
```

### Pagination built in

```csharp
// Every list endpoint is paginated from day one
var result = await _todoService.GetAllAsync(new PaginationRequest(page: 1, pageSize: 20), ct);
// result.TotalPages · result.HasNextPage · result.Items
```

---

## Tech Stack

| Layer | Technology | Status |
|---|---|---|
| Runtime | .NET 10 | Implemented |
| API | ASP.NET Core Web API | Implemented |
| ORM | Entity Framework Core 10 | Implemented |
| Database (local) | SQL Server 2022 via Docker Compose | Implemented |
| Database (cloud) | Azure SQL Database | Planned — Phase 5 |
| Auth | Azure AD / Microsoft Entra ID | Planned — Phase 6+ |
| Cloud | Microsoft Azure (App Service, SQL, Key Vault) | Planned — Phase 5 |
| IaC | Azure Bicep | Planned — Phase 5 |
| CI/CD | GitHub Actions | Planned — Phase 6 |
| Containers | Docker + Docker Compose | Partial — DB only (Phase 4: full stack) |
| API Docs | Built-in .NET 10 OpenAPI | Implemented |
| Testing | xUnit · Moq · FluentAssertions · NetArchTest | Planned — Phase 7 |
| Validation | FluentValidation | Planned — Phase 7 |
| Logging | Serilog + Azure Application Insights | Planned — Phase 7 |

---

## Project Structure

```
dotnet-azure-starter/
├── DotnetAzureStarter.Api/              # HTTP layer — controllers, middleware, DI wiring
│   ├── Middleware/                      # (placeholder — correlation ID middleware planned)
│   └── Program.cs
│
├── DotnetAzureStarter.Core/             # Domain layer — zero external dependencies (Implemented)
│   ├── Common/
│   │   ├── Result.cs                    # Void result (Delete, Send)
│   │   ├── ResultOfT.cs                 # Typed result Result<T>
│   │   ├── Error.cs                     # Structured error value object
│   │   ├── ErrorType.cs                 # Error category → HTTP status mapping
│   │   ├── ApiResponse.cs               # Universal response envelope
│   │   ├── PagedResult.cs               # Paginated collection + metadata
│   │   └── PaginationRequest.cs         # Page / PageSize parameters
│   ├── Entities/                        # Domain entities (extend BaseEntity)
│   ├── Enums/                           # Type-safe domain enumerations
│   ├── Interfaces/                      # Repository + UnitOfWork abstractions
│   │   └── Services/                    # Service layer contracts
│   ├── DTOs/                            # Request / response shapes
│   └── Extensions/                      # Entity ↔ DTO mapping
│
├── DotnetAzureStarter.Infrastructure/   # Data access (Implemented)
│   ├── Data/
│   │   ├── AppDbContext.cs              # EF Core DbContext with auto-audit timestamps
│   │   ├── UnitOfWork.cs               # IUnitOfWork implementation — atomic CommitAsync
│   │   ├── DatabaseSeeder.cs           # Dev seed data (idempotent)
│   │   └── Configurations/             # EF Core Fluent API entity configurations
│   ├── Repositories/
│   │   ├── GenericRepository.cs        # Base CRUD + paginated queries
│   │   └── TodoRepository.cs           # Domain-specific queries (completed, priority, search)
│   ├── Services/
│   │   └── TodoService.cs              # Business logic, returns Result<T>
│   └── Options/
│       └── DatabaseOptions.cs          # Strongly typed DB config (Options Pattern)
│
├── DotnetAzureStarter.Core.Tests/       # Unit tests — test project scaffolded, tests planned Phase 7
├── DotnetAzureStarter.Api.Tests/        # Integration tests — test project scaffolded, tests planned Phase 7
│
├── infra/                               # Azure Bicep — App Service, SQL, Key Vault (planned Phase 5)
├── .github/workflows/                   # CI/CD pipelines (planned Phase 6)
├── docker-compose.yml                   # SQL Server 2022 local dev container (Implemented)
└── .editorconfig                        # Compiler-enforced code style
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (required once Phase 5 is complete)

### Run locally

```bash
# Clone
git clone https://github.com/ibuenuel/dotnet-azure-starter.git
cd dotnet-azure-starter

# 1. Start the SQL Server container
docker compose up -d

# 2. Build and run the API
#    On first startup: EF Core migrations run automatically and sample data is seeded
dotnet run --project DotnetAzureStarter.Api
```

```
API:        http://localhost:5290
OpenAPI:    http://localhost:5290/openapi/v1.json
Health:     http://localhost:5290/health
```

### Run tests

```bash
dotnet test
```

> Test projects are scaffolded. Meaningful unit, integration, and architecture tests will be added in Phase 7.

### Deploy to Azure

> Azure deployment via Bicep and GitHub Actions is planned for Phases 5 and 6.

---

## Docker

The `docker-compose.yml` starts a SQL Server 2022 container with a named volume for data persistence.

```bash
docker compose up -d        # start SQL Server in background
docker compose down         # stop (data volume is preserved)
docker compose down -v      # stop and delete all data
```

The API auto-migrates and seeds on startup in Development — no manual `dotnet ef database update` needed.

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/todos` | Paginated list (`?page=1&pageSize=20`) |
| `GET` | `/api/todos/{id}` | Single item by ID |
| `POST` | `/api/todos` | Create new item |
| `PUT` | `/api/todos/{id}` | Full update |
| `DELETE` | `/api/todos/{id}` | Delete by ID |
| `GET` | `/health` | Health check (DB connectivity) |
| `GET` | `/openapi/v1.json` | OpenAPI specification |

---

## Using this as a template

The Todo domain is a **reference implementation** — it demonstrates every pattern but is not the real content. Replacing it with your own domain takes about 30 minutes:

| Step | What to do | Where |
|---|---|---|
| 1 | Create your entity extending `BaseEntity` | `Core/Entities/` |
| 2 | Add domain-specific enums | `Core/Enums/` |
| 3 | Create `IYourRepository : IRepository<YourEntity>` | `Core/Interfaces/` |
| 4 | Replace `ITodoRepository Todos` with your repository | `Core/Interfaces/IUnitOfWork.cs` |
| 5 | Create `IYourService` | `Core/Interfaces/Services/` |
| 6 | Create request/response DTOs | `Core/DTOs/` |
| 7 | Add mapping extensions | `Core/Extensions/` |
| 8 | Implement the repository and service | `Infrastructure/` |
| 9 | Register in DI | `Api/Program.cs` |

Everything in `Common/` — `Result`, `Error`, `ApiResponse`, `PagedResult`, `IRepository<T>`, `BaseEntity` — requires no changes.

---

## Engineering Standards

This boilerplate is built around the following non-negotiable standards:

**SOLID** — Single Responsibility per class, Dependency Inversion throughout, Interface Segregation between generic and specialised repositories.

**Result Pattern over exceptions** — Service methods return `Result<T>` with a typed `Error` value object. HTTP status codes are derived from `ErrorType`, not string matching.

**Sealed & immutable by default** — Value objects (`Error`, `PaginationRequest`) are `sealed record`. `PagedResult<T>` exposes `IReadOnlyList<T>`.

**DateTimeOffset everywhere** — No `DateTime` in the codebase. Azure SQL stores `datetimeoffset`; the code matches.

**Zero magic numbers** — Domain constants are enums (`TodoPriority`), never `int` with range annotations.

**TreatWarningsAsErrors** — A missing XML doc comment on a public API is a build failure, not a warning.

**Architecture tests** *(planned Phase 7)* — Layer dependency violations will be caught at build time via NetArchTest.

---

## Roadmap

- [x] Phase 1 — Solution scaffold, Clean Architecture structure, Git setup
- [x] Phase 2 — Core domain: entities, interfaces, Result pattern, DTOs, value objects
- [x] Phase 3 — Infrastructure: EF Core, repositories, Unit of Work, service implementations, controllers
- [ ] Phase 4 — Docker: multi-stage Dockerfile, full docker-compose stack (API + DB), health checks
- [ ] Phase 5 — Azure Bicep: App Service, SQL Database, Key Vault
- [ ] Phase 6 — GitHub Actions: CI pipeline, CD to Azure App Service
- [ ] Phase 7 — Polish: architecture tests, Serilog, Application Insights, FluentValidation, README badges

---

## Azure Cost Estimate

| Resource | Tier | Monthly |
|---|---|---|
| App Service Plan | F1 (Free) | $0 |
| Azure SQL Database | Basic 5 DTU | ~$5 |
| Azure Container Registry | Basic | ~$5 |
| Azure Key Vault | Standard | ~$0.03 |
| **Total** | | **~$10/mo** |

> Fully covered by the Azure free trial ($200 credit). The F1 App Service tier is permanently free.

---

## License

MIT — see [LICENSE](LICENSE).
