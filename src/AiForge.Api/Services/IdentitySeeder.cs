using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using AiForge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Api.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AiForgeDbContext dbContext,
        IConfiguration configuration)
    {
        // Create roles
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    NormalizedName = role.ToUpperInvariant()
                });
                Console.WriteLine($"Created role: {role}");
            }
        }

        // Create default organization if none exists
        Organization? defaultOrg = await dbContext.Organizations.FirstOrDefaultAsync();
        if (defaultOrg == null)
        {
            defaultOrg = new Organization
            {
                Id = Guid.NewGuid(),
                Name = "Default Organization",
                Slug = "default",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = Guid.Empty // Will be updated after admin creation
            };
            dbContext.Organizations.Add(defaultOrg);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Created default organization: {defaultOrg.Name}");
        }

        // Create admin user
        var adminEmail = configuration["Identity:AdminEmail"] ?? "admin@aiforge.local";
        var adminPassword = configuration["Identity:AdminPassword"] ?? "Admin123!";
        var adminDisplayName = configuration["Identity:AdminDisplayName"] ?? "Administrator";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = adminDisplayName,
                DefaultOrganizationId = defaultOrg.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                await userManager.AddToRoleAsync(adminUser, "User");

                // Add admin to default organization
                var membership = new OrganizationMember
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = defaultOrg.Id,
                    UserId = adminUser.Id,
                    Role = Domain.Enums.OrganizationRole.Admin,
                    JoinedAt = DateTime.UtcNow
                };
                dbContext.OrganizationMembers.Add(membership);

                // Update organization creator
                defaultOrg.CreatedByUserId = adminUser.Id;
                await dbContext.SaveChangesAsync();

                Console.WriteLine($"Created admin user: {adminEmail}");
                Console.WriteLine($"Admin password: {adminPassword}");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            Console.WriteLine($"Admin user already exists: {adminEmail}");
        }
    }
}
