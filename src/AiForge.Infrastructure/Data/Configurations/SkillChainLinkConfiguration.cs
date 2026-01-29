using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SkillChainLinkConfiguration : IEntityTypeConfiguration<SkillChainLink>
{
    public void Configure(EntityTypeBuilder<SkillChainLink> builder)
    {
        builder.ToTable("SkillChainLinks");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Position)
            .IsRequired();

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(2000);

        builder.Property(l => l.MaxRetries)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(l => l.OnSuccessTransition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.OnFailureTransition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.LinkConfig)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(l => l.SkillChainId);
        builder.HasIndex(l => l.SkillId);
        builder.HasIndex(l => l.AgentId);

        // Unique constraint: Position must be unique within chain
        builder.HasIndex(l => new { l.SkillChainId, l.Position })
            .IsUnique();

        // Relationship to SkillChain
        builder.HasOne(l => l.SkillChain)
            .WithMany(sc => sc.Links)
            .HasForeignKey(l => l.SkillChainId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to Skill (required)
        builder.HasOne(l => l.Skill)
            .WithMany(s => s.ChainLinks)
            .HasForeignKey(l => l.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Agent (optional)
        builder.HasOne(l => l.Agent)
            .WithMany(a => a.ChainLinks)
            .HasForeignKey(l => l.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referential relationships for transitions
        builder.HasOne(l => l.OnSuccessTargetLink)
            .WithMany()
            .HasForeignKey(l => l.OnSuccessTargetLinkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.OnFailureTargetLink)
            .WithMany()
            .HasForeignKey(l => l.OnFailureTargetLinkId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
