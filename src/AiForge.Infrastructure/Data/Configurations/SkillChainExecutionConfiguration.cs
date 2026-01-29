using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SkillChainExecutionConfiguration : IEntityTypeConfiguration<SkillChainExecution>
{
    public void Configure(EntityTypeBuilder<SkillChainExecution> builder)
    {
        builder.ToTable("SkillChainExecutions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.InputValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ExecutionContext)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.TotalFailureCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.RequiresHumanIntervention)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.InterventionReason)
            .HasMaxLength(2000);

        builder.Property(e => e.StartedAt)
            .IsRequired();

        builder.Property(e => e.StartedBy)
            .HasMaxLength(200);

        builder.Property(e => e.CompletedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(e => e.SkillChainId);
        builder.HasIndex(e => e.TicketId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequiresHumanIntervention);
        builder.HasIndex(e => e.CurrentLinkId);

        // Relationship to SkillChain
        builder.HasOne(e => e.SkillChain)
            .WithMany(sc => sc.Executions)
            .HasForeignKey(e => e.SkillChainId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Ticket (optional)
        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.ChainExecutions)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship to CurrentLink (optional)
        builder.HasOne(e => e.CurrentLink)
            .WithMany()
            .HasForeignKey(e => e.CurrentLinkId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
