using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Data;

public class AiForgeDbContext : DbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiForgeDbContext).Assembly);
    }
}
