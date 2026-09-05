namespace HxsAiSystem.Infrastructure.Configuration;

/// <summary>数据库连接配置。</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public string ConnectionString { get; set; } = string.Empty;
    public string DbType { get; set; } = "Oracle";
}
