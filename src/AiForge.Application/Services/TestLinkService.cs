using AiForge.Application.DTOs.TestLink;
using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface ITestLinkService
{
    Task<TestLinkDto> LinkTestAsync(Guid ticketId, LinkTestRequest request, CancellationToken cancellationToken = default);
    Task<TestLinkDto?> UpdateTestOutcomeAsync(Guid testLinkId, UpdateTestOutcomeRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestLinkDto>> GetTicketTestsAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<FileCoverageResponse> GetFileCoverageAsync(string filePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetCoverageGapsAsync(CancellationToken cancellationToken = default);
}

public class TestLinkService : ITestLinkService
{
    private readonly ITestLinkRepository _testLinkRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TestLinkService(
        ITestLinkRepository testLinkRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _testLinkRepository = testLinkRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TestLinkDto> LinkTestAsync(Guid ticketId, LinkTestRequest request, CancellationToken cancellationToken = default)
    {
        // Verify ticket exists
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket with ID '{ticketId}' not found");

        TestOutcome? outcome = null;
        if (!string.IsNullOrEmpty(request.Outcome))
        {
            outcome = Enum.Parse<TestOutcome>(request.Outcome, ignoreCase: true);
        }

        var testLink = new TestLink
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            TestFilePath = request.TestFilePath,
            TestName = request.TestName,
            TestedFunctionality = request.TestedFunctionality,
            LinkedFilePath = request.LinkedFilePath,
            Outcome = outcome,
            SessionId = request.SessionId,
            CreatedAt = DateTime.UtcNow,
            LastRunAt = outcome.HasValue ? DateTime.UtcNow : null
        };

        await _testLinkRepository.AddAsync(testLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(testLink, ticket.Key);
    }

    public async Task<TestLinkDto?> UpdateTestOutcomeAsync(Guid testLinkId, UpdateTestOutcomeRequest request, CancellationToken cancellationToken = default)
    {
        var testLink = await _testLinkRepository.GetByIdAsync(testLinkId, cancellationToken);
        if (testLink == null)
        {
            return null;
        }

        testLink.Outcome = Enum.Parse<TestOutcome>(request.Outcome, ignoreCase: true);
        testLink.LastRunAt = DateTime.UtcNow;

        await _testLinkRepository.UpdateAsync(testLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var ticket = await _ticketRepository.GetByIdAsync(testLink.TicketId, cancellationToken);
        return MapToDto(testLink, ticket?.Key);
    }

    public async Task<IEnumerable<TestLinkDto>> GetTicketTestsAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        var tests = await _testLinkRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return tests.Select(t => MapToDto(t, ticket?.Key));
    }

    public async Task<FileCoverageResponse> GetFileCoverageAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var tests = await _testLinkRepository.GetByLinkedFilePathAsync(filePath, cancellationToken);
        var testList = tests.ToList();

        return new FileCoverageResponse
        {
            FilePath = filePath,
            Tests = testList.Select(t => MapToDto(t, t.Ticket?.Key)).ToList(),
            TotalTests = testList.Count
        };
    }

    public async Task<IEnumerable<string>> GetCoverageGapsAsync(CancellationToken cancellationToken = default)
    {
        return await _testLinkRepository.GetFilesWithoutTestsAsync(cancellationToken);
    }

    private static TestLinkDto MapToDto(TestLink testLink, string? ticketKey = null)
    {
        return new TestLinkDto
        {
            Id = testLink.Id,
            TicketId = testLink.TicketId,
            TicketKey = ticketKey,
            TestFilePath = testLink.TestFilePath,
            TestName = testLink.TestName,
            TestedFunctionality = testLink.TestedFunctionality,
            Outcome = testLink.Outcome?.ToString(),
            LinkedFilePath = testLink.LinkedFilePath,
            SessionId = testLink.SessionId,
            CreatedAt = testLink.CreatedAt,
            LastRunAt = testLink.LastRunAt
        };
    }
}
