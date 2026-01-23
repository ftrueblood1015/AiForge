using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ConfigurationSetConfiguration : IEntityTypeConfiguration<ConfigurationSet>
{
    public void Configure(EntityTypeBuilder<ConfigurationSet> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.SetKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.AgentIds)
            .HasColumnType("nvarchar(max)");  // JSON array of GUIDs

        builder.Property(c => c.SkillIds)
            .HasColumnType("nvarchar(max)");  // JSON array of GUIDs

        builder.Property(c => c.TemplateIds)
            .HasColumnType("nvarchar(max)");  // JSON array of GUIDs

        builder.Property(c => c.Version)
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(200);

        // Indexes for performance
        builder.HasIndex(c => c.SetKey);
        builder.HasIndex(c => c.OrganizationId);
        builder.HasIndex(c => c.ProjectId);

        // Unique constraint: SetKey must be unique within scope
        builder.HasIndex(c => new { c.SetKey, c.OrganizationId, c.ProjectId })
            .IsUnique()
            .HasFilter(null);

        // Check constraint: exactly one of OrganizationId or ProjectId must be set
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ConfigurationSet_Scope",
            "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)"));

        // Relationship to Project (optional)
        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
