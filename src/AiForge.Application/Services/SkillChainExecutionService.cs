using System.Text.Json;
using AiForge.Application.DTOs.SessionState;
using AiForge.Application.DTOs.SkillChains;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Application.Services;

public interface ISkillChainExecutionService
{
    // Execution lifecycle
    Task<SkillChainExecutionDto> StartExecutionAsync(StartChainExecutionRequest request, CancellationToken ct = default);
    Task<SkillChainExecutionDto?> GetExecutionAsync(Guid executionId, CancellationToken ct = default);
    Task<IEnumerable<SkillChainExecutionSummaryDto>> GetExecutionsAsync(Guid? chainId, Guid? ticketId, string? status, CancellationToken ct = default);
    Task<SkillChainExecutionDto?> PauseExecutionAsync(Guid executionId, string reason, string pausedBy, CancellationToken ct = default);
    Task<SkillChainExecutionDto?> ResumeExecutionAsync(Guid executionId, ResumeExecutionRequest request, CancellationToken ct = default);
    Task<SkillChainExecutionDto?> CancelExecutionAsync(Guid executionId, string reason, string cancelledBy, CancellationToken ct = default);

    // Link execution
    Task<SkillChainLinkExecutionDto> RecordLinkOutcomeAsync(Guid executionId, RecordLinkOutcomeRequest request, CancellationToken ct = default);
    Task<SkillChainExecutionDto?> AdvanceExecutionAsync(Guid executionId, CancellationToken ct = default);

    // Human intervention
    Task<IEnumerable<SkillChainExecutionSummaryDto>> GetPendingInterventionsAsync(Guid? projectId, CancellationToken ct = default);
    Task<SkillChainExecutionDto?> ResolveInterventionAsync(Guid executionId, ResolveInterventionRequest request, CancellationToken ct = default);

    // Checkpoints
    Task<ExecutionCheckpointDto?> GetLatestCheckpointAsync(Guid executionId, CancellationToken ct = default);
}

public class SkillChainExecutionService : ISkillChainExecutionService
{
    private readonly AiForgeDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISessionStateService _sessionStateService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SkillChainExecutionService(
        AiForgeDbContext context,
        IUnitOfWork unitOfWork,
        ISessionStateService sessionStateService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _sessionStateService = sessionStateService;
    }

    public async Task<SkillChainExecutionDto> StartExecutionAsync(StartChainExecutionRequest request, CancellationToken ct = default)
    {
        var chain = await _context.SkillChains
            .Include(sc => sc.Links.OrderBy(l => l.Position))
            .FirstOrDefaultAsync(sc => sc.Id == request.SkillChainId, ct);

        if (chain == null)
            throw new InvalidOperationException($"Chain with ID '{request.SkillChainId}' not found");

        if (!chain.IsPublished)
            throw new InvalidOperationException("Cannot execute an unpublished chain");

        if (!chain.Links.Any())
            throw new InvalidOperationException("Cannot execute a chain with no links");

        var firstLink = chain.Links.OrderBy(l => l.Position).First();

        // Initialize session state options (default to enabled)
        var sessionOptions = request.SessionStateOptions ?? ChainSessionStateOptions.Default;
        var executionId = Guid.NewGuid();
        var sessionId = sessionOptions.SessionId ?? $"chain-exec-{executionId}";

        // Build initial execution context with session state options
        var executionContext = new Dictionary<string, object>
        {
            ["sessionStateOptions"] = new
            {
                enabled = sessionOptions.AutoSaveOnLinkComplete || sessionOptions.AutoLoadOnStart ||
                          sessionOptions.AutoClearOnComplete || sessionOptions.AutoSaveOnPause ||
                          sessionOptions.AutoSaveOnCancel,
                sessionId,
                autoSaveOnLinkComplete = sessionOptions.AutoSaveOnLinkComplete,
                autoLoadOnStart = sessionOptions.AutoLoadOnStart,
                autoClearOnComplete = sessionOptions.AutoClearOnComplete,
                autoSaveOnPause = sessionOptions.AutoSaveOnPause,
                autoSaveOnCancel = sessionOptions.AutoSaveOnCancel,
                sessionExpiryHours = sessionOptions.SessionExpiryHours
            }
        };

        // Try to load existing session state if enabled
        string? resumedContext = null;
        if (sessionOptions.AutoLoadOnStart)
        {
            var existingState = await _sessionStateService.LoadAsync(sessionId, ct);
            if (existingState != null && !existingState.IsExpired)
            {
                executionContext["resumedFromSession"] = new
                {
                    previousPhase = existingState.CurrentPhase,
                    workingSummary = existingState.WorkingSummary,
                    checkpoint = existingState.Checkpoint
                };
                resumedContext = existingState.WorkingSummary;
            }
        }

        var execution = new SkillChainExecution
        {
            Id = executionId,
            SkillChainId = request.SkillChainId,
            TicketId = request.TicketId,
            Status = ChainExecutionStatus.Running,
            CurrentLinkId = firstLink.Id,
            InputValues = request.InputValues,
            ExecutionContext = JsonSerializer.Serialize(executionContext, JsonOptions),
            TotalFailureCount = 0,
            RequiresHumanIntervention = false,
            StartedAt = DateTime.UtcNow,
            StartedBy = request.StartedBy
        };

        _context.SkillChainExecutions.Add(execution);

        // Create initial link execution
        var linkExecution = new SkillChainLinkExecution
        {
            Id = Guid.NewGuid(),
            SkillChainExecutionId = execution.Id,
            SkillChainLinkId = firstLink.Id,
            AttemptNumber = 1,
            Outcome = LinkExecutionOutcome.Pending,
            StartedAt = DateTime.UtcNow
        };

        _context.SkillChainLinkExecutions.Add(linkExecution);

        await _unitOfWork.SaveChangesAsync(ct);

        // Save initial session state
        if (sessionOptions.AutoSaveOnLinkComplete)
        {
            await SaveSessionStateAsync(
                execution,
                chain,
                firstLink,
                $"Chain execution started. {(resumedContext != null ? $"Resumed from previous session. {resumedContext}" : $"Starting with link: {firstLink.Name}")}",
                ct);
        }

        return await GetExecutionAsync(execution.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created execution");
    }

    public async Task<SkillChainExecutionDto?> GetExecutionAsync(Guid executionId, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .AsNoTracking()
            .Include(e => e.SkillChain)
            .Include(e => e.Ticket)
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions.OrderBy(le => le.StartedAt))
                .ThenInclude(le => le.SkillChainLink)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        return execution != null ? MapToDto(execution) : null;
    }

