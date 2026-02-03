using AiForge.Domain.Entities;
using AiForge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Data;

public class AiForgeDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AiForgeDbContext(DbContextOptions<AiForgeDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<PlanningSession> PlanningSessions => Set<PlanningSession>();
    public DbSet<ReasoningLog> ReasoningLogs => Set<ReasoningLog>();
    public DbSet<ProgressEntry> ProgressEntries => Set<ProgressEntry>();
    public DbSet<HandoffDocument> HandoffDocuments => Set<HandoffDocument>();
    public DbSet<HandoffVersion> HandoffVersions => Set<HandoffVersion>();
    public DbSet<FileSnapshot> FileSnapshots => Set<FileSnapshot>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ApiKeyUsage> ApiKeyUsages => Set<ApiKeyUsage>();
    public DbSet<ImplementationPlan> ImplementationPlans => Set<ImplementationPlan>();
    public DbSet<EffortEstimation> EffortEstimations => Set<EffortEstimation>();
    public DbSet<FileChange> FileChanges => Set<FileChange>();
    public DbSet<TestLink> TestLinks => Set<TestLink>();
    public DbSet<TechnicalDebt> TechnicalDebts => Set<TechnicalDebt>();
    public DbSet<SessionMetrics> SessionMetrics => Set<SessionMetrics>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<ConfigurationSet> ConfigurationSets => Set<ConfigurationSet>();
    public DbSet<WorkQueue> WorkQueues => Set<WorkQueue>();
    public DbSet<WorkQueueItem> WorkQueueItems => Set<WorkQueueItem>();
    public DbSet<SkillChain> SkillChains => Set<SkillChain>();
    public DbSet<SkillChainLink> SkillChainLinks => Set<SkillChainLink>();
    public DbSet<SkillChainExecution> SkillChainExecutions => Set<SkillChainExecution>();
    public DbSet<SkillChainLinkExecution> SkillChainLinkExecutions => Set<SkillChainLinkExecution>();
    public DbSet<SessionState> SessionStates => Set<SessionState>();
    public DbSet<ExecutionCheckpoint> ExecutionCheckpoints => Set<ExecutionCheckpoint>();

    // Auth & Organization entities
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiForgeDbContext).Assembly);
    }
}
