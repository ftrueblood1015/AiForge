# AiForge Ticket Status

Get comprehensive status for a ticket including all associated data.

## Ticket: $ARGUMENTS

## Session Analytics (LIGHTWEIGHT)

This is a read-only status check, so analytics tracking is minimal:
- No session metrics needed (read-only operation)
- If you make any updates or decisions while reviewing status, log them normally

## Instructions

Gather and display all information about this ticket:

1. **Use `get_context`** to retrieve:
   - Ticket details (title, description, status, type, priority)
   - Latest handoff summary
   - Recent reasoning/decisions
   - Recent progress entries
   - Active planning session

2. **Use `get_implementation_plan`** to check:
   - Current plan status (Draft/Approved/None)
   - Plan version and content summary

3. **Use `get_estimation_data`** to show:
   - Current estimation (complexity, effort, confidence)
   - Actual effort if completed

4. **Use `get_ticket_analytics`** to display:
   - Session count and total duration
   - Token usage
   - Decision count

## Output Format

```
# [TICKET-KEY]: [Title]

**Status:** [Status] | **Type:** [Type] | **Priority:** [Priority]

## Description
[First 500 chars of description...]

## Implementation Plan
**Status:** [Draft/Approved/None]
**Version:** [N]
[Summary of plan if exists]

## Estimation
| Complexity | Effort | Confidence | Actual |
|------------|--------|------------|--------|
| [Level]    | [Size] | [%]        | [Size] |

## Work Summary
- **Sessions:** [N] totaling [X] minutes
- **Tokens Used:** [N]
- **Decisions Logged:** [N]
- **Progress Entries:** [N]
- **Handoffs Created:** [N]

## Recent Activity
[Last 3 progress entries with timestamps]

## Current Handoff Summary
[Latest handoff summary if exists]

## Next Steps
[From handoff or planning session]
```

Display this information in a clear, scannable format.
