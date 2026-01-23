using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SkillKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(200);

        // Indexes for performance
        builder.HasIndex(s => s.SkillKey);
        builder.HasIndex(s => s.OrganizationId);
        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => s.Category);
        builder.HasIndex(s => s.IsPublished);

        // Unique constraint: SkillKey must be unique within scope
        builder.HasIndex(s => new { s.SkillKey, s.OrganizationId, s.ProjectId })
            .IsUnique()
            .HasFilter(null);

        // Check constraint: exactly one of OrganizationId or ProjectId must be set
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Skill_Scope",
            "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)"));

        // Relationship to Project (optional)
        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
