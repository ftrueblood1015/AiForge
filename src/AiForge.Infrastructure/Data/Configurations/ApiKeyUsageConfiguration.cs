using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ApiKeyUsageConfiguration : IEntityTypeConfiguration<ApiKeyUsage>
{
    public void Configure(EntityTypeBuilder<ApiKeyUsage> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => new { u.ApiKeyId, u.WindowStart })
            .IsUnique();

        builder.HasIndex(u => u.WindowStart); // For cleanup queries
    }
}
