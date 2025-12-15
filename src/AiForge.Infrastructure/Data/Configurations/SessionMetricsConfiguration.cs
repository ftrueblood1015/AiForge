using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SessionMetricsConfiguration : IEntityTypeConfiguration<SessionMetrics>
{
    public void Configure(EntityTypeBuilder<SessionMetrics> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        // Indexes for analytics queries
        builder.HasIndex(s => s.TicketId);
        builder.HasIndex(s => s.SessionId);
        builder.HasIndex(s => s.SessionStartedAt);
        builder.HasIndex(s => s.CreatedAt);
    }
}
