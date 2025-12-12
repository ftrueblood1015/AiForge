using AiForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AiForgeDbContext context)
    {
        // Create default API key if none exists
        if (!await context.ApiKeys.AnyAsync())
        {
            var defaultKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Key = Guid.NewGuid().ToString(),
                Name = "Default Development Key",
                IsActive = true,
                RateLimitPerMinute = 60,
                CreatedAt = DateTime.UtcNow
            };

            context.ApiKeys.Add(defaultKey);
            Console.WriteLine($"Created default API key: {defaultKey.Key}");
        }

        // Create sample project if none exists
        if (!await context.Projects.AnyAsync())
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Key = "DEMO",
                Name = "Demo Project",
                Description = "A sample project for testing AiForge",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                NextTicketNumber = 1
            };

            context.Projects.Add(project);
        }

        await context.SaveChangesAsync();
    }
}
