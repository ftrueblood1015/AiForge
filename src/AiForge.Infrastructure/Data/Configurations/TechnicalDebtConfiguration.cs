using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class TechnicalDebtConfiguration : IEntityTypeConfiguration<TechnicalDebt>
{
    public void Configure(EntityTypeBuilder<TechnicalDebt> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.Description)
            .HasMaxLength(4000);

        builder.Property(d => d.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(d => d.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Rationale)
            .HasMaxLength(2000);

        builder.Property(d => d.AffectedFiles)
            .HasMaxLength(4000);

        builder.Property(d => d.SessionId)
            .HasMaxLength(100);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.Severity);
        builder.HasIndex(d => d.OriginatingTicketId);

        // Relationship to Originating Ticket
        builder.HasOne(d => d.OriginatingTicket)
            .WithMany(t => t.OriginatedDebts)
            .HasForeignKey(d => d.OriginatingTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to Resolution Ticket (optional)
        // Use Restrict to avoid multiple cascade paths in SQL Server
        builder.HasOne(d => d.ResolutionTicket)
            .WithMany(t => t.ResolvedDebts)
            .HasForeignKey(d => d.ResolutionTicketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
