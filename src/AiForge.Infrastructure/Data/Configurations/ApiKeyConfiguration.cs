using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(a => a.Key)
            .IsUnique();

        builder.HasIndex(a => a.IsActive);

        builder.HasMany(a => a.Usages)
            .WithOne(u => u.ApiKey)
            .HasForeignKey(u => u.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
