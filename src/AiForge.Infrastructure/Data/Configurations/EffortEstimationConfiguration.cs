using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class EffortEstimationConfiguration : IEntityTypeConfiguration<EffortEstimation>
{
    public void Configure(EntityTypeBuilder<EffortEstimation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Complexity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.EstimatedEffort)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.ConfidencePercent)
            .IsRequired();

        builder.Property(e => e.EstimationReasoning)
            .HasMaxLength(2000);

        builder.Property(e => e.Assumptions)
            .HasMaxLength(1000);

        builder.Property(e => e.ActualEffort)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.VarianceNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.RevisionReason)
            .HasMaxLength(500);

        builder.Property(e => e.SessionId)
            .HasMaxLength(100);

        builder.Property(e => e.IsLatest)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.TicketId);
        builder.HasIndex(e => new { e.TicketId, e.IsLatest });
        builder.HasIndex(e => new { e.TicketId, e.Version }).IsUnique();

        // Relationship to Ticket
        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.EffortEstimations)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
