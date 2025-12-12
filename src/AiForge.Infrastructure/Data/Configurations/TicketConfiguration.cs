using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Description)
            .HasMaxLength(10000);

        builder.Property(t => t.CurrentHandoffSummary)
            .HasMaxLength(2000);

        builder.HasIndex(t => t.Key)
            .IsUnique();

        builder.HasIndex(t => new { t.ProjectId, t.Status });
        builder.HasIndex(t => new { t.ProjectId, t.Number });

        // Self-referencing relationship for parent/sub-tickets
        builder.HasOne(t => t.ParentTicket)
            .WithMany(t => t.SubTickets)
            .HasForeignKey(t => t.ParentTicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.History)
            .WithOne(h => h.Ticket)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.PlanningSessions)
            .WithOne(p => p.Ticket)
            .HasForeignKey(p => p.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.ReasoningLogs)
            .WithOne(r => r.Ticket)
            .HasForeignKey(r => r.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.ProgressEntries)
            .WithOne(p => p.Ticket)
            .HasForeignKey(p => p.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Handoffs)
            .WithOne(h => h.Ticket)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
