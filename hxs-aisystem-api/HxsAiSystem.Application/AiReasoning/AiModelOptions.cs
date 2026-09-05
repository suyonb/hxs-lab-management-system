namespace HxsAiSystem.Application.AiReasoning;

public sealed class AiModelOptions
{
    public const string SectionName = "AiModel";
    public string Provider { get; set; } = "Demo";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60;
}
