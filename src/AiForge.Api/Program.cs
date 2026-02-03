using System.Text;
using System.Text.Json.Serialization;
using AiForge.Application;
using AiForge.Application.Interfaces;
using AiForge.Application.Services;
using AiForge.Infrastructure;
using AiForge.Infrastructure.Data;
using AiForge.Infrastructure.Identity;
using AiForge.Api.Authorization;
using AiForge.Api.Middleware;
using AiForge.Api.Services;
using AiForge.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure and Application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password policy
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 4;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AiForgeDbContext>()
.AddDefaultTokenProviders();

// Add Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Add HttpContextAccessor and UserContext for accessing current user
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// Add Authorization handler
builder.Services.AddScoped<IAuthorizationHandler, ProjectAccessHandler>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AiForge";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AiForge";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
    };
});

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Authorization Policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
        policy.RequireRole("Admin"))
    .AddPolicy(AuthorizationPolicies.RequireProjectAccess, policy =>
        policy.AddRequirements(new ProjectAccessRequirement(ProjectRole.Viewer)))
    .AddPolicy(AuthorizationPolicies.RequireProjectMember, policy =>
        policy.AddRequirements(new ProjectAccessRequirement(ProjectRole.Member)))
    .AddPolicy(AuthorizationPolicies.RequireProjectOwner, policy =>
        policy.AddRequirements(new ProjectAccessRequirement(ProjectRole.Owner)));

var app = builder.Build();

// Seed database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AiForgeDbContext>();
    await DbSeeder.SeedAsync(dbContext);

    // Seed identity (roles and admin user)
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await IdentitySeeder.SeedAsync(userManager, roleManager, dbContext, builder.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// JWT Authentication (must come before API Key middleware)
app.UseAuthentication();

// API Key authentication middleware (for service accounts/MCP)
// This checks API key only if JWT auth didn't succeed
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint (excluded from auth)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
