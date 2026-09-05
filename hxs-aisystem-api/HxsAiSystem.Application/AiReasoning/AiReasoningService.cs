using System.Text.Json;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace HxsAiSystem.Application.AiReasoning;

public sealed class AiReasoningService : IAiReasoningService
{
    private readonly ISqlSugarClient _db;
    private readonly IAiModelClient _modelClient;

    public AiReasoningService(ISqlSugarClient db, DemoReasoningClient demoClient, OpenAiCompatibleClient openAiClient, IOptions<AiModelOptions> options)
    {
        _db = db;
        _modelClient = options.Value.Provider.Equals("OpenAiCompatible", StringComparison.OrdinalIgnoreCase) ? openAiClient : demoClient;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        var userRaw = RawGuidConverter.ToRaw(userId);
        var rows = await _db.Queryable<AiConversation>().Where(x => x.UserId == userRaw).OrderBy(x => x.UpdateTime, OrderByType.Desc).ToListAsync();
        return rows.Select(ToConversationDto).ToList();
    }

    public async Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request)
    {
        var now = DateTime.Now;
        var row = new AiConversation
        {
            Id = RawGuidConverter.ToRaw(Guid.NewGuid()),
            UserId = RawGuidConverter.ToRaw(userId),
            Title = NormalizeTitle(request.Title),
            CreateTime = now,
            UpdateTime = now
        };
        await _db.Insertable(row).ExecuteCommandAsync();
        return ToConversationDto(row);
    }

    public async Task<List<AiMessageDto>> GetMessagesAsync(Guid userId, Guid conversationId)
    {
        await EnsureConversationAsync(userId, conversationId);
        var conversationRaw = RawGuidConverter.ToRaw(conversationId);
        var rows = await _db.Queryable<AiMessage>().Where(x => x.ConversationId == conversationRaw).OrderBy(x => x.CreateTime).ToListAsync();
        return rows.Select(ToMessageDto).ToList();
    }

    public async Task<ReasoningResponse> ReasonAsync(Guid userId, Guid conversationId, ReasoningRequest request, CancellationToken cancellationToken = default)
    {
        var content = request.Content.Trim();
        if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("请输入需要推理的内容。");
        if (content.Length > 12000) throw new InvalidOperationException("单次输入不能超过 12000 个字符。");

        var conversation = await EnsureConversationAsync(userId, conversationId);
        var conversationRaw = RawGuidConverter.ToRaw(conversationId);
        var now = DateTime.Now;
        var userMessage = new AiMessage
        {
            Id = RawGuidConverter.ToRaw(Guid.NewGuid()), ConversationId = conversationRaw, Role = "user",
            Content = content, MessageType = "text", CreateTime = now
        };
        await _db.Insertable(userMessage).ExecuteCommandAsync();

        var result = await _modelClient.AnalyzeAsync(content, cancellationToken);
        var resultJson = JsonSerializer.Serialize(result);
        var assistantMessage = new AiMessage
        {
            Id = RawGuidConverter.ToRaw(Guid.NewGuid()), ConversationId = conversationRaw, Role = "assistant",
            Content = result.Summary, MessageType = "reasoning", Metadata = resultJson, CreateTime = DateTime.Now
        };
        await _db.Insertable(assistantMessage).ExecuteCommandAsync();

        conversation.Title ??= NormalizeTitle(content);
        conversation.UpdateTime = DateTime.Now;
        await _db.Updateable(conversation).UpdateColumns(x => new { x.Title, x.UpdateTime }).ExecuteCommandAsync();

        return new ReasoningResponse
        {
            UserMessage = ToMessageDto(userMessage), AssistantMessage = ToMessageDto(assistantMessage),
            Result = result, Provider = _modelClient.ProviderName
        };
    }

    public async Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId)
    {
        await EnsureConversationAsync(userId, conversationId);
        var raw = RawGuidConverter.ToRaw(conversationId);
        await _db.Deleteable<AiMessage>().Where(x => x.ConversationId == raw).ExecuteCommandAsync();
        return await _db.Deleteable<AiConversation>().Where(x => x.Id == raw).ExecuteCommandAsync() > 0;
    }

    private async Task<AiConversation> EnsureConversationAsync(Guid userId, Guid conversationId)
    {
        var idRaw = RawGuidConverter.ToRaw(conversationId);
        var userRaw = RawGuidConverter.ToRaw(userId);
        return await _db.Queryable<AiConversation>().FirstAsync(x => x.Id == idRaw && x.UserId == userRaw)
               ?? throw new KeyNotFoundException("会话不存在。");
    }

    private static string? NormalizeTitle(string? value)
    {
        var title = value?.Trim();
        if (string.IsNullOrWhiteSpace(title)) return null;
        return title.Length > 60 ? title[..60] + "…" : title;
    }

    private static ConversationDto ToConversationDto(AiConversation row) =>
        new(RawGuidConverter.ToGuid(row.Id), row.Title, row.CreateTime, row.UpdateTime);

    private static AiMessageDto ToMessageDto(AiMessage row)
    {
        ReasoningResult? result = null;
        if (!string.IsNullOrWhiteSpace(row.Metadata))
            try { result = JsonSerializer.Deserialize<ReasoningResult>(row.Metadata, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); } catch (JsonException) { }
        return new AiMessageDto(RawGuidConverter.ToGuid(row.Id), RawGuidConverter.ToGuid(row.ConversationId), row.Role, row.Content, row.MessageType, result, row.CreateTime);
    }
}
