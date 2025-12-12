using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ReasoningLogConfiguration : IEntityTypeConfiguration<ReasoningLog>
{
    public void Configure(EntityTypeBuilder<ReasoningLog> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.SessionId)
            .HasMaxLength(100);

        builder.Property(r => r.DecisionPoint)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.OptionsConsidered)
            .HasColumnType("nvarchar(max)"); // JSON array

        builder.Property(r => r.ChosenOption)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Rationale)
            .IsRequired()
            .HasMaxLength(5000);

        builder.HasIndex(r => r.TicketId);
        builder.HasIndex(r => r.SessionId);
        builder.HasIndex(r => r.CreatedAt);
    }
}
