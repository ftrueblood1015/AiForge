using AiForge.Application.DTOs.TestLink;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
public class TestLinkController : ControllerBase
{
    private readonly ITestLinkService _testLinkService;

    public TestLinkController(ITestLinkService testLinkService)
    {
        _testLinkService = testLinkService;
    }

    /// <summary>
    /// Link a test to a ticket
    /// </summary>
    [HttpPost("api/tickets/{ticketId:guid}/tests")]
    [ProducesResponseType(typeof(TestLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestLinkDto>> LinkTest(
        Guid ticketId,
        [FromBody] LinkTestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var testLink = await _testLinkService.LinkTestAsync(ticketId, request, cancellationToken);
            return CreatedAtAction(nameof(GetTicketTests), new { ticketId }, testLink);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all tests linked to a ticket
    /// </summary>
    [HttpGet("api/tickets/{ticketId:guid}/tests")]
    [ProducesResponseType(typeof(IEnumerable<TestLinkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TestLinkDto>>> GetTicketTests(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var tests = await _testLinkService.GetTicketTestsAsync(ticketId, cancellationToken);
        return Ok(tests);
    }

    /// <summary>
    /// Update a test outcome
    /// </summary>
    [HttpPatch("api/tests/{id:guid}/outcome")]
    [ProducesResponseType(typeof(TestLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestLinkDto>> UpdateTestOutcome(
        Guid id,
        [FromBody] UpdateTestOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        var testLink = await _testLinkService.UpdateTestOutcomeAsync(id, request, cancellationToken);
        if (testLink == null)
            return NotFound(new { error = $"Test link with ID '{id}' not found" });

        return Ok(testLink);
    }

    /// <summary>
    /// Get test coverage for a specific file
    /// </summary>
    [HttpGet("api/files/coverage")]
    [ProducesResponseType(typeof(FileCoverageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FileCoverageResponse>> GetFileCoverage(
        [FromQuery] string path,
        CancellationToken cancellationToken)
    {
        var coverage = await _testLinkService.GetFileCoverageAsync(path, cancellationToken);
        return Ok(coverage);
    }

    /// <summary>
    /// Get files without test coverage (coverage gaps)
    /// </summary>
    [HttpGet("api/files/coverage-gaps")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCoverageGaps(
        CancellationToken cancellationToken)
    {
        var gaps = await _testLinkService.GetCoverageGapsAsync(cancellationToken);
        return Ok(gaps);
    }
}
