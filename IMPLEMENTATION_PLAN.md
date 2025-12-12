# AiForge Implementation Plan

A ticket management API designed for Claude integration with "show your work" transparency features.

---

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
| Auth | API Key (GUID) via header |

---

## Project Structure

```
C:\Users\ftrue\source\repos\AiForge\
├── src/
│   ├── AiForge.Domain/                 # Entities, enums, interfaces
│   │   ├── Entities/
│   │   │   ├── Project.cs
│   │   │   ├── Ticket.cs
│   │   │   ├── Comment.cs
│   │   │   ├── TicketHistory.cs
│   │   │   ├── PlanningSession.cs
│   │   │   ├── ReasoningLog.cs
│   │   │   ├── ProgressEntry.cs
│   │   │   ├── HandoffDocument.cs
│   │   │   ├── HandoffVersion.cs
│   │   │   ├── FileSnapshot.cs
│   │   │   ├── ApiKey.cs
│   │   │   └── ApiKeyUsage.cs
│   │   ├── Enums/
│   │   │   ├── TicketStatus.cs
│   │   │   ├── TicketType.cs
│   │   │   ├── Priority.cs
│   │   │   ├── HandoffType.cs
│   │   │   └── ProgressOutcome.cs
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── AiForge.Infrastructure/         # EF Core, data access
│   │   ├── Data/
│   │   │   ├── AiForgeDbContext.cs
│   │   │   ├── Configurations/         # EF Fluent API configs
│   │   │   │   ├── ProjectConfiguration.cs
│   │   │   │   ├── TicketConfiguration.cs
│   │   │   │   └── ...
│   │   │   └── Migrations/
│   │   └── Repositories/
│   │       ├── ProjectRepository.cs
│   │       ├── TicketRepository.cs
│   │       └── ...
│   │
│   ├── AiForge.Application/            # Business logic, DTOs, services
│   │   ├── DTOs/
│   │   │   ├── Projects/
│   │   │   │   ├── ProjectDto.cs
│   │   │   │   ├── CreateProjectRequest.cs
│   │   │   │   └── UpdateProjectRequest.cs
│   │   │   ├── Tickets/
│   │   │   │   ├── TicketDto.cs
│   │   │   │   ├── TicketDetailDto.cs
│   │   │   │   ├── CreateTicketRequest.cs
│   │   │   │   └── UpdateTicketRequest.cs
│   │   │   ├── Planning/
│   │   │   │   ├── PlanningSessionDto.cs
│   │   │   │   ├── ReasoningLogDto.cs
│   │   │   │   └── ProgressEntryDto.cs
│   │   │   └── Handoffs/
│   │   │       ├── HandoffDto.cs
│   │   │       ├── CreateHandoffRequest.cs
│   │   │       └── HandoffContextDto.cs
│   │   ├── Services/
│   │   │   ├── ProjectService.cs
│   │   │   ├── TicketService.cs
│   │   │   ├── PlanningService.cs
│   │   │   ├── HandoffService.cs
│   │   │   └── SearchService.cs
│   │   ├── Mapping/
│   │   │   └── MappingProfile.cs       # AutoMapper profiles
│   │   └── Validators/
│   │       ├── CreateTicketValidator.cs
│   │       └── ...
│   │
│   ├── AiForge.Api/                    # Web API
│   │   ├── Controllers/
│   │   │   ├── ProjectsController.cs
│   │   │   ├── TicketsController.cs
│   │   │   ├── PlanningController.cs
│   │   │   ├── HandoffsController.cs
│   │   │   └── SearchController.cs
│   │   ├── Middleware/
│   │   │   └── ApiKeyAuthMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   │
│   └── AiForge.Mcp/                    # MCP Server for Claude
│       ├── McpServer.cs
│       ├── Tools/
│       │   ├── TicketTools.cs
│       │   ├── PlanningTools.cs
│       │   └── HandoffTools.cs
│       └── Program.cs
│
├── frontend/                           # React application
│   └── aiforge-ui/
│       ├── src/
│       │   ├── api/
│       │   │   ├── client.ts           # Axios instance with API key
│       │   │   ├── projects.ts
│       │   │   ├── tickets.ts
│       │   │   ├── planning.ts
│       │   │   └── handoffs.ts
│       │   ├── components/
│       │   │   ├── layout/
│       │   │   │   ├── AppLayout.tsx
│       │   │   │   ├── Sidebar.tsx
│       │   │   │   └── Header.tsx
│       │   │   ├── tickets/
│       │   │   │   ├── TicketCard.tsx
│       │   │   │   ├── TicketDetail.tsx
│       │   │   │   ├── TicketForm.tsx
│       │   │   │   └── TicketBoard.tsx
│       │   │   ├── planning/
│       │   │   │   ├── PlanningTimeline.tsx
│       │   │   │   ├── ReasoningCard.tsx
│       │   │   │   └── ProgressLog.tsx
│       │   │   └── handoffs/
│       │   │       ├── HandoffViewer.tsx
│       │   │       ├── CodeSnippet.tsx
│       │   │       └── FileDiff.tsx
│       │   ├── pages/
│       │   │   ├── Dashboard.tsx
│       │   │   ├── ProjectList.tsx
│       │   │   ├── ProjectDetail.tsx
│       │   │   ├── TicketDetail.tsx
│       │   │   ├── HandoffList.tsx
│       │   │   └── Settings.tsx
│       │   ├── stores/
│       │   │   ├── projectStore.ts
│       │   │   ├── ticketStore.ts
│       │   │   └── uiStore.ts
│       │   ├── hooks/
│       │   │   └── useApi.ts
│       │   ├── types/
│       │   │   └── index.ts
│       │   ├── App.tsx
│       │   ├── main.tsx
│       │   └── theme.ts               # MUI theme customization
│       ├── index.html
│       ├── package.json
│       ├── tsconfig.json
│       └── vite.config.ts
│
├── tests/
│   ├── AiForge.Api.Tests/
│   └── AiForge.Application.Tests/
│
├── AiForge.sln
└── README.md
```

