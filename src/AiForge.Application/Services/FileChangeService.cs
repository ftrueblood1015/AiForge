using AiForge.Application.DTOs.FileChange;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IFileChangeService
{
    Task<FileChangeDto> LogFileChangeAsync(Guid ticketId, LogFileChangeRequest request, CancellationToken cancellationToken = default);
    Task<FileHistoryResponse> GetFileHistoryAsync(string filePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileChangeDto>> GetTicketFilesAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HotFileDto>> GetHotFilesAsync(int limit = 10, CancellationToken cancellationToken = default);
}

public class FileChangeService : IFileChangeService
{
    private readonly IFileChangeRepository _fileChangeRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FileChangeService(
        IFileChangeRepository fileChangeRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _fileChangeRepository = fileChangeRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FileChangeDto> LogFileChangeAsync(Guid ticketId, LogFileChangeRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        var fileChange = new FileChange
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            FilePath = request.FilePath,
            ChangeType = Enum.Parse<FileChangeType>(request.ChangeType, ignoreCase: true),
            OldFilePath = request.OldFilePath,
            ChangeReason = request.ChangeReason,
            LinesAdded = request.LinesAdded,
            LinesRemoved = request.LinesRemoved,
            SessionId = request.SessionId,
            CreatedAt = DateTime.UtcNow
        };

        await _fileChangeRepository.AddAsync(fileChange, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(fileChange, ticket.Key);
    }

    public async Task<FileHistoryResponse> GetFileHistoryAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var changes = await _fileChangeRepository.GetByFilePathAsync(filePath, cancellationToken);
        var changeList = changes.ToList();

        return new FileHistoryResponse
        {
            FilePath = filePath,
            Changes = changeList.Select(c => MapToDto(c, c.Ticket?.Key)).ToList(),
            TotalTickets = changeList.Select(c => c.TicketId).Distinct().Count()
        };
    }

    public async Task<IEnumerable<FileChangeDto>> GetTicketFilesAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        var changes = await _fileChangeRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return changes.Select(c => MapToDto(c, ticket?.Key));
    }

    public async Task<IEnumerable<HotFileDto>> GetHotFilesAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var hotFiles = await _fileChangeRepository.GetHotFilesAsync(limit, cancellationToken);
        return hotFiles.Select(h => new HotFileDto
        {
            FilePath = h.FilePath,
            ChangeCount = h.ChangeCount,
            LastModified = h.LastModified
        });
    }

    private static FileChangeDto MapToDto(FileChange fileChange, string? ticketKey = null)
    {
        return new FileChangeDto
        {
            Id = fileChange.Id,
            TicketId = fileChange.TicketId,
            TicketKey = ticketKey,
            FilePath = fileChange.FilePath,
            ChangeType = fileChange.ChangeType.ToString(),
            OldFilePath = fileChange.OldFilePath,
            ChangeReason = fileChange.ChangeReason,
            LinesAdded = fileChange.LinesAdded,
            LinesRemoved = fileChange.LinesRemoved,
            SessionId = fileChange.SessionId,
            CreatedAt = fileChange.CreatedAt
        };
    }
}
