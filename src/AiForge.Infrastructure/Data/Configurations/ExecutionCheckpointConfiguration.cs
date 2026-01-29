using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ExecutionCheckpointConfiguration : IEntityTypeConfiguration<ExecutionCheckpoint>
{
    public void Configure(EntityTypeBuilder<ExecutionCheckpoint> builder)
    {
        builder.ToTable("ExecutionCheckpoints");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Position)
            .IsRequired();

        builder.Property(e => e.CheckpointData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.ExecutionId);
        builder.HasIndex(e => new { e.ExecutionId, e.Position });

        // Relationship to SkillChainExecution (cascade delete when execution is deleted)
        builder.HasOne(e => e.Execution)
            .WithMany(ex => ex.Checkpoints)
            .HasForeignKey(e => e.ExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to SkillChainLink (no action - links are part of chain definition)
        builder.HasOne(e => e.Link)
            .WithMany()
            .HasForeignKey(e => e.LinkId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
