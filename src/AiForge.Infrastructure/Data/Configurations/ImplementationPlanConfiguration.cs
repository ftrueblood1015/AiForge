using AiForge.Domain.Entities;
using AiForge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ImplementationPlanConfiguration : IEntityTypeConfiguration<ImplementationPlan>
{
    public void Configure(EntityTypeBuilder<ImplementationPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Version)
            .IsRequired();

        builder.Property(p => p.EstimatedEffort)
            .HasMaxLength(50);

        builder.Property(p => p.AffectedFiles)
            .HasColumnType("nvarchar(max)"); // JSON array

        builder.Property(p => p.DependencyTicketIds)
            .HasColumnType("nvarchar(max)"); // JSON array

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(p => p.RejectedBy)
            .HasMaxLength(100);

        builder.Property(p => p.RejectionReason)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(p => p.TicketId);
        builder.HasIndex(p => new { p.TicketId, p.Version }).IsUnique();
        builder.HasIndex(p => p.Status);

        // Self-referencing relationship for superseded plans
        builder.HasOne(p => p.SupersededBy)
            .WithMany()
            .HasForeignKey(p => p.SupersededById)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Ticket
        builder.HasOne(p => p.Ticket)
            .WithMany(t => t.ImplementationPlans)
            .HasForeignKey(p => p.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
