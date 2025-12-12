# AiForge

A ticket management system with "show your work" transparency features, designed for Claude AI integration via MCP (Model Context Protocol).

## Overview

AiForge enables Claude to track its work transparently through:

- **Ticket Management** - JIRA-like workflow with projects, tickets, and status tracking
- **Planning Sessions** - Document initial understanding and approach for tasks
- **Reasoning Logs** - Record decisions with rationale and confidence levels
- **Progress Entries** - Track work done/attempted with outcomes
- **Handoff Documents** - Preserve context between sessions
- **File Snapshots** - Before/after code comparisons

## Technology Stack

| Component | Technology |
|-----------|------------|
| Backend API | C# .NET 8, ASP.NET Core Web API |
| Database | SQL Server Express + Entity Framework Core 8 |
| Frontend | React 19 + Vite + TypeScript |
| UI Library | MUI (Material UI) v7 |
| State Management | Zustand |
| MCP Server | C# with ModelContextProtocol.Server |
| Authentication | API Key via `X-Api-Key` header |

## Project Structure

```
AiForge/
├── src/
│   ├── AiForge.Domain/           # Entities, enums, interfaces
│   ├── AiForge.Infrastructure/   # EF Core, DbContext, repositories
│   ├── AiForge.Application/      # Services, DTOs, mapping
│   ├── AiForge.Api/              # REST API controllers
│   └── AiForge.Mcp/              # MCP Server for Claude
├── frontend/
│   └── aiforge-ui/               # React frontend
├── tests/
│   ├── AiForge.Api.Tests/
│   └── AiForge.Application.Tests/
└── AiForge.sln
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (for frontend)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or full)
- [Claude Code CLI](https://claude.com/claude-code) (for MCP integration)

## Getting Started

### 1. Clone and Build

```bash
git clone <repository-url>
cd AiForge
dotnet build
```

### 2. Database Setup

The database is automatically created and seeded on first run. Default connection uses SQL Server Express:

```
Server=.\SQLEXPRESS;Database=AiForge;Trusted_Connection=True;
```

To customize, edit `src/AiForge.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AiForge;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Run migrations manually if needed:

```bash
dotnet ef database update -p src/AiForge.Infrastructure -s src/AiForge.Api
```

### 3. Start the Backend API

```bash
dotnet run --project src/AiForge.Api
```

The API runs at `https://localhost:7149` (or the port shown in console).

Swagger UI available at: `https://localhost:7149/swagger`

### 4. Configure the Frontend

Create `.env` file in `frontend/aiforge-ui/`:

```bash
cd frontend/aiforge-ui
cp .env.example .env
```

Edit `.env` with your settings:

```env
VITE_API_URL=https://localhost:7149
VITE_API_KEY=your-api-key-here
```

**Getting an API Key:** The database is seeded with a default API key on first run. Check the `ApiKeys` table in your database, or check the console output when the API starts.

### 5. Start the Frontend

```bash
cd frontend/aiforge-ui
npm install
npm run dev
```

Frontend runs at: `http://localhost:5173`

## Claude MCP Server Setup

The MCP server allows Claude to directly interact with AiForge for ticket management and "show your work" documentation.

### 1. Build the MCP Server

```bash
dotnet build src/AiForge.Mcp -c Release
```

### 2. Add to Claude Code

```bash
claude mcp add aiforge -- dotnet run --project "C:\path\to\AiForge\src\AiForge.Mcp"
```

Or for the built executable:

```bash
claude mcp add aiforge -- "C:\path\to\AiForge\src\AiForge.Mcp\bin\Release\net8.0\AiForge.Mcp.exe"
```

### 3. Verify Installation

```bash
claude mcp list
```

You should see `aiforge` in the list of configured MCP servers.

### Available MCP Tools

Once configured, Claude has access to these tools:

**Project Management:**
- `list_projects` - List all projects
- `create_project` - Create a new project
- `get_project` - Get project details

**Ticket Management:**
- `create_ticket` - Create a new ticket
- `get_ticket` - Get ticket details
- `search_tickets` - Search tickets with filters
- `transition_ticket` - Change ticket status
- `update_ticket` - Update ticket fields
- `add_comment` - Add comment to ticket

**Planning & Documentation:**
- `start_planning` - Start a planning session
- `complete_planning` - Complete a planning session
- `log_decision` - Log a decision with rationale
- `log_progress` - Log progress on a task
- `get_planning_data` - Get all planning data for a ticket
- `get_context` - Get full context for resuming work

**Handoffs:**
- `create_handoff` - Create a handoff document
- `list_handoffs` - List handoffs for a ticket
- `get_handoff` - Get handoff details
- `add_file_snapshot` - Add file diff to handoff

## Configuration Reference

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AiForge;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Frontend (.env)

| Variable | Description | Example |
|----------|-------------|---------|
| `VITE_API_URL` | Backend API URL | `https://localhost:7149` |
| `VITE_API_KEY` | API key for authentication | `69f49759-4c65-4ff8-afac-b2afda2a66b9` |

### MCP Server (appsettings.json)

The MCP server uses the same connection string as the API:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AiForge;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Common Commands

### Backend

```bash
# Run API with hot reload
dotnet watch run --project src/AiForge.Api

# Run tests
dotnet test

# Add EF migration
dotnet ef migrations add MigrationName -p src/AiForge.Infrastructure -s src/AiForge.Api

# Update database
dotnet ef database update -p src/AiForge.Infrastructure -s src/AiForge.Api
```

### Frontend

```bash
cd frontend/aiforge-ui

npm run dev        # Development server
npm run build      # Production build
npm run test       # Run tests
npm run lint       # Run linter
```

## API Authentication

All API endpoints (except `/health` and `/swagger`) require the `X-Api-Key` header:

```
X-Api-Key: your-api-key-guid
```

Rate limiting is enforced per API key.

## Workflow

Tickets follow this status flow:

```
ToDo → InProgress → InReview → Done
```

## License

[Add your license here]