---

## Domain Models

### Enums

```csharp
// TicketStatus.cs
public enum TicketStatus
{
    ToDo = 0,
    InProgress = 1,
    InReview = 2,
    Done = 3
}

// TicketType.cs
public enum TicketType
{
    Task = 0,
    Bug = 1,
    Feature = 2,
    Enhancement = 3
}

// Priority.cs
public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

// HandoffType.cs
public enum HandoffType
{
    SessionEnd = 0,
    Blocker = 1,
    Milestone = 2,
    ContextDump = 3
}

// ProgressOutcome.cs
public enum ProgressOutcome
{
    Success = 0,
    Failure = 1,
    Partial = 2,
    Blocked = 3
}
```

### Core Entities

```csharp
// Project.cs
public class Project
{
    public Guid Id { get; set; }
    public string Key { get; set; }              // e.g., "AIFORGE", "MYPROJ"
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int NextTicketNumber { get; set; }    // For generating PROJ-1, PROJ-2, etc.

    // Navigation
    public ICollection<Ticket> Tickets { get; set; }
}

// Ticket.cs
public class Ticket
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Key { get; set; }              // e.g., "AIFORGE-123"
    public int Number { get; set; }              // Sequential within project
    public string Title { get; set; }
    public string? Description { get; set; }
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Guid? ParentTicketId { get; set; }    // For sub-tasks
    public string? CurrentHandoffSummary { get; set; }  // Quick context
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Project Project { get; set; }
    public Ticket? ParentTicket { get; set; }
    public ICollection<Ticket> SubTickets { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<TicketHistory> History { get; set; }
    public ICollection<PlanningSession> PlanningSessions { get; set; }
    public ICollection<ReasoningLog> ReasoningLogs { get; set; }
    public ICollection<ProgressEntry> ProgressEntries { get; set; }
    public ICollection<HandoffDocument> Handoffs { get; set; }
}

// Comment.cs
public class Comment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Content { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? SessionId { get; set; }       // Claude session if AI
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; }
}

// TicketHistory.cs
public class TicketHistory
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Field { get; set; }            // "Status", "Priority", etc.
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }       // "user" or session ID
    public DateTime ChangedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; }
}
```

### AI/Planning Entities

