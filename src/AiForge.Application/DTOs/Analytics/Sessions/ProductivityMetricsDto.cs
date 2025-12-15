namespace AiForge.Application.DTOs.Analytics.Sessions;

public class ProductivityMetricsDto
{
    public int TotalTicketsCompleted { get; set; }
    public int TotalSessions { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int TotalTokensUsed { get; set; }

    public double AverageSessionsPerTicket { get; set; }
    public double AverageMinutesPerTicket { get; set; }
    public double AverageTokensPerTicket { get; set; }
    public double AverageDecisionsPerTicket { get; set; }

    public List<ProductivityByType> ByTicketType { get; set; } = new();
    public List<ProductivityByDay> DailyTrend { get; set; } = new();

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProductivityByType
{
    public string TicketType { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int AverageMinutes { get; set; }
    public int AverageTokens { get; set; }
}

public class ProductivityByDay
{
    public DateTime Date { get; set; }
    public int SessionCount { get; set; }
    public int TicketsWorkedOn { get; set; }
    public int TotalMinutes { get; set; }
    public int TotalTokens { get; set; }
}

public class ProductivityMetricsRequest
{
    public Guid? ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
