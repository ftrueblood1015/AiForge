# AiForge Planning Agent Commands

This directory contains slash commands for planning and implementing features in AiForge.

## Command Overview

| Command | Purpose | Analytics |
|---------|---------|-----------|
| `/plan-feature <description>` | Create detailed 9-phase implementation plan | Full |
| `/review-plan <ticket>` | Review and approve/reject a draft plan | Standard |
| `/implement-plan <ticket>` | Execute an approved plan phase-by-phase | Full |
| `/quick-plan <description>` | Lightweight planning for small tasks | Lightweight |
| `/ticket-status <ticket>` | View comprehensive ticket status | None |
| `/architecture` | Reference guide for project structure | None |

---

## Feature Development Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           FEATURE DEVELOPMENT WORKFLOW                       │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│  New Feature     │
│  Request         │
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  /plan-feature <description>                                                 │
│  ─────────────────────────────────────────────────────────────────────────── │
│  • Creates ticket (if needed)                                                │
│  • Logs session metrics                                                      │
│  • Analyzes requirements                                                     │
│  • Creates 9-phase implementation plan (Draft status)                        │
│  • Logs decisions with confidence levels                                     │
│  • Creates effort estimation                                                 │
│  • Updates session metrics with duration/tokens                              │
│                                                                              │
│  Output: AIFORGE-XX ticket with Draft implementation plan                    │
└────────┬─────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  /review-plan AIFORGE-XX                                                     │
│  ─────────────────────────────────────────────────────────────────────────── │
│  • Logs session metrics                                                      │
│  • Fetches draft plan                                                        │
│  • Evaluates against criteria:                                               │
│    - Architecture compliance                                                 │
│    - Data model quality                                                      │
│    - API design                                                              │
│    - Frontend patterns                                                       │
│  • Logs review decision                                                      │
│  • Approves or rejects with feedback                                         │
│  • Updates session metrics                                                   │
│                                                                              │
│  Output: Plan status → Approved or Rejected                                  │
└────────┬─────────────────────────────────────────────────────────────────────┘
         │
         ├─────── If REJECTED ──────┐
         │                          │
         │                          ▼
         │               ┌─────────────────────┐
         │               │ Revise plan based   │
         │               │ on feedback         │
         │               │ (loop back to       │
         │               │  /review-plan)      │
         │               └─────────────────────┘
         │
         ▼ If APPROVED
┌──────────────────────────────────────────────────────────────────────────────┐
│  /implement-plan AIFORGE-XX                                                  │
│  ─────────────────────────────────────────────────────────────────────────── │
│  • Logs session metrics at start                                             │
│  • Fetches approved plan                                                     │
│  • Executes phases in order:                                                 │
│                                                                              │
│    Phase 1: Domain ────────→ Entities, enums                                 │
│    Phase 2: Infrastructure ─→ EF config, migrations, repos                   │
│    Phase 3: Application ───→ DTOs, services                                  │
│    Phase 4: API ───────────→ Controllers                                     │
│    Phase 5: MCP ───────────→ Claude tools                                    │
│    Phase 6: Frontend Types ─→ TypeScript interfaces, API client              │
│    Phase 7: Frontend UI ───→ Components                                      │
│    Phase 8: Integration ───→ Page integration, routes                        │
│    Phase 9: Tests ─────────→ Unit/integration tests                          │
│                                                                              │
│  • Logs progress after each phase                                            │
│  • Logs all file changes                                                     │
│  • Logs decisions when deviating from plan                                   │
│  • Records actual effort                                                     │
│  • Creates milestone handoff                                                 │
│  • Updates session metrics with final counts                                 │
│  • Transitions ticket to InReview                                            │
│                                                                              │
│  Output: Implemented feature, ticket → InReview                              │
└────────┬─────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────┐
│  Feature Done    │
│  Ready for QA    │
└──────────────────┘
```

---

## Utility Commands

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  /ticket-status AIFORGE-XX                                                   │
│  ─────────────────────────────────────────────────────────────────────────── │
│  • Shows comprehensive ticket info:                                          │
│    - Status, type, priority                                                  │
│    - Implementation plan status                                              │
│    - Estimation data                                                         │
│    - Session analytics (time, tokens, decisions)                             │
│    - Recent activity                                                         │
│    - Current handoff summary                                                 │
│                                                                              │
│  Use anytime to check progress                                               │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│  /quick-plan <small task description>                                        │
│  ─────────────────────────────────────────────────────────────────────────── │
│  • For bug fixes and small changes                                           │
│  • Lightweight planning (no 9-phase structure)                               │
│  • Creates ticket + simple plan                                              │
│  • Logs session metrics at end                                               │
│                                                                              │
│  Use for: XSmall/Small tasks that don't need full planning                   │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│  /architecture                                                               │
│  ─────────────────────────────────────────────────────────────────────────── │
│  • Reference guide showing:                                                  │
│    - Project structure                                                       │
│    - All entities and enums                                                  │
│    - API endpoints                                                           │
│    - MCP tools                                                               │
│    - Frontend structure                                                      │
│    - Naming conventions                                                      │
│                                                                              │
│  Use for: Quick lookup during development                                    │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Analytics Data Flow

Every planning/implementation session tracks metrics for visibility and reporting.

```
    ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
    │   START     │     │   DURING    │     │    END      │
    └──────┬──────┘     └──────┬──────┘     └──────┬──────┘
           │                   │                   │
           ▼                   ▼                   ▼
    ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
    │log_session_ │     │log_decision │     │update_      │
    │metrics()    │     │log_progress │     │session_     │
    │             │     │log_file_    │     │metrics()    │
    │ • ticketId  │     │change       │     │             │
    │ • sessionId │     │             │     │ • duration  │
    │ • startTime │     │ (increment  │     │ • counts    │
    │             │     │  counters)  │     │ • notes     │
    └─────────────┘     └─────────────┘     └─────────────┘
                                                   │
                                                   ▼
                                           ┌─────────────┐
                                           │ Viewable in │
                                           │ Analytics   │
                                           │ Dashboard   │
                                           └─────────────┘
