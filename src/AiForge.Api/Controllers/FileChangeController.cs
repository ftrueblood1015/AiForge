using AiForge.Application.DTOs.FileChange;
using AiForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiForge.Api.Controllers;

[ApiController]
public class FileChangeController : ControllerBase
{
    private readonly IFileChangeService _fileChangeService;

    public FileChangeController(IFileChangeService fileChangeService)
    {
        _fileChangeService = fileChangeService;
    }

    /// <summary>
    /// Log a file change for a ticket
    /// </summary>
    [HttpPost("api/tickets/{ticketId:guid}/file-changes")]
    [ProducesResponseType(typeof(FileChangeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileChangeDto>> LogFileChange(
        Guid ticketId,
        [FromBody] LogFileChangeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fileChange = await _fileChangeService.LogFileChangeAsync(ticketId, request, cancellationToken);
            return CreatedAtAction(nameof(GetTicketFileChanges), new { ticketId }, fileChange);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all file changes for a ticket
    /// </summary>
    [HttpGet("api/tickets/{ticketId:guid}/file-changes")]
    [ProducesResponseType(typeof(IEnumerable<FileChangeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileChangeDto>>> GetTicketFileChanges(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var changes = await _fileChangeService.GetTicketFilesAsync(ticketId, cancellationToken);
        return Ok(changes);
    }

    /// <summary>
    /// Get file history - all tickets that have modified a specific file
    /// </summary>
    [HttpGet("api/files/history")]
    [ProducesResponseType(typeof(FileHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FileHistoryResponse>> GetFileHistory(
        [FromQuery] string path,
        CancellationToken cancellationToken)
    {
        var history = await _fileChangeService.GetFileHistoryAsync(path, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Get most frequently modified files (hot files)
    /// </summary>
    [HttpGet("api/files/hot")]
    [ProducesResponseType(typeof(IEnumerable<HotFileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HotFileDto>>> GetHotFiles(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var hotFiles = await _fileChangeService.GetHotFilesAsync(limit, cancellationToken);
        return Ok(hotFiles);
    }
}
