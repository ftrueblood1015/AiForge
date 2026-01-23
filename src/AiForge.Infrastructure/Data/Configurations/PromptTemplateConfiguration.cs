using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class PromptTemplateConfiguration : IEntityTypeConfiguration<PromptTemplate>
{
    public void Configure(EntityTypeBuilder<PromptTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TemplateKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Variables)
            .HasColumnType("nvarchar(max)");  // JSON schema

        builder.Property(t => t.Category)
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(200);

        // Indexes for performance
        builder.HasIndex(t => t.TemplateKey);
        builder.HasIndex(t => t.OrganizationId);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.Category);

        // Unique constraint: TemplateKey must be unique within scope
        builder.HasIndex(t => new { t.TemplateKey, t.OrganizationId, t.ProjectId })
            .IsUnique()
            .HasFilter(null);

        // Check constraint: exactly one of OrganizationId or ProjectId must be set
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_PromptTemplate_Scope",
            "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)"));

        // Relationship to Project (optional)
        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
