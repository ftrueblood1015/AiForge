using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class PlanningSessionConfiguration : IEntityTypeConfiguration<PlanningSession>
{
    public void Configure(EntityTypeBuilder<PlanningSession> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.SessionId)
            .HasMaxLength(100);

        builder.Property(p => p.InitialUnderstanding)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(p => p.Assumptions)
            .HasColumnType("nvarchar(max)"); // JSON array

        builder.Property(p => p.AlternativesConsidered)
            .HasColumnType("nvarchar(max)"); // JSON array

        builder.Property(p => p.ChosenApproach)
            .HasMaxLength(5000);

        builder.Property(p => p.Rationale)
            .HasMaxLength(5000);

        builder.HasIndex(p => p.TicketId);
        builder.HasIndex(p => p.SessionId);
    }
}
