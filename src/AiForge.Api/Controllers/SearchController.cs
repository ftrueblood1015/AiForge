using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Search across tickets and handoffs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string query,
        [FromQuery] SearchType? type,
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Search query is required" });

        var results = await _searchService.SearchAsync(query, type, projectId, cancellationToken);
        return Ok(results);
    }
}
