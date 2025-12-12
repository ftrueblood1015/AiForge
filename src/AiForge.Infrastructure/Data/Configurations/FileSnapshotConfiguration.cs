using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class FileSnapshotConfiguration : IEntityTypeConfiguration<FileSnapshot>
{
    public void Configure(EntityTypeBuilder<FileSnapshot> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.ContentBefore)
            .HasColumnType("nvarchar(max)");

        builder.Property(f => f.ContentAfter)
            .HasColumnType("nvarchar(max)");

        builder.Property(f => f.Language)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(f => f.HandoffId);
    }
}
