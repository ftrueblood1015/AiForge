using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class HandoffVersionConfiguration : IEntityTypeConfiguration<HandoffVersion>
{
    public void Configure(EntityTypeBuilder<HandoffVersion> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.StructuredContext)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(v => new { v.HandoffId, v.Version })
            .IsUnique();
    }
}
