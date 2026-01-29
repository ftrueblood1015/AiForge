using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SkillChainLinkExecutionConfiguration : IEntityTypeConfiguration<SkillChainLinkExecution>
{
    public void Configure(EntityTypeBuilder<SkillChainLinkExecution> builder)
    {
        builder.ToTable("SkillChainLinkExecutions");

        builder.HasKey(le => le.Id);

        builder.Property(le => le.AttemptNumber)
            .IsRequired();

        builder.Property(le => le.Outcome)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(le => le.Input)
            .HasColumnType("nvarchar(max)");

        builder.Property(le => le.Output)
            .HasColumnType("nvarchar(max)");

        builder.Property(le => le.ErrorDetails)
            .HasColumnType("nvarchar(max)");

        builder.Property(le => le.TransitionTaken)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(le => le.StartedAt)
            .IsRequired();

        builder.Property(le => le.ExecutedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(le => le.SkillChainExecutionId);
        builder.HasIndex(le => le.SkillChainLinkId);
        builder.HasIndex(le => le.Outcome);

        // Composite index for querying attempts on a specific link within an execution
        builder.HasIndex(le => new { le.SkillChainExecutionId, le.SkillChainLinkId, le.AttemptNumber });

        // Relationship to SkillChainExecution
        builder.HasOne(le => le.SkillChainExecution)
            .WithMany(e => e.LinkExecutions)
            .HasForeignKey(le => le.SkillChainExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to SkillChainLink
        builder.HasOne(le => le.SkillChainLink)
            .WithMany(l => l.LinkExecutions)
            .HasForeignKey(le => le.SkillChainLinkId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
