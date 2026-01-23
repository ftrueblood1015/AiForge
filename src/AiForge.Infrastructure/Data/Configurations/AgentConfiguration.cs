using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AgentKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.SystemPrompt)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Instructions)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.AgentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Capabilities)
            .HasColumnType("nvarchar(max)");  // JSON array

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(200);

        // Indexes for performance
        builder.HasIndex(a => a.AgentKey);
        builder.HasIndex(a => a.OrganizationId);
        builder.HasIndex(a => a.ProjectId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.IsEnabled);

        // Unique constraint: AgentKey must be unique within scope
        builder.HasIndex(a => new { a.AgentKey, a.OrganizationId, a.ProjectId })
            .IsUnique()
            .HasFilter(null);  // Include nulls in uniqueness check

        // Check constraint: exactly one of OrganizationId or ProjectId must be set
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Agent_Scope",
            "(OrganizationId IS NOT NULL AND ProjectId IS NULL) OR (OrganizationId IS NULL AND ProjectId IS NOT NULL)"));

        // Relationship to Project (optional)
        builder.HasOne(a => a.Project)
            .WithMany()
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
