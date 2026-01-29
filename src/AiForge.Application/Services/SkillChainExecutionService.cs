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
}

public class SkillChainExecutionService : ISkillChainExecutionService
{
    private readonly AiForgeDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public SkillChainExecutionService(AiForgeDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
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

        var execution = new SkillChainExecution
        {
            Id = Guid.NewGuid(),
            SkillChainId = request.SkillChainId,
            TicketId = request.TicketId,
            Status = ChainExecutionStatus.Running,
            CurrentLinkId = firstLink.Id,
            InputValues = request.InputValues,
            ExecutionContext = "{}",
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
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (execution.Status != ChainExecutionStatus.Running)
            throw new InvalidOperationException("Can only pause a running execution");

        execution.Status = ChainExecutionStatus.Paused;
        execution.InterventionReason = reason;

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<SkillChainExecutionDto?> ResumeExecutionAsync(Guid executionId, ResumeExecutionRequest request, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.CurrentLink)
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (execution.Status != ChainExecutionStatus.Paused)
            throw new InvalidOperationException("Can only resume a paused execution");

        execution.Status = ChainExecutionStatus.Running;
        execution.RequiresHumanIntervention = false;
        execution.InterventionReason = null;

        // Merge additional context if provided
        if (!string.IsNullOrEmpty(request.AdditionalContext))
        {
            // In production, properly merge JSON
            execution.ExecutionContext = request.AdditionalContext;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<SkillChainExecutionDto?> CancelExecutionAsync(Guid executionId, string reason, string cancelledBy, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
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

        return await GetExecutionAsync(executionId, ct);
    }

    public async Task<SkillChainLinkExecutionDto> RecordLinkOutcomeAsync(Guid executionId, RecordLinkOutcomeRequest request, CancellationToken ct = default)
    {
        var execution = await _context.SkillChainExecutions
            .Include(e => e.SkillChain)
            .Include(e => e.CurrentLink)
            .Include(e => e.LinkExecutions)
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

        if (outcome == LinkExecutionOutcome.Failure)
        {
            execution.TotalFailureCount++;

            // Check if we need to escalate
            if (execution.TotalFailureCount >= execution.SkillChain.MaxTotalFailures)
            {
                execution.Status = ChainExecutionStatus.Paused;
                execution.RequiresHumanIntervention = true;
                execution.InterventionReason = $"Total failures ({execution.TotalFailureCount}) exceeded maximum ({execution.SkillChain.MaxTotalFailures})";
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

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

        switch (transition)
        {
            case TransitionType.Complete:
                execution.Status = ChainExecutionStatus.Completed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.CompletedBy = "system";
                execution.CurrentLinkId = null;
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
                }
                else
                {
                    execution.CurrentLinkId = nextLink.Id;
                    await CreateLinkExecutionAsync(execution.Id, nextLink.Id, ct);
                }
                break;

            case TransitionType.GoToLink:
                if (!targetLinkId.HasValue)
                    throw new InvalidOperationException("GoToLink transition requires a target link");

                execution.CurrentLinkId = targetLinkId.Value;
                await CreateLinkExecutionAsync(execution.Id, targetLinkId.Value, ct);
                break;

            case TransitionType.Retry:
                var attemptNumber = execution.LinkExecutions.Count(le => le.SkillChainLinkId == currentLink.Id) + 1;
                await CreateLinkExecutionAsync(execution.Id, currentLink.Id, ct, attemptNumber);
                break;

            case TransitionType.Escalate:
                execution.Status = ChainExecutionStatus.Paused;
                execution.RequiresHumanIntervention = true;
                execution.InterventionReason = $"Link '{currentLink.Name}' failed and requires human intervention";
                break;
        }

        await _unitOfWork.SaveChangesAsync(ct);

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
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);

        if (execution == null) return null;

        if (!execution.RequiresHumanIntervention || execution.Status != ChainExecutionStatus.Paused)
            throw new InvalidOperationException("Execution does not require intervention or is not paused");

        var nextAction = Enum.Parse<TransitionType>(request.NextAction, ignoreCase: true);

        switch (nextAction)
        {
            case TransitionType.Retry:
                if (execution.CurrentLinkId == null)
                    throw new InvalidOperationException("No current link to retry");

                execution.Status = ChainExecutionStatus.Running;
                execution.RequiresHumanIntervention = false;
                execution.InterventionReason = null;
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
                await CreateLinkExecutionAsync(execution.Id, request.TargetLinkId.Value, ct);
                break;

            case TransitionType.Complete:
                execution.Status = ChainExecutionStatus.Completed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.CompletedBy = request.ResolvedBy ?? "human";
                execution.RequiresHumanIntervention = false;
                break;

            case TransitionType.Escalate:
                // Keep paused, update reason
                execution.InterventionReason = request.Resolution;
                break;

            default:
                throw new InvalidOperationException($"Invalid intervention action: {request.NextAction}");
        }

        await _unitOfWork.SaveChangesAsync(ct);

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
                .ToList()
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
}
