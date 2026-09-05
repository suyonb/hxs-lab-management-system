namespace HxsAiSystem.Application.AiReasoning;

public interface IAiModelClient
{
    string ProviderName { get; }
    Task<ReasoningResult> AnalyzeAsync(string content, CancellationToken cancellationToken = default);
}
