using AiForge.Application;
using AiForge.Application.Interfaces;
using AiForge.Application.Services;
using AiForge.Infrastructure;
using AiForge.Infrastructure.Data;
using AiForge.Mcp.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (stdout is used for MCP communication)
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

// Add database context
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? "Server=.\\SQLEXPRESS;Database=AiForge;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AiForgeDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add application layer services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Register ServiceAccountUserContext for MCP server (no HttpContext available)
// This marks all MCP requests as service account requests, bypassing user-specific access control
builder.Services.AddSingleton<IUserContext, ServiceAccountUserContext>();

// Add MCP Server with tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(TicketTools).Assembly);

var app = builder.Build();

await app.RunAsync();
