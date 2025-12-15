namespace AiForge.Application.DTOs.Analytics.Patterns;

public class FileCorrelationDto
{
    public string FilePath { get; set; } = string.Empty;
    public List<CorrelatedFile> CorrelatedFiles { get; set; } = new();
}

public class CorrelatedFile
{
    public string FilePath { get; set; } = string.Empty;
    public int CooccurrenceCount { get; set; }
    public double CorrelationStrength { get; set; } // 0-1
}

public class FileCorrelationRequest
{
    public string FilePath { get; set; } = string.Empty;
    public int MinCooccurrence { get; set; } = 2;
    public int TopN { get; set; } = 10;
}
