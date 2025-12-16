# AiForge Architecture Reference

Display the current architecture and patterns used in AiForge.

**Note:** This is a reference command (read-only). No session analytics tracking required.

## Project Structure

```
AiForge/
├── src/
│   ├── AiForge.Domain/           # Entities, enums (no dependencies)
│   ├── AiForge.Infrastructure/   # EF Core, DbContext, repositories
│   ├── AiForge.Application/      # Services, DTOs, validators
│   ├── AiForge.Api/              # Controllers, middleware, Program.cs
│   └── AiForge.Mcp/              # MCP Server for Claude integration
├── frontend/
│   └── aiforge-ui/               # React + Vite + TypeScript + MUI
├── tests/
│   ├── AiForge.Api.Tests/
│   └── AiForge.Application.Tests/
└── AiForge.sln
```

## Domain Entities

### Core Entities
| Entity | Purpose | Key Fields |
|--------|---------|------------|
| Project | Container for tickets | Key, Name, Description |
| Ticket | Work item | Key, Title, Status, Type, Priority |
| Comment | Notes on tickets | Content, IsAiGenerated |
| TicketHistory | Audit trail | FieldChanged, OldValue, NewValue |

### AI/Planning Entities
| Entity | Purpose | Key Fields |
|--------|---------|------------|
| PlanningSession | Initial understanding | InitialUnderstanding, Assumptions |
| ReasoningLog | Decisions made | DecisionPoint, ChosenOption, Confidence |
| ProgressEntry | Work done/attempted | Content, Outcome, FilesAffected |
| HandoffDocument | Context preservation | Title, Summary, Content, StructuredContext |
| FileSnapshot | Code before/after | FilePath, ContentBefore, ContentAfter |
| ImplementationPlan | Detailed plan | Content, Status, Version, AffectedFiles |

### Code Intelligence Entities
| Entity | Purpose | Key Fields |
|--------|---------|------------|
| FileChange | File modifications | FilePath, ChangeType, LinesAdded/Removed |
| TestLink | Test associations | TestFilePath, TestName, Outcome |
| TechnicalDebt | Debt tracking | Title, Category, Severity, Status |

### Analytics Entities
| Entity | Purpose | Key Fields |
|--------|---------|------------|
| SessionMetrics | Session tracking | SessionId, InputTokens, Duration |
| EffortEstimation | Effort tracking | Complexity, EstimatedEffort, ActualEffort |

## Enums

```csharp
// Ticket workflow
TicketStatus: ToDo, InProgress, InReview, Done
TicketType: Task, Bug, Feature, Enhancement
Priority: Low, Medium, High, Critical

// Implementation plans
PlanStatus: Draft, Approved, Superseded, Rejected

// Estimation
ComplexityLevel: Low, Medium, High, VeryHigh
EffortSize: XSmall, Small, Medium, Large, XLarge

// Code intelligence
FileChangeType: Created, Modified, Deleted, Renamed
TestOutcome: Passed, Failed, Skipped, NotRun
DebtCategory: Performance, Security, Maintainability, Testing, Documentation, Architecture
DebtSeverity: Low, Medium, High, Critical
DebtStatus: Open, InProgress, Resolved, Accepted

// Progress tracking
ProgressOutcome: Success, Failure, Partial, Blocked
HandoffType: SessionEnd, Blocker, Milestone, ContextDump
```

## API Endpoints

### Projects
- `GET /api/projects` - List all
- `GET /api/projects/{id}` - Get by ID
- `GET /api/projects/key/{key}` - Get by key
- `POST /api/projects` - Create
- `PUT /api/projects/{id}` - Update
- `DELETE /api/projects/{id}` - Delete

### Tickets
- `GET /api/tickets` - List with filters
- `GET /api/tickets/{id}` - Get by ID
- `POST /api/tickets` - Create
- `PUT /api/tickets/{id}` - Update
- `DELETE /api/tickets/{id}` - Delete
- `POST /api/tickets/{id}/transition` - Change status
- `GET /api/tickets/{id}/history` - Get history
- `GET /api/tickets/{id}/comments` - Get comments
- `POST /api/tickets/{id}/comments` - Add comment

### Planning
- `GET /api/planning/data?ticketId={id}` - Get all planning data
- `POST /api/planning/sessions` - Start session
- `POST /api/planning/reasoning` - Log decision
- `POST /api/planning/progress` - Log progress

### Handoffs
- `GET /api/handoffs` - List all
- `GET /api/handoffs/{id}` - Get by ID
- `GET /api/handoffs/ticket/{ticketId}` - Get by ticket
- `POST /api/handoffs` - Create
- `POST /api/handoffs/{id}/snapshots` - Add file snapshot

