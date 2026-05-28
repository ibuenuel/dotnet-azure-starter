# dotnet-azure-starter

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Azure](https://img.shields.io/badge/Azure-App%20Service%20%2B%20SQL-0078D4)](https://azure.microsoft.com)
[![CI](https://github.com/ibuenuel/dotnet-azure-starter/actions/workflows/ci.yml/badge.svg)](https://github.com/ibuenuel/dotnet-azure-starter/actions/workflows/ci.yml)
[![CD](https://github.com/ibuenuel/dotnet-azure-starter/actions/workflows/cd.yml/badge.svg)](https://github.com/ibuenuel/dotnet-azure-starter/actions/workflows/cd.yml)

**Production-ready ASP.NET Core 10 boilerplate** implementing Clean Architecture, the Result Pattern, and Unit of Work — ready to clone and build on.

> Built as a reference implementation for Senior Engineers and a starting point for new projects. Every pattern here has a reason; nothing is added speculatively.

> **Status:** Phases 1–7 complete. 72 tests (unit, integration, architecture) — all green. Azure Bicep IaC + GitHub Actions CI/CD fully implemented.

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

Architecture tests (NetArchTest) enforce these rules at build time — a misplaced `using` is a failing test, not a code review comment.

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
| Database (cloud) | Azure SQL Database | Implemented |
| Auth | Azure AD / Microsoft Entra ID | Planned — Phase 7+ |
| Cloud | Microsoft Azure (App Service, SQL, Key Vault) | Implemented |
| IaC | Azure Bicep | Implemented — |
| CI/CD | GitHub Actions | Implemented |
| Containers | Docker + Docker Compose | Implemented |
| API Docs | Built-in .NET 10 OpenAPI | Implemented |
| Testing | xUnit · Moq · FluentAssertions · NetArchTest · Testcontainers | Implemented |
| Validation | FluentValidation | Planned — Phase 8 |
| Logging | Serilog + Azure Application Insights | Planned — Phase 8 |

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
├── DotnetAzureStarter.Core.Tests/       # Unit + architecture tests (62 tests — xUnit, Moq, NetArchTest)
│   ├── Common/                          # Result, Error, ApiResponse, PagedResult, PaginationRequest
│   ├── Extensions/                      # Entity ↔ DTO mapping
│   ├── Services/                        # TodoService (Moq-based, IUnitOfWork mocked)
│   └── Architecture/                    # Layer dependency rules (NetArchTest)
│
├── DotnetAzureStarter.Api.Tests/        # Integration tests (10 tests — WebApplicationFactory + Testcontainers)
│   ├── Fixtures/                        # TodoApiFixture — real SQL Server container per test class
│   └── Controllers/                     # Full CRUD + health check end-to-end
│
├── infra/                               # Azure Bicep IaC (Implemented)
│   ├── main.bicep                       # Orchestrator — wires all modules + KV access policy
│   ├── app-service.bicep                # App Service Plan (F1) + Web App + App Insights + ACR pull
│   ├── sql-server.bicep                 # Azure SQL Server + Database (Basic) + firewall
│   ├── keyvault.bicep                   # Key Vault + DatabaseConnectionString secret
│   ├── acr.bicep                        # Azure Container Registry (Basic)
│   └── parameters.json                  # Dev environment parameters
├── .github/workflows/                   # CI/CD pipelines (Implemented)
│   ├── ci.yml                           # PR trigger: restore → build → unit/arch/integration tests
│   └── cd.yml                           # Push to main: test → ACR push → App Service deploy → migrations
├── docker-compose.yml                   # Full stack: API + SQL Server 2022 (Implemented)
├── docker-compose.override.yml          # Local dev overrides (Implemented)
├── Dockerfile                           # Multi-stage API container (Implemented)
├── .env.example                         # Environment variable template (Implemented)
└── .editorconfig                        # Compiler-enforced code style
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (required for Azure deployment)

### Run locally (Docker — recommended)

```bash
# Clone
git clone https://github.com/ibuenuel/dotnet-azure-starter.git
cd dotnet-azure-starter

# Copy environment file (defaults work out of the box)
cp .env.example .env

# Start the full stack — API + SQL Server
# On first startup: EF Core migrations run automatically and sample data is seeded
docker compose up --build
```

```
API:        http://localhost:8080
OpenAPI:    http://localhost:8080/openapi/v1.json
Health:     http://localhost:8080/health
```

### Run locally (without Docker)

```bash
# Start only the database
docker compose up database -d

# Run the API against localhost SQL Server
dotnet run --project DotnetAzureStarter.Api
```

```
API:        http://localhost:5290
OpenAPI:    http://localhost:5290/openapi/v1.json
Health:     http://localhost:5290/health
```

### Run tests

```bash
# All tests (unit + architecture + integration — Docker required for integration)
dotnet test

# Unit and architecture tests only (no Docker needed)
dotnet test DotnetAzureStarter.Core.Tests

# Integration tests only (starts a real SQL Server container via Testcontainers)
dotnet test DotnetAzureStarter.Api.Tests

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

| Suite | Count | Requires Docker |
|---|---|---|
| Unit tests — `Core.Tests` | 57 | No |
| Architecture tests — `Core.Tests` | 5 | No |
| Integration tests — `Api.Tests` | 10 | Yes |
| **Total** | **72** | |

Integration tests spin up a dedicated SQL Server container automatically (Testcontainers). No manual setup needed — Docker running is sufficient.

### Deploy to Azure

The full Azure stack (App Service, SQL Database, Key Vault, App Insights) is provisioned with a single command via Azure Bicep.

**1. Create the resource group (once):**

```bash
az group create --name rg-dotnetazstarter-dev --location westeurope
```

**2. Validate the template (no resources created):**

```bash
az deployment group validate --resource-group rg-dotnetazstarter-dev --template-file infra/main.bicep --parameters @infra/parameters.json --parameters sqlAdminLogin=sqladmin sqlAdminPassword=<your-password>
```

**3. Preview what will be created (What-If):**

```bash
az deployment group what-if --resource-group rg-dotnetazstarter-dev --template-file infra/main.bicep --parameters @infra/parameters.json --parameters sqlAdminLogin=sqladmin sqlAdminPassword=<your-password>
```

**4. Deploy:**

```bash
az deployment group create --resource-group rg-dotnetazstarter-dev --template-file infra/main.bicep --parameters @infra/parameters.json --parameters sqlAdminLogin=sqladmin sqlAdminPassword=<your-password>
```

> **PowerShell users:** Wrap passwords containing `!` in single quotes: `sqlAdminPassword='YourStr0ng!Pwd'`

**5. Verify the deployment:**

```bash
# Check the app is running
az webapp show --name app-dotnetazstarter-dev --resource-group rg-dotnetazstarter-dev --query state

# Hit the health endpoint
curl https://app-dotnetazstarter-dev.azurewebsites.net/health
```

**Provisioned resources:**

| Resource | Name | Tier |
|---|---|---|
| App Service Plan | `asp-dotnetazstarter-dev` | F1 (Free) |
| Web App (Container) | `app-dotnetazstarter-dev` | F1 |
| Azure SQL Server | `sql-dotnetazstarter-dev` | — |
| Azure SQL Database | `StarterDb` | Basic (5 DTU) |
| Key Vault | `kv-dotnetazstarter-dev` | Standard |
| Container Registry | `dotnetazstarterdevacr` | Basic |
| Application Insights | `appi-dotnetazstarter-dev` | Pay-as-you-go |
| Log Analytics Workspace | `log-dotnetazstarter-dev` | Pay-as-you-go |

The database connection string is stored as a Key Vault secret and injected into the App Service at runtime via a [Key Vault reference](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references) — no secrets in app settings or source control.

---

## CI/CD

GitHub Actions automates the full build → test → deploy pipeline.

### CI (`ci.yml`) — runs on every PR to `main`

```
Checkout → Setup .NET 10 → Restore → Build
  → Unit & Architecture Tests (Core.Tests, no Docker)
  → Integration Tests (Api.Tests, Testcontainers spins up SQL Server)
  → Upload .trx test results as artifact
```

Integration tests use [Testcontainers.MsSql](https://dotnet.testcontainers.org/) — Docker is pre-installed on `ubuntu-latest`, no manual setup needed.

### CD (`cd.yml`) — runs on push to `main`

```
test job:   same steps as CI
deploy job: Azure login
            → Build Docker image → Push to ACR (tagged with commit SHA + latest)
            → Deploy to App Service (azure/webapps-deploy@v3 pulls the SHA-tagged image)
            → Open SQL firewall for runner IP → Run EF migrations → Close firewall
            → Health check /health (waits 30s for F1 cold start)
```

The `deploy` job only runs if `test` passes. The SQL firewall is always closed after migrations (`if: always()`).

### One-time GitHub Secrets setup

**1. Create the Service Principal:**

```bash
SUBSCRIPTION_ID=$(az account show --query id --output tsv)
RESOURCE_GROUP="rg-dotnetazstarter-dev"

# Outputs JSON — copy the entire block as the AZURE_CREDENTIALS secret value
az ad sp create-for-rbac \
  --name "sp-dotnetazstarter-github-actions" \
  --role Contributor \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP" \
  --sdk-auth
```

**2. Grant the SP `AcrPush` on the registry:**

```bash
az role assignment create \
  --assignee "sp-dotnetazstarter-github-actions" \
  --role AcrPush \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.ContainerRegistry/registries/dotnetazstarterdevacr"
```

**3. Add these secrets to the GitHub repository** (Settings → Secrets → Actions):

| Secret | Value |
|---|---|
| `AZURE_CREDENTIALS` | Full JSON from `az ad sp create-for-rbac --sdk-auth` |
| `AZURE_RESOURCE_GROUP` | `rg-dotnetazstarter-dev` |
| `AZURE_WEBAPP_NAME` | `app-dotnetazstarter-dev` |
| `ACR_LOGIN_SERVER` | `dotnetazstarterdevacr.azurecr.io` |
| `AZURE_SQL_CONNECTION_STRING` | `Server=tcp:sql-dotnetazstarter-dev.database.windows.net,1433;Database=StarterDb;User Id=sqladmin;Password=...;Encrypt=True;TrustServerCertificate=False;` |
| `AZURE_PREFIX` | `dotnetazstarter` |
| `AZURE_ENVIRONMENT` | `dev` |

`AZURE_PREFIX` and `AZURE_ENVIRONMENT` let the workflow reconstruct the SQL server name (`sql-{prefix}-{environment}`) using the same naming formula as the Bicep, without an additional secret for the full server name.

---

## Docker

`docker compose up --build` starts the full stack — API and SQL Server 2022 — with a single command.

```bash
docker compose up --build       # build API image and start full stack
docker compose up --build -d    # same, detached (background)
docker compose down             # stop (data volume is preserved)
docker compose down -v          # stop and wipe all data
```

The API container waits for SQL Server to pass its health check before starting (`depends_on: condition: service_healthy`). On first startup, EF Core migrations run automatically and sample data is seeded — no manual steps needed.

`GET /health` probes DB connectivity via EF Core — returns `Healthy` / `Unhealthy` based on whether SQL Server is reachable.

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

**Architecture tests** — Layer dependency violations are caught at build time via NetArchTest. Five rules covering all forbidden cross-layer dependencies run as part of every `dotnet test` invocation.

---

## Roadmap

- [x] Phase 1 — Solution scaffold, Clean Architecture structure, Git setup
- [x] Phase 2 — Core domain: entities, interfaces, Result pattern, DTOs, value objects
- [x] Phase 3 — Infrastructure: EF Core, repositories, Unit of Work, service implementations, controllers
- [x] Phase 4 — Docker: multi-stage Dockerfile, full docker-compose stack (API + DB), health checks
- [x] Phase 5 — Tests: 72 tests across unit, architecture (NetArchTest), and integration (Testcontainers)
- [x] Phase 6 — Azure Bicep: App Service, SQL Database, Key Vault, App Insights — one-command provisioning
- [x] Phase 7 — GitHub Actions: CI on PR (72 tests), CD to Azure App Service via Docker + ACR
- [ ] Phase 8 — Polish: Serilog, Application Insights, FluentValidation

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
