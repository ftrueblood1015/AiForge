# AiForge Quick Plan

Create a lightweight implementation plan for a small feature or bug fix.

## Task: $ARGUMENTS

## Session Analytics (REQUIRED)

**At the START:**
1. Generate session ID: `quick-{timestamp}` (e.g., `quick-20251216-143022`)
2. Note start time

**At the END:**
```
log_session_metrics(
  ticketKeyOrId: "<ticket>",
  sessionId: "<session-id>",
  durationMinutes: <calculated>,
  decisionsLogged: <count>,
  notes: "Quick plan session"
)
```

For quick plans, you can log metrics in a single call at the end since these are short sessions.

## When to Use Quick Plan

Use this for:
- Bug fixes
- Small enhancements (single component changes)
- Configuration changes
- Documentation updates
- Refactoring within a single layer

For larger features spanning multiple layers, use `/plan-feature` instead.

## Quick Plan Format

### 1. Problem Statement
What issue are we solving or what small feature are we adding?

### 2. Root Cause / Approach
- For bugs: What's causing the issue?
- For features: What's the simplest solution?

### 3. Changes Required

**Files to Modify:**
| File | Change |
|------|--------|
| path/to/file | Description of change |

**Files to Create (if any):**
| File | Purpose |
|------|---------|
| path/to/new/file | Why it's needed |

### 4. Verification
- How to test the change
- Commands to run

### 5. Estimate
- **Complexity:** Low/Medium
- **Effort:** XSmall/Small
- **Confidence:** [0-100]%

## Actions

After creating the quick plan:

1. **Create ticket (if needed):**
   ```
   create_ticket with type Bug/Enhancement
   ```

2. **Log session metrics:**
   ```
   log_session_metrics(ticketKeyOrId, sessionId, durationMinutes, notes)
   ```

3. **Create implementation plan:**
   ```
   create_implementation_plan with the plan content
   ```

4. **Estimate:**
   ```
   estimate_ticket with complexity, effort, confidence
   ```

5. **Log any decisions made:**
   ```
   log_decision with decisionPoint, chosenOption, rationale, confidence
   ```

6. **For trivial changes:** Consider auto-approving and implementing immediately

## Final Analytics Checklist

- [ ] Session metrics logged with duration
- [ ] Implementation plan created
- [ ] Estimation recorded
- [ ] Key decisions logged (if any)

## Example Output

```markdown
# Quick Plan: Fix dark mode contrast in StatusChip

## Problem
StatusChip uses hardcoded light grey background that's unreadable in dark mode.

## Approach
Use MUI theme-aware colors instead of hardcoded values.

## Changes
| File | Change |
|------|--------|
| src/components/tickets/StatusChip.tsx | Replace `grey.100` with `theme.palette.action.hover` |

## Verification
1. Run `npm run build`
2. Toggle dark mode in Settings
3. View any ticket with status chips

## Estimate
- Complexity: Low
- Effort: XSmall
- Confidence: 95%
```
