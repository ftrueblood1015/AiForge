using System.Text.Json;
using AiForge.Application.DTOs.Plans;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IImplementationPlanService
{
    Task<ImplementationPlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImplementationPlanDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto?> GetCurrentByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto?> GetApprovedByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto> CreateAsync(Guid ticketId, CreateImplementationPlanRequest request, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto?> UpdateAsync(Guid id, UpdateImplementationPlanRequest request, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto?> ApproveAsync(Guid id, ApproveImplementationPlanRequest request, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto?> RejectAsync(Guid id, RejectImplementationPlanRequest request, CancellationToken cancellationToken = default);
    Task<ImplementationPlanDto?> SupersedeAsync(Guid id, SupersedeImplementationPlanRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ImplementationPlanService : IImplementationPlanService
{
    private readonly IImplementationPlanRepository _planRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ImplementationPlanService(
        IImplementationPlanRepository planRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImplementationPlanDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        return plan == null ? null : MapToDto(plan);
    }

    public async Task<IEnumerable<ImplementationPlanDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var plans = await _planRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return plans.Select(MapToDto);
    }

    public async Task<ImplementationPlanDto?> GetCurrentByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetCurrentByTicketIdAsync(ticketId, cancellationToken);
        return plan == null ? null : MapToDto(plan);
    }

    public async Task<ImplementationPlanDto?> GetApprovedByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetApprovedByTicketIdAsync(ticketId, cancellationToken);
        return plan == null ? null : MapToDto(plan);
    }

    public async Task<ImplementationPlanDto> CreateAsync(Guid ticketId, CreateImplementationPlanRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        var version = await _planRepository.GetNextVersionAsync(ticketId, cancellationToken);

        var plan = new ImplementationPlan
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Content = request.Content,
            Status = PlanStatus.Draft,
            Version = version,
            EstimatedEffort = request.EstimatedEffort,
            AffectedFiles = SerializeList(request.AffectedFiles),
            DependencyTicketIds = SerializeList(request.DependencyTicketIds),
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _planRepository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    public async Task<ImplementationPlanDto?> UpdateAsync(Guid id, UpdateImplementationPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null)
            return null;

        // Only draft plans can be edited
        if (plan.Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot update plan with status '{plan.Status}'. Only Draft plans can be updated.");

        if (request.Content != null)
            plan.Content = request.Content;

        if (request.EstimatedEffort != null)
            plan.EstimatedEffort = request.EstimatedEffort;

        if (request.AffectedFiles != null)
            plan.AffectedFiles = SerializeList(request.AffectedFiles);

        if (request.DependencyTicketIds != null)
            plan.DependencyTicketIds = SerializeList(request.DependencyTicketIds);

        await _planRepository.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    public async Task<ImplementationPlanDto?> ApproveAsync(Guid id, ApproveImplementationPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null)
            return null;

        // Only draft plans can be approved
        if (plan.Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot approve plan with status '{plan.Status}'. Only Draft plans can be approved.");

        plan.Status = PlanStatus.Approved;
        plan.ApprovedBy = request.ApprovedBy;
        plan.ApprovedAt = DateTime.UtcNow;

        await _planRepository.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    public async Task<ImplementationPlanDto?> RejectAsync(Guid id, RejectImplementationPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null)
            return null;

        // Only draft plans can be rejected
        if (plan.Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot reject plan with status '{plan.Status}'. Only Draft plans can be rejected.");

        plan.Status = PlanStatus.Rejected;
        plan.RejectedBy = request.RejectedBy;
        plan.RejectedAt = DateTime.UtcNow;
        plan.RejectionReason = request.RejectionReason;

        await _planRepository.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    public async Task<ImplementationPlanDto?> SupersedeAsync(Guid id, SupersedeImplementationPlanRequest request, CancellationToken cancellationToken = default)
    {
        var oldPlan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (oldPlan == null)
            return null;

        // Only approved plans can be superseded
        if (oldPlan.Status != PlanStatus.Approved)
            throw new InvalidOperationException($"Cannot supersede plan with status '{oldPlan.Status}'. Only Approved plans can be superseded.");

        var version = await _planRepository.GetNextVersionAsync(oldPlan.TicketId, cancellationToken);

        // Create the new plan
        var newPlan = new ImplementationPlan
        {
            Id = Guid.NewGuid(),
            TicketId = oldPlan.TicketId,
            Content = request.Content,
            Status = PlanStatus.Draft,
            Version = version,
            EstimatedEffort = request.EstimatedEffort,
            AffectedFiles = SerializeList(request.AffectedFiles),
            DependencyTicketIds = SerializeList(request.DependencyTicketIds),
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _planRepository.AddAsync(newPlan, cancellationToken);

        // Mark the old plan as superseded
        oldPlan.Status = PlanStatus.Superseded;
        oldPlan.SupersededById = newPlan.Id;
        oldPlan.SupersededAt = DateTime.UtcNow;

        await _planRepository.UpdateAsync(oldPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(newPlan);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null)
            return false;

        // Only draft plans can be deleted
        if (plan.Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot delete plan with status '{plan.Status}'. Only Draft plans can be deleted.");

        await _planRepository.DeleteAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    #region Helpers

    private ImplementationPlanDto MapToDto(ImplementationPlan plan)
    {
        return new ImplementationPlanDto
        {
            Id = plan.Id,
            TicketId = plan.TicketId,
            Content = plan.Content,
            Status = plan.Status.ToString(),
            Version = plan.Version,
            EstimatedEffort = plan.EstimatedEffort,
            AffectedFiles = DeserializeList(plan.AffectedFiles),
            DependencyTicketIds = DeserializeList(plan.DependencyTicketIds),
            CreatedBy = plan.CreatedBy,
            CreatedAt = plan.CreatedAt,
            ApprovedBy = plan.ApprovedBy,
            ApprovedAt = plan.ApprovedAt,
            SupersededById = plan.SupersededById,
            SupersededAt = plan.SupersededAt,
            RejectedBy = plan.RejectedBy,
            RejectedAt = plan.RejectedAt,
            RejectionReason = plan.RejectionReason
        };
    }

    private static string? SerializeList(List<string>? list)
    {
        if (list == null || list.Count == 0)
            return null;
        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    #endregion
}
