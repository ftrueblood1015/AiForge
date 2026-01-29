using System.ComponentModel;
using System.Text.Json;
using AiForge.Application.DTOs.SkillChains;
using AiForge.Application.Services;
using ModelContextProtocol.Server;

namespace AiForge.Mcp.Tools;

[McpServerToolType]
public class SkillChainTools
{
    private readonly ISkillChainService _chainService;
    private readonly ISkillChainExecutionService _executionService;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SkillChainTools(ISkillChainService chainService, ISkillChainExecutionService executionService)
    {
        _chainService = chainService;
        _executionService = executionService;
    }

    #region Chain Management

    [McpServerTool(Name = "create_skill_chain"), Description("Create a new skill chain workflow definition")]
    public async Task<string> CreateSkillChain(
        [Description("Unique chain key (e.g., 'feature-development', 'bug-fix-workflow')")] string chainKey,
        [Description("Display name for the chain")] string name,
        [Description("Organization ID (GUID) for org-level chain")] string? organizationId = null,
        [Description("Project ID (GUID) for project-level chain")] string? projectId = null,
        [Description("Chain description")] string? description = null,
        [Description("JSON schema for required inputs")] string? inputSchema = null,
        [Description("Max total failures before human intervention (default: 5)")] int maxTotalFailures = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateSkillChainRequest
            {
                ChainKey = chainKey,
                Name = name,
                Description = description,
                InputSchema = inputSchema,
                MaxTotalFailures = maxTotalFailures,
                OrganizationId = string.IsNullOrEmpty(organizationId) ? null : Guid.Parse(organizationId),
                ProjectId = string.IsNullOrEmpty(projectId) ? null : Guid.Parse(projectId)
            };

            var chain = await _chainService.CreateAsync(request, "mcp-session", cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                chain.Id,
                chain.ChainKey,
                chain.Name,
                chain.Scope,
                chain.IsPublished,
                message = $"Skill chain '{chain.Name}' created successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_skill_chain"), Description("Get a skill chain by ID or key")]
    public async Task<string> GetSkillChain(
        [Description("Skill chain ID (GUID) or chain key")] string chainIdOrKey,
        [Description("Organization ID (GUID) - required when looking up by key")] string? organizationId = null,
        [Description("Project ID (GUID) - optional scope for key lookup")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            SkillChainDto? chain;

            if (Guid.TryParse(chainIdOrKey, out var id))
            {
                chain = await _chainService.GetByIdAsync(id, cancellationToken);
            }
            else
            {
                if (string.IsNullOrEmpty(organizationId))
                    return JsonSerializer.Serialize(new { error = "organizationId is required when looking up by key" });

                var orgId = Guid.Parse(organizationId);
                var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
                chain = await _chainService.GetByKeyAsync(chainIdOrKey, orgId, projId, cancellationToken);
            }

            if (chain == null)
                return JsonSerializer.Serialize(new { error = $"Skill chain '{chainIdOrKey}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                chain = new
                {
                    chain.Id,
                    chain.ChainKey,
                    chain.Name,
                    chain.Description,
                    chain.InputSchema,
                    chain.MaxTotalFailures,
                    chain.OrganizationId,
                    chain.ProjectId,
                    chain.Scope,
                    chain.IsPublished,
                    chain.CreatedAt,
                    chain.UpdatedAt,
                    Links = chain.Links.Select(l => new
                    {
                        l.Id,
                        l.Position,
                        l.Name,
                        l.SkillId,
                        l.SkillName,
                        l.AgentId,
                        l.AgentName,
                        l.MaxRetries,
                        l.OnSuccessTransition,
                        l.OnSuccessTargetLinkId,
                        l.OnFailureTransition,
                        l.OnFailureTargetLinkId
                    })
                }
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "list_skill_chains"), Description("List available skill chains with optional filters")]
    public async Task<string> ListSkillChains(
        [Description("Organization ID (GUID) to filter by")] string? organizationId = null,
        [Description("Project ID (GUID) to filter by")] string? projectId = null,
        [Description("Only return published chains")] bool? publishedOnly = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orgId = string.IsNullOrEmpty(organizationId) ? null : (Guid?)Guid.Parse(organizationId);
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);

            var chains = await _chainService.GetChainsAsync(orgId, projId, publishedOnly, cancellationToken);
            var chainList = chains.ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                totalCount = chainList.Count,
                chains = chainList.Select(c => new
                {
                    c.Id,
                    c.ChainKey,
                    c.Name,
                    c.Description,
                    c.Scope,
                    c.IsPublished,
                    c.LinkCount,
                    c.ExecutionCount,
                    c.CreatedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_skill_chain"), Description("Update an existing skill chain")]
    public async Task<string> UpdateSkillChain(
        [Description("Skill chain ID (GUID)")] string chainId,
        [Description("New name (optional)")] string? name = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New input schema (optional)")] string? inputSchema = null,
        [Description("New max total failures (optional)")] int? maxTotalFailures = null,
        [Description("Publish or unpublish the chain (optional)")] bool? isPublished = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(chainId);
            var request = new UpdateSkillChainRequest
            {
                Name = name,
                Description = description,
                InputSchema = inputSchema,
                MaxTotalFailures = maxTotalFailures,
                IsPublished = isPublished
            };

            var chain = await _chainService.UpdateAsync(id, request, "mcp-session", cancellationToken);
            if (chain == null)
                return JsonSerializer.Serialize(new { error = $"Skill chain '{chainId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                chain.Id,
                chain.ChainKey,
                chain.Name,
                chain.IsPublished,
                message = $"Skill chain '{chain.Name}' updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "delete_skill_chain"), Description("Delete a skill chain")]
    public async Task<string> DeleteSkillChain(
        [Description("Skill chain ID (GUID)")] string chainId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(chainId);
            var deleted = await _chainService.DeleteAsync(id, cancellationToken);

            if (!deleted)
                return JsonSerializer.Serialize(new { error = $"Skill chain '{chainId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Skill chain '{chainId}' deleted successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "publish_skill_chain"), Description("Publish a skill chain to make it available for execution")]
    public async Task<string> PublishSkillChain(
        [Description("Skill chain ID (GUID)")] string chainId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(chainId);
            var chain = await _chainService.PublishAsync(id, "mcp-session", cancellationToken);

            if (chain == null)
                return JsonSerializer.Serialize(new { error = $"Skill chain '{chainId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                chain.Id,
                chain.ChainKey,
                chain.Name,
                chain.IsPublished,
                message = $"Skill chain '{chain.Name}' published successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "unpublish_skill_chain"), Description("Unpublish a skill chain to hide it from execution")]
    public async Task<string> UnpublishSkillChain(
        [Description("Skill chain ID (GUID)")] string chainId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(chainId);
            var chain = await _chainService.UnpublishAsync(id, "mcp-session", cancellationToken);

            if (chain == null)
                return JsonSerializer.Serialize(new { error = $"Skill chain '{chainId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                chain.Id,
                chain.ChainKey,
                chain.Name,
                chain.IsPublished,
                message = $"Skill chain '{chain.Name}' unpublished successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Link Management

    [McpServerTool(Name = "add_chain_link"), Description("Add a link (step) to a skill chain")]
    public async Task<string> AddChainLink(
        [Description("Skill chain ID (GUID)")] string chainId,
        [Description("Link name (e.g., 'Plan Feature', 'Review Code')")] string name,
        [Description("Skill ID (GUID) to execute at this step")] string skillId,
        [Description("Link description")] string? description = null,
        [Description("Agent ID (GUID) to use for execution (optional)")] string? agentId = null,
        [Description("Max retries for this link (default: 3)")] int maxRetries = 3,
        [Description("Transition on success: NextLink, GoToLink, Complete (default: NextLink)")] string onSuccessTransition = "NextLink",
        [Description("Target link ID for GoToLink on success")] string? onSuccessTargetLinkId = null,
        [Description("Transition on failure: Retry, GoToLink, Escalate (default: Retry)")] string onFailureTransition = "Retry",
        [Description("Target link ID for GoToLink on failure")] string? onFailureTargetLinkId = null,
        [Description("Position in chain (optional, appends to end if not specified)")] int? position = null,
        [Description("Additional link configuration (JSON)")] string? linkConfig = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(chainId);
            var request = new CreateSkillChainLinkRequest
            {
                Name = name,
                Description = description,
                SkillId = Guid.Parse(skillId),
                AgentId = string.IsNullOrEmpty(agentId) ? null : Guid.Parse(agentId),
                MaxRetries = maxRetries,
                OnSuccessTransition = onSuccessTransition,
                OnSuccessTargetLinkId = string.IsNullOrEmpty(onSuccessTargetLinkId) ? null : Guid.Parse(onSuccessTargetLinkId),
                OnFailureTransition = onFailureTransition,
                OnFailureTargetLinkId = string.IsNullOrEmpty(onFailureTargetLinkId) ? null : Guid.Parse(onFailureTargetLinkId),
                Position = position,
                LinkConfig = linkConfig
            };

            var link = await _chainService.AddLinkAsync(id, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                link = new
                {
                    link.Id,
                    link.Position,
                    link.Name,
                    link.SkillId,
                    link.SkillName,
                    link.AgentId,
                    link.AgentName,
                    link.MaxRetries,
                    link.OnSuccessTransition,
                    link.OnFailureTransition
                },
                message = $"Link '{link.Name}' added at position {link.Position}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "update_chain_link"), Description("Update a chain link's configuration")]
    public async Task<string> UpdateChainLink(
        [Description("Chain link ID (GUID)")] string linkId,
        [Description("New name (optional)")] string? name = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New skill ID (optional)")] string? skillId = null,
        [Description("New agent ID (optional)")] string? agentId = null,
        [Description("New max retries (optional)")] int? maxRetries = null,
        [Description("New success transition (optional)")] string? onSuccessTransition = null,
        [Description("New success target link ID (optional)")] string? onSuccessTargetLinkId = null,
        [Description("New failure transition (optional)")] string? onFailureTransition = null,
        [Description("New failure target link ID (optional)")] string? onFailureTargetLinkId = null,
        [Description("New link config (optional)")] string? linkConfig = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(linkId);
            var request = new UpdateSkillChainLinkRequest
            {
                Name = name,
                Description = description,
                SkillId = string.IsNullOrEmpty(skillId) ? null : Guid.Parse(skillId),
                AgentId = string.IsNullOrEmpty(agentId) ? null : Guid.Parse(agentId),
                MaxRetries = maxRetries,
                OnSuccessTransition = onSuccessTransition,
                OnSuccessTargetLinkId = string.IsNullOrEmpty(onSuccessTargetLinkId) ? null : Guid.Parse(onSuccessTargetLinkId),
                OnFailureTransition = onFailureTransition,
                OnFailureTargetLinkId = string.IsNullOrEmpty(onFailureTargetLinkId) ? null : Guid.Parse(onFailureTargetLinkId),
                LinkConfig = linkConfig
            };

            var link = await _chainService.UpdateLinkAsync(id, request, cancellationToken);
            if (link == null)
                return JsonSerializer.Serialize(new { error = $"Chain link '{linkId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                link = new
                {
                    link.Id,
                    link.Position,
                    link.Name,
                    link.SkillId,
                    link.SkillName,
                    link.MaxRetries,
                    link.OnSuccessTransition,
                    link.OnFailureTransition
                },
                message = $"Link '{link.Name}' updated successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "remove_chain_link"), Description("Remove a link from a skill chain")]
    public async Task<string> RemoveChainLink(
        [Description("Chain link ID (GUID)")] string linkId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(linkId);
            var removed = await _chainService.RemoveLinkAsync(id, cancellationToken);

            if (!removed)
                return JsonSerializer.Serialize(new { error = $"Chain link '{linkId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Chain link '{linkId}' removed successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "reorder_chain_links"), Description("Reorder links in a skill chain")]
    public async Task<string> ReorderChainLinks(
        [Description("Skill chain ID (GUID)")] string chainId,
        [Description("Comma-separated list of link IDs in desired order")] string linkIdsInOrder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(chainId);
            var linkIds = linkIdsInOrder.Split(',')
                .Select(s => Guid.Parse(s.Trim()))
                .ToList();

            var reordered = await _chainService.ReorderLinksAsync(id, linkIds, cancellationToken);

            if (!reordered)
                return JsonSerializer.Serialize(new { error = $"Skill chain '{chainId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Links reordered successfully"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Execution Management

    [McpServerTool(Name = "start_chain_execution"), Description("Start executing a skill chain")]
    public async Task<string> StartChainExecution(
        [Description("Skill chain ID (GUID)")] string skillChainId,
        [Description("Ticket ID (GUID) to associate with execution (optional)")] string? ticketId = null,
        [Description("Input values as JSON object matching the chain's InputSchema")] string? inputValues = null,
        [Description("Who is starting the execution")] string? startedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new StartChainExecutionRequest
            {
                SkillChainId = Guid.Parse(skillChainId),
                TicketId = string.IsNullOrEmpty(ticketId) ? null : Guid.Parse(ticketId),
                InputValues = inputValues,
                StartedBy = startedBy ?? "mcp-session"
            };

            var execution = await _executionService.StartExecutionAsync(request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.SkillChainId,
                    execution.ChainName,
                    execution.Status,
                    execution.CurrentLinkId,
                    execution.CurrentLinkName,
                    execution.StartedAt
                },
                message = $"Chain execution started - now at '{execution.CurrentLinkName}'"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "get_chain_execution"), Description("Get current state of a chain execution")]
    public async Task<string> GetChainExecution(
        [Description("Execution ID (GUID)")] string executionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var execution = await _executionService.GetExecutionAsync(id, cancellationToken);

            if (execution == null)
                return JsonSerializer.Serialize(new { error = $"Execution '{executionId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.SkillChainId,
                    execution.ChainKey,
                    execution.ChainName,
                    execution.TicketId,
                    execution.TicketKey,
                    execution.Status,
                    execution.CurrentLinkId,
                    execution.CurrentLinkName,
                    execution.CurrentLinkPosition,
                    execution.InputValues,
                    execution.TotalFailureCount,
                    execution.RequiresHumanIntervention,
                    execution.InterventionReason,
                    execution.StartedAt,
                    execution.CompletedAt,
                    LinkExecutions = execution.LinkExecutions.Select(le => new
                    {
                        le.Id,
                        le.LinkName,
                        le.LinkPosition,
                        le.AttemptNumber,
                        le.Outcome,
                        le.TransitionTaken,
                        le.ErrorDetails,
                        le.StartedAt,
                        le.CompletedAt
                    })
                }
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "list_chain_executions"), Description("List chain executions with optional filters")]
    public async Task<string> ListChainExecutions(
        [Description("Skill chain ID (GUID) to filter by")] string? chainId = null,
        [Description("Ticket ID (GUID) to filter by")] string? ticketId = null,
        [Description("Status filter: Pending, Running, Paused, Completed, Failed, Cancelled")] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chainGuid = string.IsNullOrEmpty(chainId) ? null : (Guid?)Guid.Parse(chainId);
            var ticketGuid = string.IsNullOrEmpty(ticketId) ? null : (Guid?)Guid.Parse(ticketId);

            var executions = await _executionService.GetExecutionsAsync(chainGuid, ticketGuid, status, cancellationToken);
            var executionList = executions.ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                totalCount = executionList.Count,
                executions = executionList.Select(e => new
                {
                    e.Id,
                    e.SkillChainId,
                    e.ChainName,
                    e.TicketId,
                    e.TicketKey,
                    e.Status,
                    e.CurrentLinkName,
                    e.TotalFailureCount,
                    e.RequiresHumanIntervention,
                    e.StartedAt,
                    e.CompletedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "record_link_outcome"), Description("Record the outcome of the current chain link execution")]
    public async Task<string> RecordLinkOutcome(
        [Description("Execution ID (GUID)")] string executionId,
        [Description("Link ID (GUID) that was executed")] string linkId,
        [Description("Outcome: Success, Failure")] string outcome,
        [Description("Output from execution (JSON)")] string? output = null,
        [Description("Error details if failed")] string? errorDetails = null,
        [Description("Who executed the link")] string? executedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var request = new RecordLinkOutcomeRequest
            {
                LinkId = Guid.Parse(linkId),
                Outcome = outcome,
                Output = output,
                ErrorDetails = errorDetails,
                ExecutedBy = executedBy ?? "mcp-session"
            };

            var linkExec = await _executionService.RecordLinkOutcomeAsync(id, request, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = true,
                linkExecution = new
                {
                    linkExec.Id,
                    linkExec.LinkName,
                    linkExec.AttemptNumber,
                    linkExec.Outcome,
                    linkExec.CompletedAt
                },
                message = $"Outcome '{outcome}' recorded for link '{linkExec.LinkName}'"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "advance_chain"), Description("Advance chain execution to the next appropriate link based on last outcome")]
    public async Task<string> AdvanceChain(
        [Description("Execution ID (GUID)")] string executionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var execution = await _executionService.AdvanceExecutionAsync(id, cancellationToken);

            if (execution == null)
                return JsonSerializer.Serialize(new { error = $"Execution '{executionId}' not found" });

            var message = execution.Status == "Completed"
                ? "Chain execution completed"
                : execution.Status == "Paused"
                    ? $"Execution paused - {execution.InterventionReason}"
                    : $"Advanced to link '{execution.CurrentLinkName}'";

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.Status,
                    execution.CurrentLinkId,
                    execution.CurrentLinkName,
                    execution.CurrentLinkPosition,
                    execution.TotalFailureCount,
                    execution.RequiresHumanIntervention,
                    execution.InterventionReason,
                    execution.CompletedAt
                },
                message
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "pause_chain_execution"), Description("Pause a running chain execution")]
    public async Task<string> PauseChainExecution(
        [Description("Execution ID (GUID)")] string executionId,
        [Description("Reason for pausing")] string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var execution = await _executionService.PauseExecutionAsync(id, reason, "mcp-session", cancellationToken);

            if (execution == null)
                return JsonSerializer.Serialize(new { error = $"Execution '{executionId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.Status,
                    execution.CurrentLinkName,
                    execution.InterventionReason
                },
                message = $"Execution paused: {reason}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "resume_chain_execution"), Description("Resume a paused chain execution")]
    public async Task<string> ResumeChainExecution(
        [Description("Execution ID (GUID)")] string executionId,
        [Description("Additional context to merge (JSON)")] string? additionalContext = null,
        [Description("Who is resuming the execution")] string? resumedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var request = new ResumeExecutionRequest
            {
                ResumedBy = resumedBy ?? "mcp-session",
                AdditionalContext = additionalContext
            };

            var execution = await _executionService.ResumeExecutionAsync(id, request, cancellationToken);

            if (execution == null)
                return JsonSerializer.Serialize(new { error = $"Execution '{executionId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.Status,
                    execution.CurrentLinkName,
                    execution.RequiresHumanIntervention
                },
                message = $"Execution resumed - continuing at '{execution.CurrentLinkName}'"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "cancel_chain_execution"), Description("Cancel a chain execution")]
    public async Task<string> CancelChainExecution(
        [Description("Execution ID (GUID)")] string executionId,
        [Description("Reason for cancellation")] string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var execution = await _executionService.CancelExecutionAsync(id, reason, "mcp-session", cancellationToken);

            if (execution == null)
                return JsonSerializer.Serialize(new { error = $"Execution '{executionId}' not found" });

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.Status,
                    execution.CompletedAt
                },
                message = $"Execution cancelled: {reason}"
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Human Intervention

    [McpServerTool(Name = "get_pending_interventions"), Description("Get chain executions requiring human intervention")]
    public async Task<string> GetPendingInterventions(
        [Description("Project ID (GUID) to filter by")] string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projId = string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
            var executions = await _executionService.GetPendingInterventionsAsync(projId, cancellationToken);
            var executionList = executions.ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                totalCount = executionList.Count,
                interventions = executionList.Select(e => new
                {
                    e.Id,
                    e.SkillChainId,
                    e.ChainName,
                    e.TicketId,
                    e.TicketKey,
                    e.Status,
                    e.CurrentLinkName,
                    e.TotalFailureCount,
                    e.RequiresHumanIntervention,
                    e.StartedAt
                })
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "resolve_intervention"), Description("Resolve a human intervention and continue execution")]
    public async Task<string> ResolveIntervention(
        [Description("Execution ID (GUID)")] string executionId,
        [Description("Description of the resolution")] string resolution,
        [Description("Next action: Retry, GoToLink, Complete, Escalate")] string nextAction = "Retry",
        [Description("Target link ID if nextAction is GoToLink")] string? targetLinkId = null,
        [Description("Who is resolving the intervention")] string? resolvedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.Parse(executionId);
            var request = new ResolveInterventionRequest
            {
                Resolution = resolution,
                NextAction = nextAction,
                TargetLinkId = string.IsNullOrEmpty(targetLinkId) ? null : Guid.Parse(targetLinkId),
                ResolvedBy = resolvedBy ?? "mcp-session"
            };

            var execution = await _executionService.ResolveInterventionAsync(id, request, cancellationToken);

            if (execution == null)
                return JsonSerializer.Serialize(new { error = $"Execution '{executionId}' not found" });

            var message = execution.Status == "Completed"
                ? "Intervention resolved - chain completed"
                : $"Intervention resolved - continuing at '{execution.CurrentLinkName}'";

            return JsonSerializer.Serialize(new
            {
                success = true,
                execution = new
                {
                    execution.Id,
                    execution.Status,
                    execution.CurrentLinkId,
                    execution.CurrentLinkName,
                    execution.RequiresHumanIntervention,
                    execution.CompletedAt
                },
                message
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion
}
