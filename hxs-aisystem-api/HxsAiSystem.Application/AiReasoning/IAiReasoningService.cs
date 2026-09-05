namespace HxsAiSystem.Application.AiReasoning;

public interface IAiReasoningService
{
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request);
    Task<List<AiMessageDto>> GetMessagesAsync(Guid userId, Guid conversationId);
    Task<ReasoningResponse> ReasonAsync(Guid userId, Guid conversationId, ReasoningRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId);
}
