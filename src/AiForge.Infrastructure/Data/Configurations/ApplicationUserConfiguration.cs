using AiForge.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiForge.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.IsActive);

        builder.HasOne(u => u.DefaultOrganization)
            .WithMany()
            .HasForeignKey(u => u.DefaultOrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.OrganizationMemberships)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ProjectMemberships)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ApiKeys)
            .WithOne()
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
