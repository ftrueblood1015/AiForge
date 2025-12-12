using AutoMapper;
using AiForge.Application.DTOs.Comments;
using AiForge.Domain.Entities;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<CommentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CommentDto> CreateAsync(Guid ticketId, CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<CommentDto?> UpdateAsync(Guid id, UpdateCommentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CommentService(
        ICommentRepository commentRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CommentDto>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return _mapper.Map<IEnumerable<CommentDto>>(comments);
    }

    public async Task<CommentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        return comment == null ? null : _mapper.Map<CommentDto>(comment);
    }

    public async Task<CommentDto> CreateAsync(Guid ticketId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        var comment = _mapper.Map<Comment>(request);
        comment.TicketId = ticketId;

        await _commentRepository.AddAsync(comment, cancellationToken);

        // Update ticket's UpdatedAt
        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CommentDto>(comment);
    }

    public async Task<CommentDto?> UpdateAsync(Guid id, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
            return null;

        comment.Content = request.Content;

        await _commentRepository.UpdateAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CommentDto>(comment);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
            return false;

        await _commentRepository.DeleteAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
