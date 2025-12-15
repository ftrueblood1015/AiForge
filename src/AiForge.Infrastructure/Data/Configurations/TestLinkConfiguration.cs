using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class TestLinkConfiguration : IEntityTypeConfiguration<TestLink>
{
    public void Configure(EntityTypeBuilder<TestLink> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TestFilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.TestName)
            .HasMaxLength(500);

        builder.Property(t => t.TestedFunctionality)
            .HasMaxLength(500);

        builder.Property(t => t.Outcome)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.LinkedFilePath)
            .HasMaxLength(1000);

        builder.Property(t => t.SessionId)
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.TicketId);
        builder.HasIndex(t => t.TestFilePath);
        builder.HasIndex(t => t.LinkedFilePath);

        // Relationship to Ticket
        builder.HasOne(t => t.Ticket)
            .WithMany(ticket => ticket.TestLinks)
            .HasForeignKey(t => t.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