```csharp
// PlanningSession.cs
public class PlanningSession
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }           // Claude session identifier
    public string InitialUnderstanding { get; set; }
    public string? Assumptions { get; set; }         // JSON array
    public string? AlternativesConsidered { get; set; }  // JSON array
    public string? ChosenApproach { get; set; }
    public string? Rationale { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; }
}

// ReasoningLog.cs
public class ReasoningLog
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string DecisionPoint { get; set; }        // What decision was made
    public string? OptionsConsidered { get; set; }   // JSON array
    public string ChosenOption { get; set; }
    public string Rationale { get; set; }
    public int? ConfidencePercent { get; set; }      // 0-100
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; }
}

// ProgressEntry.cs
public class ProgressEntry
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Content { get; set; }              // What was done/attempted
    public ProgressOutcome Outcome { get; set; }
    public string? FilesAffected { get; set; }       // JSON array
    public string? ErrorDetails { get; set; }        // If failed
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; }
}

// HandoffDocument.cs
public class HandoffDocument
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? SessionId { get; set; }
    public string Title { get; set; }
    public HandoffType Type { get; set; }
    public string Summary { get; set; }              // Short, for lists
    public string Content { get; set; }              // Full markdown
    public string? StructuredContext { get; set; }   // JSON blob
    public bool IsActive { get; set; }               // False if superseded
    public Guid? SupersededById { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Ticket Ticket { get; set; }
    public HandoffDocument? SupersededBy { get; set; }
    public ICollection<HandoffVersion> Versions { get; set; }
    public ICollection<FileSnapshot> FileSnapshots { get; set; }
}

// HandoffVersion.cs (for history)
public class HandoffVersion
{
    public Guid Id { get; set; }
    public Guid HandoffId { get; set; }
    public int Version { get; set; }
    public string Content { get; set; }
    public string? StructuredContext { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public HandoffDocument Handoff { get; set; }
}

// FileSnapshot.cs
public class FileSnapshot
{
    public Guid Id { get; set; }
    public Guid HandoffId { get; set; }
    public string FilePath { get; set; }
    public string? ContentBefore { get; set; }       // Nullable for new files
    public string? ContentAfter { get; set; }        // Nullable for deleted files
    public string Language { get; set; }             // For syntax highlighting
    public DateTime CreatedAt { get; set; }

    // Navigation
    public HandoffDocument Handoff { get; set; }
}

// ApiKey.cs
public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; }                  // The actual GUID key
    public string Name { get; set; }                 // Friendly name
    public bool IsActive { get; set; }
    public int RateLimitPerMinute { get; set; }      // Max requests per minute (0 = unlimited)
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

// ApiKeyUsage.cs (for tracking rate limit)
public class ApiKeyUsage
{
    public Guid Id { get; set; }
    public Guid ApiKeyId { get; set; }
    public DateTime WindowStart { get; set; }        // Start of the current minute window
    public int RequestCount { get; set; }            // Requests in this window

    // Navigation
    public ApiKey ApiKey { get; set; }
}
```

### Structured Context JSON Schema

```json
// HandoffDocument.StructuredContext
{
  "assumptions": [
    "User wants JWT-based auth",
    "Refresh tokens should rotate on use"
  ],
  "decisionsMade": [
    {
      "decision": "Use 15-minute access token lifetime",
      "rationale": "Balance between security and UX"
    }
  ],
  "openQuestions": [
    "Should we add rate limiting to auth endpoints?"
  ],
  "blockers": [],
  "filesModified": [
    "src/Controllers/AuthController.cs",
    "src/Services/JwtService.cs"
  ],
  "testsAdded": [
    "tests/AuthControllerTests.cs"
  ],
  "nextSteps": [
    "Implement frontend login form",
    "Add refresh token rotation"
  ],
  "warnings": [
    "JWT secret in appsettings.json needs to be changed before deploy"
  ]
}
```

---

## API Endpoints

### Projects
```
GET    /api/projects                    - List all projects
POST   /api/projects                    - Create project
GET    /api/projects/{id}               - Get project by ID
GET    /api/projects/key/{key}          - Get project by key (e.g., "AIFORGE")
PUT    /api/projects/{id}               - Update project
DELETE /api/projects/{id}               - Delete project
```

### Tickets
```
GET    /api/tickets                     - List tickets (with filtering)
         ?projectId={guid}
         ?status={status}
         ?type={type}
         ?priority={priority}
         ?search={text}
POST   /api/tickets                     - Create ticket
GET    /api/tickets/{id}                - Get ticket with full details
GET    /api/tickets/key/{key}           - Get by key (e.g., "AIFORGE-123")
PUT    /api/tickets/{id}                - Update ticket
DELETE /api/tickets/{id}                - Delete ticket
POST   /api/tickets/{id}/transition     - Change status
         Body: { "status": "InProgress" }
GET    /api/tickets/{id}/history        - Get change history
```

