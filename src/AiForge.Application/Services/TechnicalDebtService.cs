using AiForge.Application.DTOs.TechnicalDebt;
using AiForge.Application.Extensions;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface ITechnicalDebtService
{
    Task<TechnicalDebtDto> FlagDebtAsync(Guid ticketId, CreateDebtRequest request, CancellationToken cancellationToken = default);
    Task<TechnicalDebtDto?> UpdateDebtAsync(Guid debtId, UpdateDebtRequest request, CancellationToken cancellationToken = default);
    Task<TechnicalDebtDto?> ResolveDebtAsync(Guid debtId, ResolveDebtRequest request, CancellationToken cancellationToken = default);
    Task<TechnicalDebtDto?> GetDebtByIdAsync(Guid debtId, CancellationToken cancellationToken = default);
    Task<DebtBacklogResponse> GetDebtBacklogAsync(string? status, string? category, string? severity, CancellationToken cancellationToken = default);
    Task<IEnumerable<TechnicalDebtDto>> GetDebtByTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<DebtSummaryResponse> GetDebtSummaryAsync(CancellationToken cancellationToken = default);
}

public class TechnicalDebtService : ITechnicalDebtService
{
    private readonly ITechnicalDebtRepository _debtRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    public TechnicalDebtService(
        ITechnicalDebtRepository debtRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _debtRepository = debtRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    public async Task<TechnicalDebtDto> FlagDebtAsync(Guid ticketId, CreateDebtRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        var debt = new TechnicalDebt
        {
            Id = Guid.NewGuid(),
            OriginatingTicketId = ticketId,
            Title = request.Title,
            Description = request.Description,
            Category = Enum.Parse<DebtCategory>(request.Category, ignoreCase: true),
            Severity = Enum.Parse<DebtSeverity>(request.Severity, ignoreCase: true),
            Status = DebtStatus.Open,
            Rationale = request.Rationale,
            AffectedFiles = request.AffectedFiles,
            SessionId = request.SessionId,
            CreatedAt = DateTime.UtcNow
        };

        await _debtRepository.AddAsync(debt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(debt, ticket.Key, null);
    }

    public async Task<TechnicalDebtDto?> UpdateDebtAsync(Guid debtId, UpdateDebtRequest request, CancellationToken cancellationToken = default)
    {
        var debt = await _debtRepository.GetByIdAsync(debtId, cancellationToken);
        if (debt == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.Title))
            debt.Title = request.Title;

        if (request.Description != null)
            debt.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Category))
            debt.Category = Enum.Parse<DebtCategory>(request.Category, ignoreCase: true);

        if (!string.IsNullOrEmpty(request.Severity))
            debt.Severity = Enum.Parse<DebtSeverity>(request.Severity, ignoreCase: true);

        if (!string.IsNullOrEmpty(request.Status))
            debt.Status = Enum.Parse<DebtStatus>(request.Status, ignoreCase: true);

        if (request.Rationale != null)
            debt.Rationale = request.Rationale;

        if (request.AffectedFiles != null)
            debt.AffectedFiles = request.AffectedFiles;

        await _debtRepository.UpdateAsync(debt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var originatingTicket = await _ticketRepository.GetByIdAsync(debt.OriginatingTicketId, cancellationToken);
        var resolutionTicket = debt.ResolutionTicketId.HasValue
            ? await _ticketRepository.GetByIdAsync(debt.ResolutionTicketId.Value, cancellationToken)
            : null;

        return MapToDto(debt, originatingTicket?.Key, resolutionTicket?.Key);
    }

    public async Task<TechnicalDebtDto?> ResolveDebtAsync(Guid debtId, ResolveDebtRequest request, CancellationToken cancellationToken = default)
    {
        var debt = await _debtRepository.GetByIdAsync(debtId, cancellationToken);
        if (debt == null)
        {
            return null;
        }

        debt.Status = DebtStatus.Resolved;
        debt.ResolutionTicketId = request.ResolutionTicketId;
        debt.ResolvedAt = DateTime.UtcNow;

        await _debtRepository.UpdateAsync(debt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var originatingTicket = await _ticketRepository.GetByIdAsync(debt.OriginatingTicketId, cancellationToken);
        var resolutionTicket = debt.ResolutionTicketId.HasValue
            ? await _ticketRepository.GetByIdAsync(debt.ResolutionTicketId.Value, cancellationToken)
            : null;

        return MapToDto(debt, originatingTicket?.Key, resolutionTicket?.Key);
    }

    public async Task<TechnicalDebtDto?> GetDebtByIdAsync(Guid debtId, CancellationToken cancellationToken = default)
    {
        var debt = await _debtRepository.GetByIdAsync(debtId, cancellationToken);
        if (debt == null)
        {
            return null;
        }

        var originatingTicket = await _ticketRepository.GetByIdAsync(debt.OriginatingTicketId, cancellationToken);

        // Check project access
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null && originatingTicket != null && !accessibleProjects.Contains(originatingTicket.ProjectId))
            return null;

        var resolutionTicket = debt.ResolutionTicketId.HasValue
            ? await _ticketRepository.GetByIdAsync(debt.ResolutionTicketId.Value, cancellationToken)
            : null;

        return MapToDto(debt, originatingTicket?.Key, resolutionTicket?.Key);
    }

