using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class FileChangeConfiguration : IEntityTypeConfiguration<FileChange>
{
    public void Configure(EntityTypeBuilder<FileChange> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.ChangeType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(f => f.OldFilePath)
            .HasMaxLength(1000);

        builder.Property(f => f.ChangeReason)
            .HasMaxLength(500);

        builder.Property(f => f.SessionId)
            .HasMaxLength(100);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(f => f.TicketId);
        builder.HasIndex(f => f.FilePath);
        builder.HasIndex(f => new { f.TicketId, f.FilePath });

        // Relationship to Ticket
        builder.HasOne(f => f.Ticket)
            .WithMany(t => t.FileChanges)
            .HasForeignKey(f => f.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