    public async Task<IEnumerable<SkillChainExecutionSummaryDto>> GetExecutionsAsync(Guid? chainId, Guid? ticketId, string? status, CancellationToken ct = default)
    {
        IQueryable<SkillChainExecution> query = _context.SkillChainExecutions
            .AsNoTracking()
            .Include(e => e.SkillChain)
            .Include(e => e.Ticket)
            .Include(e => e.CurrentLink);

        if (chainId.HasValue)
            query = query.Where(e => e.SkillChainId == chainId);

        if (ticketId.HasValue)
            query = query.Where(e => e.TicketId == ticketId);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<ChainExecutionStatus>(status, ignoreCase: true);
            query = query.Where(e => e.Status == statusEnum);
        }

        var executions = await query
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);

        return executions.Select(MapToSummaryDto);
    }

    public async Task<SkillChainExecutionDto?> PauseExecutionAsync(Guid executionId, string reason, string pausedBy, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.SkillChain)
                .ThenInclude(sc => sc.Links)
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (execution.Status != ChainExecutionStatus.Running)
            throw new InvalidOperationException("Can only pause a running execution");

        execution.Status = ChainExecutionStatus.Paused;
        execution.InterventionReason = reason;

        await _unitOfWork.SaveChangesAsync(ct);

        // Save session state on pause
        var config = GetSessionStateConfig(execution);
        if (config?.AutoSaveOnPause == true)
        {
            var summary = BuildWorkingSummary(execution, execution.CurrentLink, execution.LinkExecutions,
                $"Paused by {pausedBy}: {reason}");
            await SaveSessionStateAsync(execution, execution.SkillChain, execution.CurrentLink, summary, ct);
        }

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<SkillChainExecutionDto?> ResumeExecutionAsync(Guid executionId, ResumeExecutionRequest request, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.CurrentLink)
            .Include(e => e.Checkpoints)
                .ThenInclude(c => c.Link)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (execution.Status != ChainExecutionStatus.Paused)
            throw new InvalidOperationException("Can only resume a paused execution");

        execution.Status = ChainExecutionStatus.Running;
        execution.RequiresHumanIntervention = false;
        execution.InterventionReason = null;

        // Load checkpoint context if available
        var latestCheckpoint = execution.Checkpoints
            .OrderByDescending(c => c.Position)
            .FirstOrDefault();

        if (latestCheckpoint != null)
        {
            // Merge checkpoint data into execution context
            var currentContext = string.IsNullOrEmpty(execution.ExecutionContext)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(execution.ExecutionContext, JsonOptions) ?? new Dictionary<string, object>();

            currentContext["resumedFromCheckpoint"] = new
            {
                checkpointId = latestCheckpoint.Id.ToString(),
                checkpointPosition = latestCheckpoint.Position,
                checkpointLinkName = latestCheckpoint.Link?.Name,
                checkpointData = latestCheckpoint.CheckpointData
            };

            execution.ExecutionContext = JsonSerializer.Serialize(currentContext, JsonOptions);
        }

        // Merge additional context if provided
        if (!string.IsNullOrEmpty(request.AdditionalContext))
        {
            var currentContext = string.IsNullOrEmpty(execution.ExecutionContext)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(execution.ExecutionContext, JsonOptions) ?? new Dictionary<string, object>();

            var additionalData = JsonSerializer.Deserialize<Dictionary<string, object>>(request.AdditionalContext, JsonOptions);
            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                    currentContext[kvp.Key] = kvp.Value;
            }

            execution.ExecutionContext = JsonSerializer.Serialize(currentContext, JsonOptions);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<ExecutionCheckpointDto?> GetLatestCheckpointAsync(Guid executionId, CancellationToken ct = default)
    {
        var checkpoint = await _context.ExecutionCheckpoints
            .AsNoTracking()
            .Include(c => c.Link)
            .Where(c => c.ExecutionId == executionId)
            .OrderByDescending(c => c.Position)
            .FirstOrDefaultAsync(ct);

        if (checkpoint == null)
            return null;

        return new ExecutionCheckpointDto
        {
            Id = checkpoint.Id,
            ExecutionId = checkpoint.ExecutionId,
            LinkId = checkpoint.LinkId,
            LinkName = checkpoint.Link?.Name,
            Position = checkpoint.Position,
            CheckpointData = checkpoint.CheckpointData,
            CreatedAt = checkpoint.CreatedAt
        };
    }

    public async Task<SkillChainExecutionDto?> CancelExecutionAsync(Guid executionId, string reason, string cancelledBy, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.SkillChain)
                .ThenInclude(sc => sc.Links)
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (execution.Status == ChainExecutionStatus.Completed || execution.Status == ChainExecutionStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel an already completed or cancelled execution");

        execution.Status = ChainExecutionStatus.Cancelled;
        execution.CompletedAt = DateTime.UtcNow;
        execution.CompletedBy = cancelledBy;
        execution.InterventionReason = reason;

        // Mark any pending link executions as skipped
        foreach (var linkExec in execution.LinkExecutions.Where(le => le.Outcome == LinkExecutionOutcome.Pending))
        {
            linkExec.Outcome = LinkExecutionOutcome.Skipped;
            linkExec.CompletedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Save session state on cancel (preserves context for potential restart)
        var config = GetSessionStateConfig(execution);
        if (config?.AutoSaveOnCancel == true)
        {
            var summary = BuildWorkingSummary(execution, execution.CurrentLink, execution.LinkExecutions,
                $"Cancelled by {cancelledBy}: {reason}");
            await SaveSessionStateAsync(execution, execution.SkillChain, execution.CurrentLink, summary, ct);
        }

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<SkillChainLinkExecutionDto> RecordLinkOutcomeAsync(Guid executionId, RecordLinkOutcomeRequest request, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.SkillChain)
                .ThenInclude(sc => sc.Links)
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions)
                .ThenInclude(le => le.SkillChainLink)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null)
            throw new InvalidOperationException($"Execution with ID '{executionId}' not found");

        if (execution.Status != ChainExecutionStatus.Running)
            throw new InvalidOperationException("Can only record outcomes for running executions");

        // Find the current pending link execution
        var currentLinkExec = execution.LinkExecutions
            .Where(le => le.SkillChainLinkId == request.LinkId && le.Outcome == LinkExecutionOutcome.Pending)
            .OrderByDescending(le => le.AttemptNumber)
            .FirstOrDefault();

        if (currentLinkExec == null)
            throw new InvalidOperationException("No pending link execution found for this link");

        var outcome = Enum.Parse<LinkExecutionOutcome>(request.Outcome, ignoreCase: true);
        currentLinkExec.Outcome = outcome;
        currentLinkExec.Output = request.Output;
        currentLinkExec.ErrorDetails = request.ErrorDetails;
        currentLinkExec.CompletedAt = DateTime.UtcNow;
        currentLinkExec.ExecutedBy = request.ExecutedBy;

        var escalated = false;
        if (outcome == LinkExecutionOutcome.Failure)
        {
            execution.TotalFailureCount++;

            // Check if we need to escalate
            if (execution.TotalFailureCount >= execution.SkillChain.MaxTotalFailures)
            {
                execution.Status = ChainExecutionStatus.Paused;
                execution.RequiresHumanIntervention = true;
                execution.InterventionReason = $"Total failures ({execution.TotalFailureCount}) exceeded maximum ({execution.SkillChain.MaxTotalFailures})";
                escalated = true;
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Save execution checkpoint on success
        if (outcome == LinkExecutionOutcome.Success && currentLinkExec.SkillChainLink != null)
        {
            await SaveCheckpointAsync(execution, currentLinkExec.SkillChainLink, request.Output, ct);
        }

        // Save session state after recording outcome
        var config = GetSessionStateConfig(execution);
        if (config?.AutoSaveOnLinkComplete == true)
        {
            var linkName = currentLinkExec.SkillChainLink?.Name ?? "Unknown";
            var additionalContext = outcome == LinkExecutionOutcome.Success
                ? $"Link '{linkName}' completed successfully"
                : $"Link '{linkName}' failed: {request.ErrorDetails}";

            if (escalated)
                additionalContext += $". Escalated: {execution.InterventionReason}";

            var summary = BuildWorkingSummary(execution, execution.CurrentLink, execution.LinkExecutions, additionalContext);
            await SaveSessionStateAsync(execution, execution.SkillChain, execution.CurrentLink, summary, ct);
        }

        // Reload with navigation properties
        var savedLinkExec = await _context.SkillChainLinkExecutions
            .AsNoTracking()
            .Include(le => le.SkillChainLink)
            .FirstOrDefaultAsync(le => le.Id == currentLinkExec.Id, ct);

        return MapToLinkExecutionDto(savedLinkExec!);
    }

    public async Task<SkillChainExecutionDto?> AdvanceExecutionAsync(Guid executionId, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.SkillChain)
                .ThenInclude(sc => sc.Links.OrderBy(l => l.Position))
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions)
                .ThenInclude(le => le.SkillChainLink)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (execution.Status != ChainExecutionStatus.Running)
            throw new InvalidOperationException("Can only advance a running execution");

        if (execution.CurrentLink == null)
            throw new InvalidOperationException("Execution has no current link");

        // Get the most recent link execution for the current link
        var lastLinkExec = execution.LinkExecutions
            .Where(le => le.SkillChainLinkId == execution.CurrentLinkId)
            .OrderByDescending(le => le.AttemptNumber)
            .FirstOrDefault();

        if (lastLinkExec == null || lastLinkExec.Outcome == LinkExecutionOutcome.Pending)
            throw new InvalidOperationException("Current link execution has not been completed");

        var currentLink = execution.CurrentLink;
        TransitionType transition;
        Guid? targetLinkId;

        if (lastLinkExec.Outcome == LinkExecutionOutcome.Success)
        {
            transition = currentLink.OnSuccessTransition;
            targetLinkId = currentLink.OnSuccessTargetLinkId;
        }
        else // Failure
        {
            // Check retry count
            var attemptCount = execution.LinkExecutions.Count(le => le.SkillChainLinkId == currentLink.Id);
            if (attemptCount < currentLink.MaxRetries && currentLink.OnFailureTransition == TransitionType.Retry)
            {
                transition = TransitionType.Retry;
                targetLinkId = null;
            }
            else
            {
                transition = currentLink.OnFailureTransition;
                targetLinkId = currentLink.OnFailureTargetLinkId;
            }
        }

        // Record the transition taken
        lastLinkExec.TransitionTaken = transition;

        SkillChainLink? newCurrentLink = null;
        var chainCompleted = false;
        var escalated = false;

        switch (transition)
        {
            case TransitionType.Complete:
                execution.Status = ChainExecutionStatus.Completed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.CompletedBy = "system";
                execution.CurrentLinkId = null;
                chainCompleted = true;
                break;

            case TransitionType.NextLink:
                var nextLink = execution.SkillChain.Links
                    .Where(l => l.Position > currentLink.Position)
                    .OrderBy(l => l.Position)
                    .FirstOrDefault();

                if (nextLink == null)
                {
                    // No more links, complete the chain
                    execution.Status = ChainExecutionStatus.Completed;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.CompletedBy = "system";
                    execution.CurrentLinkId = null;
                    chainCompleted = true;
                }
                else
                {
                    execution.CurrentLinkId = nextLink.Id;
                    newCurrentLink = nextLink;
                    await CreateLinkExecutionAsync(execution.Id, nextLink.Id, ct);
                }
                break;

            case TransitionType.GoToLink:
                if (!targetLinkId.HasValue)
                    throw new InvalidOperationException("GoToLink transition requires a target link");

                execution.CurrentLinkId = targetLinkId.Value;
                newCurrentLink = execution.SkillChain.Links.FirstOrDefault(l => l.Id == targetLinkId.Value);
                await CreateLinkExecutionAsync(execution.Id, targetLinkId.Value, ct);
                break;

            case TransitionType.Retry:
                var attemptNumber = execution.LinkExecutions.Count(le => le.SkillChainLinkId == currentLink.Id) + 1;
                newCurrentLink = currentLink;
                await CreateLinkExecutionAsync(execution.Id, currentLink.Id, ct, attemptNumber);
                break;

            case TransitionType.Escalate:
                execution.Status = ChainExecutionStatus.Paused;
                execution.RequiresHumanIntervention = true;
                execution.InterventionReason = $"Link '{currentLink.Name}' failed and requires human intervention";
                escalated = true;
                break;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Handle session state based on transition outcome
        var config = GetSessionStateConfig(execution);
        if (config != null && config.Enabled)
        {
            if (chainCompleted && config.AutoClearOnComplete)
            {
                // Clear session state on successful completion
                await ClearSessionStateAsync(execution, ct);
            }
            else if (escalated && config.AutoSaveOnPause)
            {
                // Save state when escalated
                var summary = BuildWorkingSummary(execution, currentLink, execution.LinkExecutions,
                    $"Escalated at link '{currentLink.Name}'");
                await SaveSessionStateAsync(execution, execution.SkillChain, currentLink, summary, ct);
            }
            else if (newCurrentLink != null && config.AutoSaveOnLinkComplete)
            {
                // Save state when advancing to new link
                var summary = BuildWorkingSummary(execution, newCurrentLink, execution.LinkExecutions,
                    $"Advanced to link '{newCurrentLink.Name}' (position {newCurrentLink.Position})");
                await SaveSessionStateAsync(execution, execution.SkillChain, newCurrentLink, summary, ct);
            }
        }

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<IEnumerable<SkillChainExecutionSummaryDto>> GetPendingInterventionsAsync(Guid? projectId, CancellationToken ct = default)
    {
        IQueryable<SkillChainExecution> query = _context.SkillChainExecutions
            .AsNoTracking()
            .Include(e => e.SkillChain)
            .Include(e => e.Ticket)
            .Include(e => e.CurrentLink)
            .Where(e => e.RequiresHumanIntervention && e.Status == ChainExecutionStatus.Paused);

        if (projectId.HasValue)
        {
            query = query.Where(e => e.SkillChain.ProjectId == projectId);
        }

        var executions = await query
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);

        return executions.Select(MapToSummaryDto);
    }

    public async Task<SkillChainExecutionDto?> ResolveInterventionAsync(Guid executionId, ResolveInterventionRequest request, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.SkillChain)
                .ThenInclude(sc => sc.Links)
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions)
                .ThenInclude(le => le.SkillChainLink)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (!execution.RequiresHumanIntervention || execution.Status != ChainExecutionStatus.Paused)
            throw new InvalidOperationException("Execution does not require intervention or is not paused");

        var nextAction = Enum.Parse<TransitionType>(request.NextAction, ignoreCase: true);
        var chainCompleted = false;
        SkillChainLink? newLink = null;

        switch (nextAction)
        {
            case TransitionType.Retry:
                if (execution.CurrentLinkId == null)
                    throw new InvalidOperationException("No current link to retry");

                execution.Status = ChainExecutionStatus.Running;
                execution.RequiresHumanIntervention = false;
                execution.InterventionReason = null;
                newLink = execution.CurrentLink;
                await CreateLinkExecutionAsync(execution.Id, execution.CurrentLinkId.Value, ct);
                break;

            case TransitionType.GoToLink:
                if (!request.TargetLinkId.HasValue)
                    throw new InvalidOperationException("GoToLink requires a target link");

                var targetLink = execution.SkillChain.Links.FirstOrDefault(l => l.Id == request.TargetLinkId);
                if (targetLink == null)
                    throw new InvalidOperationException($"Target link with ID '{request.TargetLinkId}' not found in chain");

                execution.CurrentLinkId = request.TargetLinkId.Value;
                execution.Status = ChainExecutionStatus.Running;
                execution.RequiresHumanIntervention = false;
                execution.InterventionReason = null;
                newLink = targetLink;
                await CreateLinkExecutionAsync(execution.Id, request.TargetLinkId.Value, ct);
                break;

            case TransitionType.Complete:
                execution.Status = ChainExecutionStatus.Completed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.CompletedBy = request.ResolvedBy ?? "human";
                execution.RequiresHumanIntervention = false;
                chainCompleted = true;
                break;

            case TransitionType.Escalate:
                // Keep paused, update reason
                execution.InterventionReason = request.Resolution;
                break;

            default:
                throw new InvalidOperationException($"Invalid intervention action: {request.NextAction}");
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Handle session state after intervention resolution
        var config = GetSessionStateConfig(execution);
        if (config != null && config.Enabled)
        {
            if (chainCompleted && config.AutoClearOnComplete)
            {
                await ClearSessionStateAsync(execution, ct);
            }
            else if (newLink != null && config.AutoSaveOnLinkComplete)
            {
                var summary = BuildWorkingSummary(execution, newLink, execution.LinkExecutions,
                    $"Intervention resolved by {request.ResolvedBy ?? "human"}: {request.Resolution}. Resuming at link '{newLink.Name}'");
                await SaveSessionStateAsync(execution, execution.SkillChain, newLink, summary, ct);
            }
        }

        return await GetExecutionAsync(executionId, ct);
    }

    private async Task CreateLinkExecutionAsync(Guid executionId, Guid linkId, CancellationToken ct, int? attemptNumber = null)
    {
        var existingAttempts = await _context.SkillChainLinkExecutions
            .CountAsync(le => le.SkillChainExecutionId == executionId && le.SkillChainLinkId == linkId, ct);

        var linkExecution = new SkillChainLinkExecution
        {
            Id = Guid.NewGuid(),
            SkillChainExecutionId = executionId,
            SkillChainLinkId = linkId,
            AttemptNumber = attemptNumber ?? (existingAttempts + 1),
            Outcome = LinkExecutionOutcome.Pending,
            StartedAt = DateTime.UtcNow
        };

        _context.SkillChainLinkExecutions.Add(linkExecution);
    }

    private static SkillChainExecutionDto MapToDto(SkillChainExecution execution)
    {
        var config = GetSessionStateConfig(execution);

        return new SkillChainExecutionDto
        {
            Id = execution.Id,
            SkillChainId = execution.SkillChainId,
            ChainKey = execution.SkillChain?.ChainKey,
            ChainName = execution.SkillChain?.Name,
            TicketId = execution.TicketId,
            TicketKey = execution.Ticket?.Key,
            Status = execution.Status.ToString(),
            CurrentLinkId = execution.CurrentLinkId,
            CurrentLinkName = execution.CurrentLink?.Name,
            CurrentLinkPosition = execution.CurrentLink?.Position,
            InputValues = execution.InputValues,
            ExecutionContext = execution.ExecutionContext,
            TotalFailureCount = execution.TotalFailureCount,
            RequiresHumanIntervention = execution.RequiresHumanIntervention,
            InterventionReason = execution.InterventionReason,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            StartedBy = execution.StartedBy,
            CompletedBy = execution.CompletedBy,
            LinkExecutions = execution.LinkExecutions
                .OrderBy(le => le.StartedAt)
                .Select(MapToLinkExecutionDto)
                .ToList(),

            // Session state info
            SessionId = config?.SessionId,
            SessionStateEnabled = config?.Enabled ?? false,
            SessionPhase = execution.CurrentLink != null
                ? InferPhaseFromLink(execution.CurrentLink, execution.SkillChain?.Links.Count ?? 1)
                : (execution.Status == ChainExecutionStatus.Completed ? "Finalizing" : null)
        };
    }

    private static SkillChainExecutionSummaryDto MapToSummaryDto(SkillChainExecution execution)
    {
        return new SkillChainExecutionSummaryDto
        {
            Id = execution.Id,
            SkillChainId = execution.SkillChainId,
            ChainName = execution.SkillChain?.Name,
            TicketId = execution.TicketId,
            TicketKey = execution.Ticket?.Key,
            Status = execution.Status.ToString(),
            CurrentLinkName = execution.CurrentLink?.Name,
            TotalFailureCount = execution.TotalFailureCount,
            RequiresHumanIntervention = execution.RequiresHumanIntervention,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt
        };
    }

    private static SkillChainLinkExecutionDto MapToLinkExecutionDto(SkillChainLinkExecution linkExec)
    {
        return new SkillChainLinkExecutionDto
        {
            Id = linkExec.Id,
            SkillChainExecutionId = linkExec.SkillChainExecutionId,
            SkillChainLinkId = linkExec.SkillChainLinkId,
            LinkName = linkExec.SkillChainLink?.Name,
            LinkPosition = linkExec.SkillChainLink?.Position,
            AttemptNumber = linkExec.AttemptNumber,
            Outcome = linkExec.Outcome.ToString(),
            Input = linkExec.Input,
            Output = linkExec.Output,
            ErrorDetails = linkExec.ErrorDetails,
            TransitionTaken = linkExec.TransitionTaken?.ToString(),
            StartedAt = linkExec.StartedAt,
            CompletedAt = linkExec.CompletedAt,
            ExecutedBy = linkExec.ExecutedBy
        };
    }

    #region Checkpoint Methods

    /// <summary>
    /// Saves a checkpoint after successful link completion.
    /// </summary>
    private async Task SaveCheckpointAsync(
        SkillChainExecution execution,
        SkillChainLink link,
        string? output,
        CancellationToken ct)
    {
        var checkpoint = new ExecutionCheckpoint
        {
            Id = Guid.NewGuid(),
            ExecutionId = execution.Id,
            LinkId = link.Id,
            Position = link.Position,
            CheckpointData = BuildCheckpointData(link.Name, output),
            CreatedAt = DateTime.UtcNow
        };

        _context.ExecutionCheckpoints.Add(checkpoint);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Builds checkpoint data by extracting key fields based on link type.
    /// </summary>
    private static string BuildCheckpointData(string? linkName, string? output)
    {
        if (string.IsNullOrEmpty(output))
            return "{}";

        try
        {
            using var outputDoc = JsonDocument.Parse(output);
            var root = outputDoc.RootElement;

            // Extract fields based on link type
            var checkpointFields = new Dictionary<string, object?>();
            var normalizedName = linkName?.ToLowerInvariant() ?? "";

            string[] fieldsToExtract = normalizedName switch
            {
                var n when n.Contains("research") => ["ticketKey", "affectedAreas", "researchSummary"],
                var n when n.Contains("plan") => ["planId", "estimatedEffort", "keyDecisions"],
                var n when n.Contains("review") => ["approved", "feedbackSummary"],
                var n when n.Contains("implement") => ["filesChanged", "completedItems"],
                var n when n.Contains("organize") => ["queueId", "subTickets", "subTicketCount"],
                var n when n.Contains("finalize") => ["handoffId", "summary"],
                _ => ["summary", "outcome"]
            };

            foreach (var field in fieldsToExtract)
            {
                if (root.TryGetProperty(field, out var value))
                {
                    checkpointFields[field] = value.ValueKind switch
                    {
                        JsonValueKind.String => value.GetString(),
                        JsonValueKind.Number => value.TryGetInt32(out var i) ? i : value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Array => JsonSerializer.Deserialize<object[]>(value.GetRawText()),
                        JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(value.GetRawText()),
                        _ => null
                    };
                }
            }

            // Always include link name for context
            checkpointFields["linkName"] = linkName;

            return JsonSerializer.Serialize(checkpointFields, JsonOptions);
        }
        catch
        {
            // Fallback: truncate raw output
            var truncated = output.Length > 500 ? output[..500] : output;
            return JsonSerializer.Serialize(new { linkName, raw = truncated }, JsonOptions);
        }
    }

    #endregion

    #region Session State Helpers

    /// <summary>
    /// Gets session state options from execution context.
    /// </summary>
    private static SessionStateConfig? GetSessionStateConfig(SkillChainExecution execution)
    {
        if (string.IsNullOrEmpty(execution.ExecutionContext))
            return null;

        try
        {
            var context = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(execution.ExecutionContext, JsonOptions);
            if (context == null || !context.TryGetValue("sessionStateOptions", out var optionsElement))
                return null;

            return new SessionStateConfig
            {
                Enabled = optionsElement.TryGetProperty("enabled", out var e) && e.GetBoolean(),
                SessionId = optionsElement.TryGetProperty("sessionId", out var s) ? s.GetString() : null,
                AutoSaveOnLinkComplete = optionsElement.TryGetProperty("autoSaveOnLinkComplete", out var asl) && asl.GetBoolean(),
                AutoLoadOnStart = optionsElement.TryGetProperty("autoLoadOnStart", out var al) && al.GetBoolean(),
                AutoClearOnComplete = optionsElement.TryGetProperty("autoClearOnComplete", out var ac) && ac.GetBoolean(),
                AutoSaveOnPause = optionsElement.TryGetProperty("autoSaveOnPause", out var asp) && asp.GetBoolean(),
                AutoSaveOnCancel = optionsElement.TryGetProperty("autoSaveOnCancel", out var asc) && asc.GetBoolean(),
                SessionExpiryHours = optionsElement.TryGetProperty("sessionExpiryHours", out var seh) ? seh.GetInt32() : 24
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves session state for the current execution state.
    /// </summary>
    private async Task SaveSessionStateAsync(
        SkillChainExecution execution,
        SkillChain chain,
        SkillChainLink? currentLink,
        string workingSummary,
        CancellationToken ct)
    {
        var config = GetSessionStateConfig(execution);
        if (config == null || !config.Enabled || string.IsNullOrEmpty(config.SessionId))
            return;

        var phase = InferPhaseFromLink(currentLink, chain.Links.Count);

        // Build checkpoint with execution state
        var checkpoint = new Dictionary<string, object>
        {
            ["executionId"] = execution.Id.ToString(),
            ["chainId"] = chain.Id.ToString(),
            ["chainName"] = chain.Name ?? "",
            ["status"] = execution.Status.ToString(),
            ["currentLinkId"] = currentLink?.Id.ToString() ?? "",
            ["currentLinkName"] = currentLink?.Name ?? "",
            ["currentLinkPosition"] = currentLink?.Position ?? 0,
            ["totalLinks"] = chain.Links.Count,
            ["totalFailureCount"] = execution.TotalFailureCount,
            ["requiresHumanIntervention"] = execution.RequiresHumanIntervention
        };

        if (!string.IsNullOrEmpty(execution.InterventionReason))
            checkpoint["interventionReason"] = execution.InterventionReason;

        await _sessionStateService.SaveAsync(new SaveSessionStateRequest
        {
            SessionId = config.SessionId,
            TicketId = execution.TicketId,
            CurrentPhase = phase,
            WorkingSummary = TruncateWorkingSummary(workingSummary),
            Checkpoint = checkpoint,
            ExpiresInHours = config.SessionExpiryHours
        }, ct);
    }

    /// <summary>
    /// Clears session state for the execution.
    /// </summary>
    private async Task ClearSessionStateAsync(SkillChainExecution execution, CancellationToken ct)
    {
        var config = GetSessionStateConfig(execution);
        if (config == null || !config.Enabled || string.IsNullOrEmpty(config.SessionId))
            return;

        await _sessionStateService.ClearAsync(config.SessionId, ct);
    }

    /// <summary>
    /// Infers the session phase from the current link name and position.
    /// </summary>
    private static string InferPhaseFromLink(SkillChainLink? link, int totalLinks)
    {
        if (link == null)
            return "Finalizing";

        var linkName = link.Name?.ToLowerInvariant() ?? "";

        // Match by link name keywords
        if (linkName.Contains("research") || linkName.Contains("explore") || linkName.Contains("discover"))
            return "Researching";
        if (linkName.Contains("plan") || linkName.Contains("design") || linkName.Contains("architect"))
            return "Planning";
        if (linkName.Contains("implement") || linkName.Contains("code") || linkName.Contains("build") || linkName.Contains("develop"))
            return "Implementing";
        if (linkName.Contains("review") || linkName.Contains("check") || linkName.Contains("validate"))
            return "Reviewing";
        if (linkName.Contains("test") || linkName.Contains("verify"))
            return "Testing";
        if (linkName.Contains("final") || linkName.Contains("complete") || linkName.Contains("deploy") || linkName.Contains("handoff"))
            return "Finalizing";

        // Fall back to position-based inference
        if (totalLinks <= 1)
            return "Implementing";

        var progress = (double)link.Position / (totalLinks - 1);
        return progress switch
        {
            < 0.2 => "Researching",
            < 0.4 => "Planning",
            < 0.7 => "Implementing",
            < 0.9 => "Reviewing",
            _ => "Finalizing"
        };
    }

    /// <summary>
    /// Builds a working summary from the execution state and recent link executions.
    /// </summary>
    private static string BuildWorkingSummary(
        SkillChainExecution execution,
        SkillChainLink? currentLink,
        IEnumerable<SkillChainLinkExecution>? recentLinkExecutions,
        string? additionalContext = null)
    {
        var parts = new List<string>();

        // Current state
        parts.Add($"Chain: {execution.SkillChain?.Name ?? "Unknown"}");
        parts.Add($"Status: {execution.Status}");

        if (currentLink != null)
            parts.Add($"Current link: {currentLink.Name} (position {currentLink.Position})");

        if (execution.TotalFailureCount > 0)
            parts.Add($"Failures: {execution.TotalFailureCount}");

        if (execution.RequiresHumanIntervention)
            parts.Add($"Requires intervention: {execution.InterventionReason}");

        // Recent completed links
        if (recentLinkExecutions != null)
        {
            var completed = recentLinkExecutions
                .Where(le => le.Outcome != LinkExecutionOutcome.Pending)
                .OrderByDescending(le => le.CompletedAt)
                .Take(3)
                .ToList();

            if (completed.Any())
            {
                parts.Add("Recent links: " + string.Join(", ", completed.Select(le =>
                    $"{le.SkillChainLink?.Name ?? "?"} ({le.Outcome})")));
            }
        }

        // Additional context
        if (!string.IsNullOrEmpty(additionalContext))
            parts.Add(additionalContext);

        return string.Join(". ", parts);
    }

    /// <summary>
    /// Truncates working summary to fit the 4000 character limit.
    /// </summary>
    private static string TruncateWorkingSummary(string summary)
    {
        const int maxLength = 4000;
        if (string.IsNullOrEmpty(summary) || summary.Length <= maxLength)
            return summary;

        return summary[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Internal config class for session state options.
    /// </summary>
    private class SessionStateConfig
    {
        public bool Enabled { get; set; }
        public string? SessionId { get; set; }
        public bool AutoSaveOnLinkComplete { get; set; }
        public bool AutoLoadOnStart { get; set; }
        public bool AutoClearOnComplete { get; set; }
        public bool AutoSaveOnPause { get; set; }
        public bool AutoSaveOnCancel { get; set; }
        public int SessionExpiryHours { get; set; } = 24;
    }

    #endregion
}
