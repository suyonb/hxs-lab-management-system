using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>AI 会话消息。</summary>
[SugarTable("HXS_AI_MESSAGE")]
public class AiMessage
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "CONVERSATION_ID")]
    public byte[] ConversationId { get; set; } = [];

    [SugarColumn(ColumnName = "ROLE", Length = 20)]
    public string Role { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "CONTENT", ColumnDataType = "CLOB")]
    public string Content { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "MESSAGE_TYPE", Length = 30)]
    public string MessageType { get; set; } = "text";

    [SugarColumn(ColumnName = "METADATA", ColumnDataType = "CLOB", IsNullable = true)]
    public string? Metadata { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }
}