### Comments
```
GET    /api/tickets/{ticketId}/comments     - List comments
POST   /api/tickets/{ticketId}/comments     - Add comment
PUT    /api/comments/{id}                   - Update comment
DELETE /api/comments/{id}                   - Delete comment
```

### Planning (Claude "Show Your Work")
```
POST   /api/planning/sessions               - Start planning session
         Body: { "ticketId", "initialUnderstanding", "assumptions" }
PUT    /api/planning/sessions/{id}          - Update session
POST   /api/planning/sessions/{id}/complete - Mark session complete

POST   /api/planning/reasoning              - Log a decision
         Body: { "ticketId", "decisionPoint", "optionsConsidered",
                 "chosenOption", "rationale", "confidencePercent" }

POST   /api/planning/progress               - Log progress entry
         Body: { "ticketId", "content", "outcome", "filesAffected" }

GET    /api/planning/tickets/{ticketId}     - Get all planning data for ticket
```

### Handoffs
```
GET    /api/handoffs                        - List handoffs (filterable)
         ?ticketId={guid}
         ?type={type}
         ?search={text}
POST   /api/handoffs                        - Create handoff
GET    /api/handoffs/{id}                   - Get full handoff
GET    /api/handoffs/ticket/{ticketId}/latest  - Get latest active handoff
PUT    /api/handoffs/{id}                   - Update handoff (creates version)

POST   /api/handoffs/{id}/snapshots         - Add file snapshot
         Body: { "filePath", "contentBefore", "contentAfter", "language" }
GET    /api/handoffs/{id}/snapshots         - Get file snapshots
```

### AI Context (Convenience endpoints for MCP)
```
GET    /api/ai/context/{ticketId}           - Get full context for Claude
         Returns: ticket + latest handoff + recent reasoning + progress

POST   /api/ai/session/start                - Register Claude session start
         Body: { "ticketId", "sessionId" }
POST   /api/ai/session/end                  - Register Claude session end
         Body: { "sessionId", "summary" }
```

### Search
```
GET    /api/search                          - Global search
         ?q={query}
         &type={tickets|handoffs|all}
         &projectId={guid}
```

---

## Authentication

### API Key Middleware (with Rate Limiting)

```csharp
// ApiKeyAuthMiddleware.cs
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-Api-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AiForgeDbContext dbContext)
    {
        // Skip auth for health check, swagger, etc.
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key required" });
            return;
        }

        var apiKey = await dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == apiKeyHeader.ToString() && k.IsActive);

        if (apiKey == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // Check rate limit (if configured)
        if (apiKey.RateLimitPerMinute > 0)
        {
            var now = DateTime.UtcNow;
            var windowStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);

            var usage = await dbContext.ApiKeyUsages
                .FirstOrDefaultAsync(u => u.ApiKeyId == apiKey.Id && u.WindowStart == windowStart);

            if (usage == null)
            {
                // First request in this window - create new usage record
                usage = new ApiKeyUsage
                {
                    Id = Guid.NewGuid(),
                    ApiKeyId = apiKey.Id,
                    WindowStart = windowStart,
                    RequestCount = 1
                };
                dbContext.ApiKeyUsages.Add(usage);

                // Clean up old usage records (older than 5 minutes)
                var cutoff = windowStart.AddMinutes(-5);
                var oldUsages = dbContext.ApiKeyUsages.Where(u => u.WindowStart < cutoff);
                dbContext.ApiKeyUsages.RemoveRange(oldUsages);
            }
            else if (usage.RequestCount >= apiKey.RateLimitPerMinute)
            {
                // Rate limit exceeded
                var secondsUntilReset = 60 - now.Second;
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = secondsUntilReset.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = apiKey.RateLimitPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = windowStart.AddMinutes(1).ToString("o");
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    retryAfterSeconds = secondsUntilReset,
                    limit = apiKey.RateLimitPerMinute
                });
                return;
            }
            else
            {
                // Increment counter
                usage.RequestCount++;
            }

            // Add rate limit headers to response
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit"] = apiKey.RateLimitPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] =
                    Math.Max(0, apiKey.RateLimitPerMinute - usage.RequestCount).ToString();
                context.Response.Headers["X-RateLimit-Reset"] = windowStart.AddMinutes(1).ToString("o");
                return Task.CompletedTask;
            });
        }

        // Update last used
        apiKey.LastUsedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await _next(context);
    }
}
```