```

### Session ID Formats

| Command Type | Format | Example |
|--------------|--------|---------|
| Planning | `plan-{timestamp}` | `plan-20251216-143022` |
| Implementation | `impl-{ticket}-{timestamp}` | `impl-AIFORGE-19-20251216-143022` |
| Review | `review-{ticket}-{timestamp}` | `review-AIFORGE-19-20251216-143022` |
| Quick Plan | `quick-{timestamp}` | `quick-20251216-143022` |

### What Gets Tracked

| Metric | Description |
|--------|-------------|
| `durationMinutes` | Total session time |
| `decisionsLogged` | Count of `log_decision` calls |
| `progressEntriesLogged` | Count of `log_progress` calls |
| `filesModified` | Count of `log_file_change` calls |
| `handoffCreated` | Whether a handoff was created |
| `inputTokens` | Estimated input tokens (if available) |
| `outputTokens` | Estimated output tokens (if available) |
| `notes` | Session summary |

---

## Quick Reference

| When you want to... | Use this command |
|---------------------|------------------|
| Plan a new feature | `/plan-feature Add user notifications` |
| Review a draft plan | `/review-plan AIFORGE-19` |
| Implement an approved plan | `/implement-plan AIFORGE-19` |
| Check ticket status | `/ticket-status AIFORGE-19` |
| Plan a small bug fix | `/quick-plan Fix null reference in login` |
| Look up architecture | `/architecture` |

---

## 9-Phase Implementation Structure

When `/plan-feature` creates a plan, it follows this structure:

### Backend Phases
1. **Phase 1: Domain Layer** - Entities, enums in `AiForge.Domain`
2. **Phase 2: Infrastructure Layer** - EF config, migrations, repositories
3. **Phase 3: Application Layer** - DTOs, services, validators
4. **Phase 4: API Layer** - Controllers, endpoints
5. **Phase 5: MCP Tools** - Claude integration tools

### Frontend Phases
6. **Phase 6: Types & API** - TypeScript interfaces, API client
7. **Phase 7: Components** - React components with MUI
8. **Phase 8: Integration** - Page integration, routing

### Quality Phase
9. **Phase 9: Tests** - Unit tests, integration tests

---

## Review Criteria

When `/review-plan` evaluates a plan, it checks:

### Architecture Compliance
- Follows Clean Architecture (Domain → Infrastructure → Application → API)
- No circular dependencies
- Proper layer separation

### Data Model Quality
- Appropriate property types
- Correct relationships
- Proper indexes

### API Design
- RESTful conventions
- Correct HTTP methods and status codes
- DTOs separate from domain

### Frontend Patterns
- Component scoping
- Type matching
- Loading/error state handling

### Consistency
- Naming conventions match existing code
- Similar to previous implementations (AIFORGE-12, 13, 14, 15)

---

## Files in This Directory

| File | Description |
|------|-------------|
| `README.md` | This documentation |
| `plan-feature.md` | Full feature planning agent |
| `implement-plan.md` | Implementation execution agent |
| `review-plan.md` | Plan review and approval agent |
| `quick-plan.md` | Lightweight planning for small tasks |
| `ticket-status.md` | Ticket status dashboard |
| `architecture.md` | Project architecture reference |
