using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class WorkQueueConfiguration : IEntityTypeConfiguration<WorkQueue>
{
    public void Configure(EntityTypeBuilder<WorkQueue> builder)
    {
        builder.ToTable("WorkQueues");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.Description)
            .HasMaxLength(2000);

        builder.Property(q => q.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // JSON column for ContextHelper
        builder.OwnsOne(q => q.Context, contextBuilder =>
        {
            contextBuilder.ToJson("Context");

            contextBuilder.Property(c => c.CurrentFocus)
                .HasMaxLength(500);
        });

        // Checkout tracking
        builder.Property(q => q.CheckedOutBy)
            .HasMaxLength(200);

        builder.Property(q => q.CreatedBy)
            .HasMaxLength(200);

        builder.Property(q => q.UpdatedBy)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(q => q.Project)
            .WithMany()
            .HasForeignKey(q => q.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.ImplementationPlan)
            .WithMany()
            .HasForeignKey(q => q.ImplementationPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(q => q.ProjectId);
        builder.HasIndex(q => q.Status);
        builder.HasIndex(q => q.CheckedOutBy);
    }
}