---

## MCP Server Implementation

The MCP server will be a separate C# console app that communicates via stdio.

### Tools to Expose

```csharp
// Tool definitions for Claude
public static class McpTools
{
    // Project Tools
    [McpTool("list_projects", "List all projects")]
    [McpTool("get_project", "Get project details by key")]
    [McpTool("create_project", "Create a new project")]

    // Ticket Tools
    [McpTool("search_tickets", "Search tickets with filters")]
    [McpTool("get_ticket", "Get ticket details by key (e.g., AIFORGE-123)")]
    [McpTool("create_ticket", "Create a new ticket")]
    [McpTool("update_ticket", "Update ticket fields")]
    [McpTool("transition_ticket", "Change ticket status")]
    [McpTool("add_comment", "Add a comment to a ticket")]

    // Planning Tools (Show Your Work)
    [McpTool("start_planning", "Start a planning session for a ticket")]
    [McpTool("log_decision", "Log a decision with rationale")]
    [McpTool("log_progress", "Log progress on a task")]
    [McpTool("complete_planning", "Complete planning session")]

    // Handoff Tools
    [McpTool("get_context", "Get full context for a ticket (for resuming work)")]
    [McpTool("create_handoff", "Create a handoff document")]
    [McpTool("add_file_snapshot", "Add a file diff to handoff")]

    // Search
    [McpTool("search", "Search across tickets and handoffs")]
}
```

### Example Tool Definitions

```csharp
public class CreateTicketTool : IMcpTool
{
    public string Name => "create_ticket";
    public string Description => "Create a new ticket in a project";

    public JsonSchema InputSchema => new JsonSchemaBuilder()
        .Type(SchemaType.Object)
        .Properties(
            ("project_key", new JsonSchemaBuilder().Type(SchemaType.String).Description("Project key, e.g., AIFORGE")),
            ("title", new JsonSchemaBuilder().Type(SchemaType.String).Description("Ticket title")),
            ("description", new JsonSchemaBuilder().Type(SchemaType.String).Description("Detailed description")),
            ("type", new JsonSchemaBuilder().Type(SchemaType.String).Enum("Task", "Bug", "Feature", "Enhancement")),
            ("priority", new JsonSchemaBuilder().Type(SchemaType.String).Enum("Low", "Medium", "High", "Critical"))
        )
        .Required("project_key", "title", "type")
        .Build();

    public async Task<McpResult> ExecuteAsync(JsonElement input, CancellationToken ct)
    {
        // Implementation
    }
}

public class LogDecisionTool : IMcpTool
{
    public string Name => "log_decision";
    public string Description => "Log a decision with rationale for transparency";

    public JsonSchema InputSchema => new JsonSchemaBuilder()
        .Type(SchemaType.Object)
        .Properties(
            ("ticket_key", new JsonSchemaBuilder().Type(SchemaType.String)),
            ("decision", new JsonSchemaBuilder().Type(SchemaType.String).Description("What decision was made")),
            ("options_considered", new JsonSchemaBuilder().Type(SchemaType.Array).Items(new JsonSchemaBuilder().Type(SchemaType.String))),
            ("rationale", new JsonSchemaBuilder().Type(SchemaType.String).Description("Why this option was chosen")),
            ("confidence", new JsonSchemaBuilder().Type(SchemaType.Integer).Minimum(0).Maximum(100))
        )
        .Required("ticket_key", "decision", "rationale")
        .Build();
}

public class CreateHandoffTool : IMcpTool
{
    public string Name => "create_handoff";
    public string Description => "Create a handoff document summarizing work done";

    public JsonSchema InputSchema => new JsonSchemaBuilder()
        .Type(SchemaType.Object)
        .Properties(
            ("ticket_key", new JsonSchemaBuilder().Type(SchemaType.String)),
            ("type", new JsonSchemaBuilder().Type(SchemaType.String).Enum("SessionEnd", "Blocker", "Milestone", "ContextDump")),
            ("title", new JsonSchemaBuilder().Type(SchemaType.String)),
            ("summary", new JsonSchemaBuilder().Type(SchemaType.String).Description("Brief summary for lists")),
            ("content", new JsonSchemaBuilder().Type(SchemaType.String).Description("Full markdown content")),
            ("context", new JsonSchemaBuilder().Type(SchemaType.Object).Properties(
                ("assumptions", new JsonSchemaBuilder().Type(SchemaType.Array)),
                ("decisions_made", new JsonSchemaBuilder().Type(SchemaType.Array)),
                ("open_questions", new JsonSchemaBuilder().Type(SchemaType.Array)),
                ("blockers", new JsonSchemaBuilder().Type(SchemaType.Array)),
                ("files_modified", new JsonSchemaBuilder().Type(SchemaType.Array)),
                ("next_steps", new JsonSchemaBuilder().Type(SchemaType.Array)),
                ("warnings", new JsonSchemaBuilder().Type(SchemaType.Array))
            ))
        )
        .Required("ticket_key", "type", "title", "summary", "content")
        .Build();
}
```

