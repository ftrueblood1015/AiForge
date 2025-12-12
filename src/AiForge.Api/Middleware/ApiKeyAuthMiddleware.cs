using AiForge.Domain.Entities;
using AiForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForge.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-Api-Key";

    private static readonly string[] ExcludedPaths = new[]
    {
        "/health",
        "/swagger",
        "/favicon.ico"
    };

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AiForgeDbContext dbContext)
    {
        // Skip auth for excluded paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (ExcludedPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key required", header = API_KEY_HEADER });
            return;
        }

        var apiKey = await dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == apiKeyHeader.ToString() && k.IsActive);

        if (apiKey == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // Check rate limit (if configured)
        if (apiKey.RateLimitPerMinute > 0)
        {
            var now = DateTime.UtcNow;
            var windowStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);

            var usage = await dbContext.ApiKeyUsages
                .FirstOrDefaultAsync(u => u.ApiKeyId == apiKey.Id && u.WindowStart == windowStart);

            if (usage == null)
            {
                // First request in this window - create new usage record
                usage = new ApiKeyUsage
                {
                    Id = Guid.NewGuid(),
                    ApiKeyId = apiKey.Id,
                    WindowStart = windowStart,
                    RequestCount = 1
                };
                dbContext.ApiKeyUsages.Add(usage);

                // Clean up old usage records (older than 5 minutes)
                var cutoff = windowStart.AddMinutes(-5);
                var oldUsages = dbContext.ApiKeyUsages.Where(u => u.WindowStart < cutoff);
                dbContext.ApiKeyUsages.RemoveRange(oldUsages);
            }
            else if (usage.RequestCount >= apiKey.RateLimitPerMinute)
            {
                // Rate limit exceeded
                var secondsUntilReset = 60 - now.Second;
                context.Response.StatusCode = 429;
                context.Response.Headers["Retry-After"] = secondsUntilReset.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = apiKey.RateLimitPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = windowStart.AddMinutes(1).ToString("o");
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    retryAfterSeconds = secondsUntilReset,
                    limit = apiKey.RateLimitPerMinute
                });
                return;
            }
            else
            {
                // Increment counter
                usage.RequestCount++;
            }

            // Add rate limit headers to response
            var remaining = Math.Max(0, apiKey.RateLimitPerMinute - usage.RequestCount);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit"] = apiKey.RateLimitPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = windowStart.AddMinutes(1).ToString("o");
                return Task.CompletedTask;
            });
        }

        // Update last used
        apiKey.LastUsedAt = DateTime.UtcNow;

        // Store API key info in HttpContext for use in controllers
        context.Items["ApiKey"] = apiKey;

        // Save changes with concurrency handling - usage tracking should not fail the request
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request updated the same record - this is fine for usage tracking
            // Clear the change tracker to prevent issues with subsequent operations
            dbContext.ChangeTracker.Clear();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Another request created the usage record first - this is fine
            dbContext.ChangeTracker.Clear();
        }

        await _next(context);
    }
}
