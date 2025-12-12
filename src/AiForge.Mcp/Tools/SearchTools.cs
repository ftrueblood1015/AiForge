using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class SearchTools
{
    private readonly ISearchService _searchService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SearchTools(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [McpServerTool(Name = "search"), Description("Search across tickets and handoffs")]
    public async Task<string> Search(
        [Description("Search query text")] string query,
        [Description("Type to search: All, Tickets, Handoffs (default: All)")] string type = "All",
        [Description("Project ID (GUID) to filter by (optional)")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchType = Enum.Parse<SearchType>(type, true);
            var projId = string.IsNullOrEmpty(projectId) ? (Guid?)null : Guid.Parse(projectId);

            var results = await _searchService.SearchAsync(query, searchType, projId, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                totalCount = results.TotalCount,
                tickets = results.Tickets.Select(t => new
                {
                    t.Id,
                    t.Key,
                    t.Title,
                    Status = t.Status.ToString(),
                    Type = t.Type.ToString(),
                    Priority = t.Priority.ToString(),
                    t.ProjectKey
                }),
                handoffs = results.Handoffs.Select(h => new
                {
                    h.Id,
                    h.TicketId,
                    h.Title,
                    Type = h.Type.ToString(),
                    h.Summary,
                    h.IsActive,
                    h.CreatedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
