using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>AI 会话。</summary>
[SugarTable("HXS_AI_CONVERSATION")]
public class AiConversation
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "USER_ID")]
    public byte[] UserId { get; set; } = [];

    [SugarColumn(ColumnName = "TITLE", Length = 200, IsNullable = true)]
    public string? Title { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnName = "UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }
}
