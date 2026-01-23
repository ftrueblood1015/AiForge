using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class WorkQueueItemConfiguration : IEntityTypeConfiguration<WorkQueueItem>
{
    public void Configure(EntityTypeBuilder<WorkQueueItem> builder)
    {
        builder.ToTable("WorkQueueItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.WorkItemType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.Property(i => i.AddedBy)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(i => i.WorkQueue)
            .WithMany(q => q.Items)
            .HasForeignKey(i => i.WorkQueueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => new { i.WorkQueueId, i.Position });
        builder.HasIndex(i => new { i.WorkItemId, i.WorkItemType });

        // Unique constraint: item can only be in queue once
        builder.HasIndex(i => new { i.WorkQueueId, i.WorkItemId, i.WorkItemType })
            .IsUnique();
    }
}
