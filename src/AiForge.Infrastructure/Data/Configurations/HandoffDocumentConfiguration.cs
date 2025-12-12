using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class HandoffDocumentConfiguration : IEntityTypeConfiguration<HandoffDocument>
{
    public void Configure(EntityTypeBuilder<HandoffDocument> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.SessionId)
            .HasMaxLength(100);

        builder.Property(h => h.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(h => h.Summary)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(h => h.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)"); // Full markdown content

        builder.Property(h => h.StructuredContext)
            .HasColumnType("nvarchar(max)"); // JSON blob

        // Self-referencing for superseded handoffs
        builder.HasOne(h => h.SupersededBy)
            .WithMany()
            .HasForeignKey(h => h.SupersededById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.Versions)
            .WithOne(v => v.Handoff)
            .HasForeignKey(v => v.HandoffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.FileSnapshots)
            .WithOne(f => f.Handoff)
            .HasForeignKey(f => f.HandoffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.TicketId);
        builder.HasIndex(h => h.SessionId);
        builder.HasIndex(h => h.IsActive);
        builder.HasIndex(h => h.CreatedAt);
    }
}
