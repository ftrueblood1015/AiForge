using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SkillChainConfiguration : IEntityTypeConfiguration<SkillChain>
{
    public void Configure(EntityTypeBuilder<SkillChain> builder)
    {
        builder.ToTable("SkillChains");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.ChainKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sc => sc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sc => sc.Description)
            .HasMaxLength(2000);

        builder.Property(sc => sc.InputSchema)
            .HasColumnType("nvarchar(max)");

        builder.Property(sc => sc.MaxTotalFailures)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(sc => sc.CreatedAt)
            .IsRequired();

        builder.Property(sc => sc.CreatedBy)
            .HasMaxLength(200);

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired();

        builder.Property(sc => sc.UpdatedBy)
            .HasMaxLength(200);

        // Indexes for performance
        builder.HasIndex(sc => sc.ChainKey);
        builder.HasIndex(sc => sc.OrganizationId);
        builder.HasIndex(sc => sc.ProjectId);
        builder.HasIndex(sc => sc.IsPublished);

        // Unique constraint: ChainKey must be unique within scope
        builder.HasIndex(sc => new { sc.ChainKey, sc.OrganizationId, sc.ProjectId })
            .IsUnique()
            .HasFilter(null);

        // Check constraint: exactly one of OrganizationId or ProjectId must be set
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SkillChain_Scope",
            "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)"));

        // Relationship to Project (optional)
        builder.HasOne(sc => sc.Project)
            .WithMany()
            .HasForeignKey(sc => sc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Links collection configured in SkillChainLinkConfiguration
        // Executions collection configured in SkillChainExecutionConfiguration
    }
}
