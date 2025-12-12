using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ProgressEntryConfiguration : IEntityTypeConfiguration<ProgressEntry>
{
    public void Configure(EntityTypeBuilder<ProgressEntry> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.SessionId)
            .HasMaxLength(100);

        builder.Property(p => p.Content)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(p => p.FilesAffected)
            .HasColumnType("nvarchar(max)"); // JSON array

        builder.Property(p => p.ErrorDetails)
            .HasMaxLength(10000);

        builder.HasIndex(p => p.TicketId);
        builder.HasIndex(p => p.SessionId);
        builder.HasIndex(p => p.CreatedAt);
    }
}
