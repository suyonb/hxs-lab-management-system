namespace HxsAiSystem.Application.AiReasoning;

public sealed record ConversationDto(Guid Id, string? Title, DateTime CreateTime, DateTime UpdateTime);
public sealed record AiMessageDto(Guid Id, Guid ConversationId, string Role, string Content, string MessageType, ReasoningResult? Result, DateTime CreateTime);

public sealed class CreateConversationRequest
{
    public string? Title { get; set; }
}

public sealed class ReasoningRequest
{
    public string Content { get; set; } = string.Empty;
}

public sealed class ReasoningResponse
{
    public required AiMessageDto UserMessage { get; init; }
    public required AiMessageDto AssistantMessage { get; init; }
    public required ReasoningResult Result { get; init; }
    public string Provider { get; init; } = "demo";
}

public sealed class ReasoningResult
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Facts { get; set; } = [];
    public List<string> Inferences { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];
    public double Confidence { get; set; }
}