    public async Task<DebtBacklogResponse> GetDebtBacklogAsync(string? status, string? category, string? severity, CancellationToken cancellationToken = default)
    {
        DebtStatus? statusEnum = null;
        DebtCategory? categoryEnum = null;
        DebtSeverity? severityEnum = null;

        if (!string.IsNullOrEmpty(status))
            statusEnum = Enum.Parse<DebtStatus>(status, ignoreCase: true);

        if (!string.IsNullOrEmpty(category))
            categoryEnum = Enum.Parse<DebtCategory>(category, ignoreCase: true);

        if (!string.IsNullOrEmpty(severity))
            severityEnum = Enum.Parse<DebtSeverity>(severity, ignoreCase: true);

        var debts = await _debtRepository.GetBacklogAsync(statusEnum, categoryEnum, severityEnum, cancellationToken);
        var debtList = debts.ToList();

        // Filter by accessible projects
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null)
        {
            debtList = debtList.Where(d =>
                d.OriginatingTicket != null &&
                accessibleProjects.Contains(d.OriginatingTicket.ProjectId)).ToList();
        }

        return new DebtBacklogResponse
        {
            Items = debtList.Select(d => MapToDto(d, d.OriginatingTicket?.Key, d.ResolutionTicket?.Key)).ToList(),
            TotalCount = debtList.Count
        };
    }

    public async Task<IEnumerable<TechnicalDebtDto>> GetDebtByTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // Check project access via ticket
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return Enumerable.Empty<TechnicalDebtDto>();

        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);
        if (accessibleProjects != null && !accessibleProjects.Contains(ticket.ProjectId))
            return Enumerable.Empty<TechnicalDebtDto>();

        var debts = await _debtRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return debts.Select(d => MapToDto(d, d.OriginatingTicket?.Key, d.ResolutionTicket?.Key));
    }

    public async Task<DebtSummaryResponse> GetDebtSummaryAsync(CancellationToken cancellationToken = default)
    {
        var accessibleProjects = await _projectMemberService.GetAccessibleProjectIdsOrNullAsync(_userContext, cancellationToken);

        // If filtering is needed, get filtered summary via backlog
        if (accessibleProjects != null)
        {
            var debts = await _debtRepository.GetBacklogAsync(null, null, null, cancellationToken);
            var filteredDebts = debts.Where(d =>
                d.OriginatingTicket != null &&
                accessibleProjects.Contains(d.OriginatingTicket.ProjectId)).ToList();

            return new DebtSummaryResponse
            {
                TotalOpen = filteredDebts.Count(d => d.Status == DebtStatus.Open),
                TotalResolved = filteredDebts.Count(d => d.Status == DebtStatus.Resolved),
                ByCategory = filteredDebts.GroupBy(d => d.Category.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                BySeverity = filteredDebts.GroupBy(d => d.Severity.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        var summary = await _debtRepository.GetSummaryAsync(cancellationToken);

        return new DebtSummaryResponse
        {
            TotalOpen = summary.TotalOpen,
            TotalResolved = summary.TotalResolved,
            ByCategory = summary.ByCategory,
            BySeverity = summary.BySeverity
        };
    }

    private static TechnicalDebtDto MapToDto(TechnicalDebt debt, string? originatingTicketKey, string? resolutionTicketKey)
    {
        return new TechnicalDebtDto
        {
            Id = debt.Id,
            OriginatingTicketId = debt.OriginatingTicketId,
            OriginatingTicketKey = originatingTicketKey,
            ResolutionTicketId = debt.ResolutionTicketId,
            ResolutionTicketKey = resolutionTicketKey,
            Title = debt.Title,
            Description = debt.Description,
            Category = debt.Category.ToString(),
            Severity = debt.Severity.ToString(),
            Status = debt.Status.ToString(),
            Rationale = debt.Rationale,
            AffectedFiles = debt.AffectedFiles,
            SessionId = debt.SessionId,
            CreatedAt = debt.CreatedAt,
            ResolvedAt = debt.ResolvedAt
        };
    }
}
