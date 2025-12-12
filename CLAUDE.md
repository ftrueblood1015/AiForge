# AiForge - Ticket Management API for Claude Integration

You are an expert C#, .NET 8, and Entity Framework Core developer building a ticket management system with "show your work" transparency features designed for Claude integration.

## Project Overview

**AiForge** is a ticket management API that allows Claude to track its work transparently. Key features:
- Ticket/task management with JIRA-like workflow
- Planning sessions and reasoning logs ("show your work")
- Handoff documents for context preservation between sessions
- MCP server for direct Claude integration

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | C# .NET 8, ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server Express (local) |
| Frontend | React 18 + Vite + TypeScript |
| UI Library | MUI (Material UI) v5 |
| State Management | Zustand |
| MCP Server | C# (custom implementation) |
| Auth | API Key (GUID) via X-Api-Key header |

## Project Structure

```
AiForge/
├── src/
│   ├── AiForge.Domain/           # Entities, enums, interfaces (no dependencies)
│   ├── AiForge.Infrastructure/   # EF Core, DbContext, repositories
│   ├── AiForge.Application/      # Services, DTOs, validators, mapping
│   ├── AiForge.Api/              # Controllers, middleware, Program.cs
│   └── AiForge.Mcp/              # MCP Server for Claude integration
├── frontend/
│   └── aiforge-ui/               # React + Vite + TypeScript
├── tests/
│   ├── AiForge.Api.Tests/
│   └── AiForge.Application.Tests/
└── AiForge.sln
```

## C# Coding Standards

### General
- Use C# 12 features (primary constructors, collection expressions, etc.)
- Enable nullable reference types (`#nullable enable`)
- Use `async/await` consistently - never block with `.Result` or `.Wait()`
- Follow Microsoft C# naming conventions (PascalCase for public, _camelCase for private fields)
- Use expression-bodied members where they improve readability
- Prefer `var` when the type is obvious from the right side

### Entity Framework Core
- Always use async methods (`SaveChangesAsync`, `ToListAsync`, `FirstOrDefaultAsync`)
- Configure relationships and constraints with Fluent API in `Configurations/` folder
- Use migrations with descriptive names: `dotnet ef migrations add AddTicketPriorityColumn`
- Avoid N+1 queries - use `.Include()` or projection with `.Select()`
- Use `AsNoTracking()` for read-only queries
- Keep DbContext lifetime scoped (one per request)

### API Design
- Follow RESTful conventions (GET for reads, POST for creates, PUT for full updates, PATCH for partial)
- Return appropriate HTTP status codes (200, 201, 204, 400, 401, 404, 409, 500)
- Use DTOs to separate API contracts from domain models
- Validate input with FluentValidation
- Use `[ApiController]` attribute for automatic model validation
- Return `ActionResult<T>` from controller actions

### Architecture (Clean Architecture)
- **Domain**: Entities, enums, interfaces only. No dependencies on other projects.
- **Infrastructure**: EF Core, external services. References Domain.
- **Application**: Business logic, DTOs, services. References Domain.
- **Api**: Controllers, middleware. References Application and Infrastructure.

### Dependency Injection
- Register services in `Program.cs` using extension methods
- Use constructor injection exclusively
- Prefer interfaces over concrete types
- Scope DbContext and repositories to request lifetime

### Error Handling
- Use custom exception types (e.g., `NotFoundException`, `ValidationException`)
- Handle exceptions globally with middleware
- Log errors with structured logging (Serilog recommended)
- Never expose stack traces in production responses

## Common Commands

### Build & Run
```bash
# Build solution
dotnet build

# Run API (with hot reload)
dotnet watch run --project src/AiForge.Api

# Run API (production mode)
dotnet run --project src/AiForge.Api -c Release
```

### Database (Entity Framework)
```bash
# Add migration
dotnet ef migrations add MigrationName -p src/AiForge.Infrastructure -s src/AiForge.Api

# Update database
dotnet ef database update -p src/AiForge.Infrastructure -s src/AiForge.Api

# Revert last migration
dotnet ef migrations remove -p src/AiForge.Infrastructure -s src/AiForge.Api

# Generate SQL script
dotnet ef migrations script -p src/AiForge.Infrastructure -s src/AiForge.Api
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/AiForge.Application.Tests
```

### Frontend
```bash
cd frontend/aiforge-ui
npm install
npm run dev      # Development server
npm run build    # Production build
```

## Key Domain Entities

### Core Entities
- **Project**: Container for tickets (has Key like "AIFORGE")
- **Ticket**: Work item with status, type, priority
- **Comment**: Notes on tickets
- **TicketHistory**: Audit trail of changes

### AI/Planning Entities (Show Your Work)
- **PlanningSession**: Claude's initial understanding and approach for a ticket
- **ReasoningLog**: Decisions made with rationale and confidence
- **ProgressEntry**: Work done/attempted with outcome
- **HandoffDocument**: Context preservation between sessions
- **FileSnapshot**: Before/after code snapshots

### Status Flow
```
ToDo → InProgress → InReview → Done
```

## API Authentication

All endpoints (except /health, /swagger) require `X-Api-Key` header:
```
X-Api-Key: {guid-api-key}
```

Rate limiting is per API key, tracked in `ApiKeyUsage` table.

## When Implementing Features

1. **Domain First**: Define entities/enums in `AiForge.Domain`
2. **Configure EF**: Add Fluent API config in `Infrastructure/Data/Configurations/`
3. **Create Migration**: `dotnet ef migrations add ...`
4. **Add DTOs**: Create request/response DTOs in `Application/DTOs/`
5. **Implement Service**: Business logic in `Application/Services/`
6. **Add Controller**: API endpoint in `Api/Controllers/`
7. **Test**: Write unit tests for services, integration tests for endpoints

## Important Notes

- The MCP server (`AiForge.Mcp`) communicates via stdio with Claude
- Handoff documents use structured JSON in `StructuredContext` for machine parsing
- File snapshots enable showing diffs in the UI
- Always maintain backward compatibility in API responses
