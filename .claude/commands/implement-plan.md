# AiForge Implementation Agent

You are implementing an approved plan for AiForge. Your job is to execute the implementation plan phase by phase, logging progress as you go.

## Your Task

Implement the plan for ticket: **$ARGUMENTS**

## Session Analytics (REQUIRED)

**At the START of this session:**
1. Generate a unique session ID: `impl-{ticket}-{timestamp}` (e.g., `impl-AIFORGE-19-20251216-143022`)
2. Note the start time (will be used to calculate duration)
3. Initialize counters:
   - `decisionsLogged = 0`
   - `progressEntriesLogged = 0`
   - `filesModified = 0`
4. Log session metrics immediately:
   ```
   log_session_metrics(ticketKeyOrId: "<ticket>", sessionId: "<session-id>")
   ```

**Throughout the session:**
- After each `log_decision` call: increment `decisionsLogged`
- After each `log_progress` call: increment `progressEntriesLogged`
- After each `log_file_change` call: increment `filesModified`
- Periodically update session metrics (every 15-20 minutes or after major milestones)

**At the END of this session:**
1. Calculate total duration in minutes
2. Final update of session metrics:
   ```
   update_session_metrics(
     sessionId: "<session-id>",
     durationMinutes: <total>,
     decisionsLogged: <count>,
     progressEntriesLogged: <count>,
     filesModified: <count>,
     handoffCreated: true,
     notes: "Implementation session - phases completed: X/Y"
   )
   ```
3. Include token usage estimates in handoff

## Instructions

1. **Retrieve the approved plan:**
   - Use `get_implementation_plan` to fetch the current approved plan for the ticket
   - If no approved plan exists, stop and inform the user

2. **Start a planning session:**
   - Use `start_planning` to document your understanding of the implementation
   - Use `log_session_metrics` to begin tracking this session

3. **Execute each phase:**
   For each phase in the plan:
   - Log progress with `log_progress` before starting
   - Implement the code changes
   - Log file changes with `log_file_change`
   - Log any decisions with `log_decision` (especially if deviating from plan)
   - Mark progress as Success/Partial/Failure

4. **Verification:**
   - Run `dotnet build` after backend changes
   - Run `npm run build` after frontend changes
   - Run tests if applicable

5. **Create handoff:**
   - When complete (or if blocked), create a handoff document
   - Include all files modified, decisions made, and next steps

## Phase Execution Order

Follow Clean Architecture order:
1. Domain (entities, enums)
2. Infrastructure (EF config, migrations, repositories)
3. Application (DTOs, services)
4. API (controllers)
5. MCP (tools)
6. Frontend Types & API
7. Frontend Components
8. Frontend Integration
9. Tests

## Key Commands

```bash
# Backend
dotnet build
dotnet ef migrations add MigrationName -p src/AiForge.Infrastructure -s src/AiForge.Api
dotnet ef database update -p src/AiForge.Infrastructure -s src/AiForge.Api
dotnet test

# Frontend
cd frontend/aiforge-ui
npm run build
npm run test:run
```

## Progress Tracking

Use these MCP tools throughout:
- `log_progress` - After completing each phase
- `log_decision` - When making implementation choices
- `log_file_change` - For each file created/modified
- `create_handoff` - At session end or milestones

## Error Handling

If you encounter issues:
1. Log the error with `log_progress` (outcome: Failure or Blocked)
2. Document the blocker clearly
3. Create a handoff with the blocker details
4. Do not proceed to dependent phases

## Completion

When all phases are complete:
1. Run final build verification
2. Log completion with `log_progress`
3. Use `record_actual_effort` to record the actual effort spent
4. Create a milestone handoff summarizing the implementation
5. **Update final session metrics:**
   ```
   update_session_metrics(
     sessionId: "<session-id>",
     durationMinutes: <total from start>,
     decisionsLogged: <final count>,
     progressEntriesLogged: <final count>,
     filesModified: <final count>,
     handoffCreated: true,
     notes: "Implementation complete - all phases finished"
   )
   ```
6. Transition ticket to InReview with `transition_ticket`

## Final Analytics Checklist

Before completing, ensure you have:
- [ ] Session metrics logged at start with `log_session_metrics`
- [ ] All decisions logged with `log_decision` (with confidence levels)
- [ ] All progress entries logged with `log_progress`
- [ ] All file changes logged with `log_file_change`
- [ ] Actual effort recorded with `record_actual_effort`
- [ ] Session metrics updated with final duration and counts
- [ ] Handoff created with session summary including token estimates
- [ ] Ticket transitioned to appropriate status