---

## Frontend Pages & Components

### Pages

1. **Dashboard** (`/`)
   - Overview stats (tickets by status, recent activity)
   - Quick access to recent tickets
   - Active Claude sessions indicator

2. **Projects** (`/projects`)
   - List of all projects
   - Create new project button

3. **Project Detail** (`/projects/:key`)
   - Kanban board view (To Do | In Progress | In Review | Done)
   - List view toggle
   - Filter by type, priority
   - Create ticket button

4. **Ticket Detail** (`/tickets/:key`)
   - Full ticket info with tabs:
     - **Details** - Description, metadata, comments
     - **AI Context** - Planning sessions, reasoning logs, progress
     - **Handoffs** - List of handoff documents
     - **History** - Change log

5. **Handoff Viewer** (`/handoffs/:id`)
   - Full handoff document with:
     - Markdown content rendering
     - Structured context sidebar
     - File diffs with syntax highlighting

6. **Settings** (`/settings`)
   - API key management (view, regenerate)

### Key Components

```
components/
├── layout/
│   ├── AppLayout.tsx          # Main layout with sidebar
│   ├── Sidebar.tsx            # Navigation
│   └── Header.tsx             # Top bar with search
│
├── tickets/
│   ├── TicketBoard.tsx        # Kanban board
│   ├── TicketCard.tsx         # Card in board/list
│   ├── TicketDetail.tsx       # Full ticket view
│   ├── TicketForm.tsx         # Create/edit form
│   ├── StatusChip.tsx         # Colored status badge
│   └── PriorityChip.tsx       # Priority indicator
│
├── planning/
│   ├── PlanningTimeline.tsx   # Timeline of AI activity
│   ├── PlanningSessionCard.tsx
│   ├── ReasoningLogCard.tsx   # Decision with rationale
│   └── ProgressEntryCard.tsx  # Progress log item
│
├── handoffs/
│   ├── HandoffList.tsx        # List of handoffs
│   ├── HandoffViewer.tsx      # Full document view
│   ├── StructuredContext.tsx  # JSON context display
│   ├── CodeSnippet.tsx        # Syntax highlighted code
│   └── FileDiff.tsx           # Side-by-side or unified diff
│
└── common/
    ├── SearchBar.tsx
    ├── ConfirmDialog.tsx
    ├── LoadingSpinner.tsx
    └── EmptyState.tsx
```

---

## Implementation Phases

### Phase 1: Foundation (Backend Core)
1. Create solution structure and projects
2. Implement domain entities and enums
3. Set up EF Core with DbContext and configurations
4. Create and run initial migration
5. Implement repositories and unit of work
6. Set up API project with middleware
7. Implement API key authentication
8. Create Projects CRUD endpoints
9. Create Tickets CRUD endpoints with filtering
10. Add comments and history tracking

### Phase 2: AI Features (Backend)
1. Implement PlanningSession endpoints
2. Implement ReasoningLog endpoints
3. Implement ProgressEntry endpoints
4. Implement HandoffDocument endpoints
5. Add FileSnapshot support
6. Create AI context aggregation endpoint
7. Add search functionality

### Phase 3: MCP Server
1. Create MCP server project structure
2. Implement stdio communication
3. Create tool definitions
4. Implement ticket tools
5. Implement planning tools
6. Implement handoff tools
7. Test with Claude

