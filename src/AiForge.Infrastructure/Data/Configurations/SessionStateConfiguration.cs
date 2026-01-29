using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class SessionStateConfiguration : IEntityTypeConfiguration<SessionState>
{
    public void Configure(EntityTypeBuilder<SessionState> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.WorkingSummary)
            .HasMaxLength(4000);

        builder.Property(s => s.LastCheckpoint)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.CurrentPhase)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(s => s.Ticket)
            .WithMany()
            .HasForeignKey(s => s.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Queue)
            .WithMany()
            .HasForeignKey(s => s.QueueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for efficient queries
        builder.HasIndex(s => s.SessionId).IsUnique();
        builder.HasIndex(s => s.TicketId);
        builder.HasIndex(s => s.QueueId);
        builder.HasIndex(s => s.ExpiresAt);
        builder.HasIndex(s => s.CreatedAt);
    }
}