### Implementation Plans
- `GET /api/tickets/{id}/plans` - List plans
- `GET /api/tickets/{id}/plans/current` - Get current
- `POST /api/tickets/{id}/plans` - Create
- `PUT /api/plans/{id}` - Update
- `POST /api/plans/{id}/approve` - Approve
- `POST /api/plans/{id}/reject` - Reject
- `POST /api/plans/{id}/supersede` - Create new version

### Analytics
- `GET /api/analytics/dashboard` - Overview
- `GET /api/analytics/confidence/low` - Low confidence decisions
- `GET /api/analytics/patterns/hot-files` - Frequently modified files
- `GET /api/analytics/sessions/ticket/{id}` - Ticket session stats

## MCP Tools (40+)

### Ticket Management
`create_ticket`, `get_ticket`, `update_ticket`, `transition_ticket`, `search_tickets`, `add_comment`

### Planning
`start_planning`, `complete_planning`, `log_decision`, `log_progress`, `get_context`, `get_planning_data`

### Handoffs
`create_handoff`, `get_handoff`, `list_handoffs`, `add_file_snapshot`

### Implementation Plans
`create_implementation_plan`, `update_implementation_plan`, `get_implementation_plan`, `approve_implementation_plan`, `reject_implementation_plan`, `supersede_implementation_plan`

### Code Intelligence
`log_file_change`, `query_file_history`, `get_hot_files`, `link_test`, `update_test_outcome`, `flag_technical_debt`, `get_debt_backlog`, `resolve_debt`

### Analytics
`log_session_metrics`, `update_session_metrics`, `get_analytics_summary`, `get_confidence_report`, `get_pattern_insights`, `get_ticket_analytics`

### Estimation
`estimate_ticket`, `record_actual_effort`, `get_estimation_data`

## Frontend Structure

```
frontend/aiforge-ui/src/
├── api/                    # Axios API clients
│   ├── client.ts          # Base axios instance
│   ├── projects.ts        # Project API
│   ├── tickets.ts         # Ticket API
│   ├── planning.ts        # Planning API
│   ├── handoffs.ts        # Handoff API
│   ├── plans.ts           # Implementation Plan API
│   ├── estimation.ts      # Estimation API
│   ├── codeIntelligence.ts # Code Intel API
│   └── analytics.ts       # Analytics API
├── components/
│   ├── layout/            # AppLayout, Header, Sidebar
│   ├── tickets/           # TicketCard, TicketBoard, TicketForm
│   ├── planning/          # PlanningTimeline, ReasoningLogCard
│   ├── handoffs/          # HandoffViewer, CodeSnippet, FileDiff
│   ├── plans/             # ImplementationPlanView
│   ├── estimation/        # EstimationCard, EstimationSection
│   ├── codeIntelligence/  # CodeIntelligenceTab
│   ├── analytics/         # TicketAnalyticsTab
│   ├── history/           # TicketHistoryTimeline
│   └── common/            # ErrorBoundary, LoadingState
├── pages/
│   ├── Dashboard.tsx
│   ├── ProjectList.tsx, ProjectDetail.tsx
│   ├── TicketList.tsx, TicketDetail.tsx
│   ├── HandoffList.tsx, HandoffDetail.tsx
│   ├── TechnicalDebtDashboard.tsx
│   ├── AnalyticsDashboard.tsx
│   └── Settings.tsx
├── stores/                # Zustand stores
├── hooks/                 # Custom hooks
├── types/                 # TypeScript interfaces
└── theme.ts              # MUI theme config
```

## Common Patterns

### Adding a New Feature

1. **Domain:** Create entity in `AiForge.Domain/Entities/`
2. **Infrastructure:** Add EF config, migration, repository
3. **Application:** Create DTOs, service interface and implementation
4. **API:** Create controller with REST endpoints
5. **MCP:** Add tools for Claude integration
6. **Frontend:** Add types, API client, components, integrate into pages

### Naming Conventions

- **Entities:** PascalCase singular (e.g., `ImplementationPlan`)
- **Tables:** PascalCase plural (e.g., `ImplementationPlans`)
- **DTOs:** `[Entity]Dto`, `Create[Entity]Request`, `Update[Entity]Request`
- **Services:** `I[Entity]Service`, `[Entity]Service`
- **Controllers:** `[Entity]sController` (plural)
- **MCP Tools:** snake_case (e.g., `create_implementation_plan`)
- **Frontend files:** PascalCase for components, camelCase for utilities