### Phase 4: Frontend Foundation
1. Create Vite + React + TypeScript project
2. Set up MUI theme
3. Configure Zustand stores
4. Create API client with axios
5. Implement AppLayout and navigation
6. Create Dashboard page
7. Create Projects list and detail pages

### Phase 5: Frontend Tickets
1. Implement TicketBoard (Kanban)
2. Implement TicketCard component
3. Implement TicketDetail page
4. Implement TicketForm (create/edit)
5. Add filtering and search
6. Add drag-and-drop status changes

### Phase 6: Frontend AI Features
1. Implement PlanningTimeline component
2. Implement ReasoningLogCard
3. Implement ProgressEntryCard
4. Create AI Context tab in ticket detail
5. Implement HandoffList page
6. Implement HandoffViewer with markdown
7. Implement CodeSnippet with syntax highlighting
8. Implement FileDiff component

### Phase 7: Polish & Testing
1. Add loading states and error handling
2. Add empty states
3. Implement Settings page
4. Write API integration tests
5. Manual end-to-end testing
6. Documentation

---

## Database Seeding

Initial seed data for development:

```csharp
public static class DbSeeder
{
    public static async Task SeedAsync(AiForgeDbContext context)
    {
        // Create default API key
        if (!await context.ApiKeys.AnyAsync())
        {
            context.ApiKeys.Add(new ApiKey
            {
                Id = Guid.NewGuid(),
                Key = Guid.NewGuid().ToString(),  // Or a fixed key for dev
                Name = "Default Key",
                IsActive = true,
                RateLimitPerMinute = 60,          // 60 requests per minute (0 = unlimited)
                CreatedAt = DateTime.UtcNow
            });
        }

        // Create sample project
        if (!await context.Projects.AnyAsync())
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Key = "DEMO",
                Name = "Demo Project",
                Description = "A sample project for testing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                NextTicketNumber = 1
            };
            context.Projects.Add(project);
        }

        await context.SaveChangesAsync();
    }
}
```

---

## Configuration

### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AiForge;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

### Frontend .env
```
VITE_API_URL=https://localhost:7xxx
VITE_API_KEY=your-api-key-here
```

---

## Commands Reference

### Backend
```bash
# Create solution
dotnet new sln -n AiForge

# Create projects
dotnet new classlib -n AiForge.Domain -o src/AiForge.Domain
dotnet new classlib -n AiForge.Infrastructure -o src/AiForge.Infrastructure
dotnet new classlib -n AiForge.Application -o src/AiForge.Application
dotnet new webapi -n AiForge.Api -o src/AiForge.Api
dotnet new console -n AiForge.Mcp -o src/AiForge.Mcp

# Add to solution
dotnet sln add src/AiForge.Domain
dotnet sln add src/AiForge.Infrastructure
dotnet sln add src/AiForge.Application
dotnet sln add src/AiForge.Api
dotnet sln add src/AiForge.Mcp

# Add project references
dotnet add src/AiForge.Infrastructure reference src/AiForge.Domain
dotnet add src/AiForge.Application reference src/AiForge.Domain
dotnet add src/AiForge.Api reference src/AiForge.Application src/AiForge.Infrastructure
dotnet add src/AiForge.Mcp reference src/AiForge.Application

# Add packages (Infrastructure)
dotnet add src/AiForge.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/AiForge.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/AiForge.Infrastructure package Microsoft.EntityFrameworkCore.Tools

# Add packages (Application)
dotnet add src/AiForge.Application package AutoMapper
dotnet add src/AiForge.Application package FluentValidation

# Add packages (Api)
dotnet add src/AiForge.Api package Swashbuckle.AspNetCore

# Run migrations
dotnet ef migrations add InitialCreate -p src/AiForge.Infrastructure -s src/AiForge.Api
dotnet ef database update -p src/AiForge.Infrastructure -s src/AiForge.Api
```

### Frontend
```bash
# Create Vite project
cd frontend
npm create vite@latest aiforge-ui -- --template react-ts
cd aiforge-ui

# Install dependencies
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled
npm install zustand
npm install axios
npm install react-router-dom
npm install @types/react-router-dom -D
npm install react-markdown
npm install react-syntax-highlighter @types/react-syntax-highlighter
npm install diff              # For file diffs
```

---

This plan provides a complete roadmap for building AiForge. Ready to start implementation?
