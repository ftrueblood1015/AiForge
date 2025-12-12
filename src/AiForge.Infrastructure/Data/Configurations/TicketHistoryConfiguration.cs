using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Field)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.OldValue)
            .HasMaxLength(2000);

        builder.Property(h => h.NewValue)
            .HasMaxLength(2000);

        builder.Property(h => h.ChangedBy)
            .HasMaxLength(100);

        builder.HasIndex(h => h.TicketId);
        builder.HasIndex(h => h.ChangedAt);
    }
}
