namespace AiForge.Application.DTOs.Analytics.Patterns;

public class DebtPatternSummaryDto
{
    public int TotalDebtItems { get; set; }
    public int OpenDebtItems { get; set; }
    public int ResolvedDebtItems { get; set; }
    public List<DebtByCategoryDto> ByCategory { get; set; } = new();
    public List<DebtBySeverityDto> BySeverity { get; set; } = new();
    public List<DebtHotspotDto> TopHotspots { get; set; } = new();
}

public class DebtByCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public int OpenCount { get; set; }
}

public class DebtBySeverityDto
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
    public int OpenCount { get; set; }
}

public class DebtHotspotDto
{
    public string FilePath { get; set; } = string.Empty;
    public int DebtItemCount { get; set; }
    public List<string> Categories { get; set; } = new();
}

public class DebtPatternRequest
{
    public Guid? ProjectId { get; set; }
    public bool IncludeResolved { get; set; } = false;
}
