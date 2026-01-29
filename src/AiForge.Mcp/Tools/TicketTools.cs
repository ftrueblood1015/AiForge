using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.Tickets;
using AiForge.Application.Services;
using AiForge.Domain.Enums;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class TicketTools
{
    private readonly ITicketService _ticketService;
    private readonly ICommentService _commentService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public TicketTools(ITicketService ticketService, ICommentService commentService)
    {
        _ticketService = ticketService;
        _commentService = commentService;
    }

    [McpServerTool(Name = "search_tickets"), Description("Search tickets with optional filters")]
    public async Task<string> SearchTickets(
        [Description("Project ID (GUID) to filter by")] string? projectId = null,
        [Description("Status filter: ToDo, InProgress, InReview, Done")] string? status = null,
        [Description("Type filter: Task, Bug, Feature, Enhancement")] string? type = null,
        [Description("Priority filter: Low, Medium, High, Critical")] string? priority = null,
        [Description("Search text to filter by title or description")] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var request = new TicketSearchRequest
        {
            ProjectId = string.IsNullOrEmpty(projectId) ? null : Guid.Parse(projectId),
            Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<TicketStatus>(status, true),
            Type = string.IsNullOrEmpty(type) ? null : Enum.Parse<TicketType>(type, true),
            Priority = string.IsNullOrEmpty(priority) ? null : Enum.Parse<Priority>(priority, true),
            Search = search
        };

        var tickets = await _ticketService.SearchAsync(request, cancellationToken);
        var result = tickets.Select(t => new
        {
            t.Id,
            t.Key,
            t.Title,
            Status = t.Status.ToString(),
            Type = t.Type.ToString(),
            Priority = t.Priority.ToString(),
            t.ProjectKey
        });

        return JsonSerializer.Serialize(result, JsonOptions);
    }

    [McpServerTool(Name = "get_ticket"), Description("Get detailed ticket information by key (e.g., DEMO-1) or ID")]
    public async Task<string> GetTicket(
        [Description("Ticket key (e.g., DEMO-1) or ticket ID (GUID)")] string ticketKeyOrId,
        [Description("Return compact response (minimal fields only)")] bool compact = false,
        CancellationToken cancellationToken = default)
    {
        TicketDetailDto? ticket;

        if (Guid.TryParse(ticketKeyOrId, out var id))
        {
            ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        }
        else
        {
            ticket = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
        }

        if (ticket == null)
            return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

        if (compact)
        {
            return JsonSerializer.Serialize(new
            {
                ticket.Id,
                ticket.Key,
                ticket.Title,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                Type = ticket.Type.ToString()
            }, JsonOptions);
        }

        return JsonSerializer.Serialize(new
        {
            ticket.Id,
            ticket.Key,
            ticket.Title,
            ticket.Description,
            Status = ticket.Status.ToString(),
            Type = ticket.Type.ToString(),
            Priority = ticket.Priority.ToString(),
            ticket.ProjectKey,
            ticket.ParentTicketId,
            ticket.CurrentHandoffSummary,
            ticket.CommentCount,
            SubTicketCount = ticket.SubTickets?.Count ?? 0,
            ticket.CreatedAt,
            ticket.UpdatedAt
        }, JsonOptions);
    }

    [McpServerTool(Name = "create_ticket"), Description("Create a new ticket in a project")]
    public async Task<string> CreateTicket(
        [Description("Project key (e.g., DEMO, AIFORGE)")] string projectKey,
        [Description("Ticket title")] string title,
        [Description("Ticket description (markdown supported)")] string? description = null,
        [Description("Type: Task, Bug, Feature, Enhancement (default: Task)")] string type = "Task",
        [Description("Priority: Low, Medium, High, Critical (default: Medium)")] string priority = "Medium",
        [Description("Parent ticket ID for sub-tasks")] string? parentTicketId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateTicketRequest
            {
                ProjectKey = projectKey.ToUpperInvariant(),
                Title = title,
                Description = description,
                Type = Enum.Parse<TicketType>(type, true),
                Priority = Enum.Parse<Priority>(priority, true),
                ParentTicketId = string.IsNullOrEmpty(parentTicketId) ? null : Guid.Parse(parentTicketId)
            };

            var ticket = await _ticketService.CreateAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                ticket.Id,
                ticket.Key,
                ticket.Title,
                Status = ticket.Status.ToString(),
                Type = ticket.Type.ToString(),
                Priority = ticket.Priority.ToString(),
                message = $"Ticket {ticket.Key} created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_ticket"), Description("Update a ticket's fields")]
    public async Task<string> UpdateTicket(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("New title (optional)")] string? title = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New priority: Low, Medium, High, Critical (optional)")] string? priority = null,
        [Description("New type: Task, Bug, Feature, Enhancement (optional)")] string? type = null,
        [Description("Who made the change (e.g., session ID)")] string? changedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the ticket ID
            Guid ticketId;
            if (Guid.TryParse(ticketKeyOrId, out var id))
            {
                ticketId = id;
            }
            else
            {
                var existing = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
                if (existing == null)
                    return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });
                ticketId = existing.Id;
            }

            var request = new UpdateTicketRequest
            {
                Title = title,
                Description = description,
                Priority = string.IsNullOrEmpty(priority) ? null : Enum.Parse<Priority>(priority, true),
                Type = string.IsNullOrEmpty(type) ? null : Enum.Parse<TicketType>(type, true)
            };

            var ticket = await _ticketService.UpdateAsync(ticketId, request, changedBy, cancellationToken);
            if (ticket == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                ticket.Id,
                ticket.Key,
                ticket.Title,
                Status = ticket.Status.ToString(),
                Type = ticket.Type.ToString(),
                Priority = ticket.Priority.ToString(),
                message = $"Ticket {ticket.Key} updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "transition_ticket"), Description("Change a ticket's status")]
    public async Task<string> TransitionTicket(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("New status: ToDo, InProgress, InReview, Done")] string status,
        [Description("Optional comment about the transition")] string? comment = null,
        [Description("Who made the change (e.g., session ID)")] string? changedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the ticket ID
            Guid ticketId;
            if (Guid.TryParse(ticketKeyOrId, out var id))
            {
                ticketId = id;
            }
            else
            {
                var existing = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
                if (existing == null)
                    return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });
                ticketId = existing.Id;
            }

            var request = new TransitionTicketRequest
            {
                Status = Enum.Parse<TicketStatus>(status, true)
            };

            // Note: Comment would need to be added via a separate add_comment call

            var ticket = await _ticketService.TransitionAsync(ticketId, request, changedBy, cancellationToken);
            if (ticket == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                ticket.Id,
                ticket.Key,
                ticket.Title,
                Status = ticket.Status.ToString(),
                message = $"Ticket {ticket.Key} transitioned to {ticket.Status}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "add_comment"), Description("Add a comment to a ticket")]
    public async Task<string> AddComment(
        [Description("Ticket key (e.g., DEMO-1) or ID")] string ticketKeyOrId,
        [Description("Comment content (markdown supported)")] string content,
        [Description("Whether this comment is AI-generated")] bool isAiGenerated = true,
        [Description("Session ID for AI-generated comments")] string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the ticket ID
            Guid ticketId;
            if (Guid.TryParse(ticketKeyOrId, out var id))
            {
                ticketId = id;
            }
            else
            {
                var existing = await _ticketService.GetByKeyAsync(ticketKeyOrId.ToUpperInvariant(), cancellationToken);
                if (existing == null)
                    return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });
                ticketId = existing.Id;
            }

            var request = new AiForge.Application.DTOs.Comments.CreateCommentRequest
            {
                Content = content,
                IsAiGenerated = isAiGenerated,
                SessionId = sessionId
            };

            var comment = await _commentService.CreateAsync(ticketId, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                comment.Id,
                comment.TicketId,
                message = "Comment added successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    // Sub-ticket operations

    [McpServerTool(Name = "create_sub_ticket"), Description("Create a sub-ticket under a parent ticket for breaking down work")]
    public async Task<string> CreateSubTicket(
        [Description("Parent ticket key (e.g., DEMO-1) or ID")] string parentTicketKeyOrId,
        [Description("Sub-ticket title")] string title,
        [Description("Sub-ticket description (optional)")] string? description = null,
        [Description("Type: Task, Bug, Feature, Enhancement (default: Task)")] string type = "Task",
        [Description("Priority: Low, Medium, High, Critical (default: Medium)")] string priority = "Medium",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Resolve parent ticket
            var parentTicket = await ResolveTicketAsync(parentTicketKeyOrId, cancellationToken);
            if (parentTicket == null)
                return JsonSerializer.Serialize(new { error = $"Parent ticket '{parentTicketKeyOrId}' not found" });

            var request = new CreateSubTicketRequest
            {
                Title = title,
                Description = description,
                Type = Enum.Parse<TicketType>(type, true),
                Priority = Enum.Parse<Priority>(priority, true)
            };

            var ticket = await _ticketService.CreateSubTicketAsync(parentTicket.Id, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                Id = ticket.Id,
                Key = ticket.Key,
                Title = ticket.Title,
                ParentKey = parentTicket.Key,
                Status = ticket.Status.ToString(),
                Type = ticket.Type.ToString(),
                Priority = ticket.Priority.ToString(),
                message = $"Sub-ticket {ticket.Key} created under {parentTicket.Key}"
            }, JsonOptions);
        }
        catch (InvalidOperationException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "list_sub_tickets"), Description("List all sub-tickets for a parent ticket")]
    public async Task<string> ListSubTickets(
        [Description("Parent ticket key (e.g., DEMO-1) or ID")] string parentTicketKeyOrId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parentTicket = await ResolveTicketAsync(parentTicketKeyOrId, cancellationToken);
            if (parentTicket == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{parentTicketKeyOrId}' not found" });

            var subTickets = await _ticketService.GetSubTicketsAsync(parentTicket.Id, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                parentKey = parentTicket.Key,
                parentTitle = parentTicket.Title,
                subTicketCount = subTickets.Count(),
                subTickets = subTickets.Select(st => new
                {
                    st.Id,
                    st.Key,
                    st.Title,
                    Status = st.Status.ToString(),
                    Type = st.Type.ToString(),
                    Priority = st.Priority.ToString()
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "move_ticket"), Description("Move a ticket to a different parent or promote to top-level")]
    public async Task<string> MoveTicket(
        [Description("Ticket key (e.g., DEMO-1) or ID to move")] string ticketKeyOrId,
        [Description("New parent ticket key or ID (omit or null to promote to top-level)")] string? newParentKeyOrId = null,
        [Description("Who made the change (e.g., session ID)")] string? changedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ticket = await ResolveTicketAsync(ticketKeyOrId, cancellationToken);
            if (ticket == null)
                return JsonSerializer.Serialize(new { error = $"Ticket '{ticketKeyOrId}' not found" });

            Guid? newParentId = null;
            if (!string.IsNullOrEmpty(newParentKeyOrId))
            {
                var newParent = await ResolveTicketAsync(newParentKeyOrId, cancellationToken);
                if (newParent == null)
                    return JsonSerializer.Serialize(new { error = $"New parent ticket '{newParentKeyOrId}' not found" });
                newParentId = newParent.Id;
            }

            var request = new MoveSubTicketRequest { NewParentTicketId = newParentId };
            var result = await _ticketService.MoveTicketAsync(ticket.Id, request, changedBy, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                ticketKey = result!.Key,
                newParentId = newParentId,
                message = newParentId.HasValue
                    ? $"Ticket {result.Key} moved under new parent"
                    : $"Ticket {result.Key} promoted to top-level"
            }, JsonOptions);
        }
        catch (InvalidOperationException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    // Helper method to resolve ticket by key or ID
    private async Task<TicketDetailDto?> ResolveTicketAsync(string keyOrId, CancellationToken ct)
    {
        if (Guid.TryParse(keyOrId, out var id))
            return await _ticketService.GetByIdAsync(id, ct);

        return await _ticketService.GetByKeyAsync(keyOrId.ToUpperInvariant(), ct);
    }
}
