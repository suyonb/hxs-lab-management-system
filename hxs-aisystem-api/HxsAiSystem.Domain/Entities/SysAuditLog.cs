using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

[SugarTable("HXS_SYS_AUDIT_LOG")]
public class SysAuditLog
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "USER_ID", IsNullable = true)]
    public byte[]? UserId { get; set; }
    [SugarColumn(ColumnName = "USER_NAME", Length = 100, IsNullable = true)]
    public string? UserName { get; set; }
    [SugarColumn(ColumnName = "MODULE_CODE", Length = 100)]
    public string ModuleCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "ACTION_CODE", Length = 100)]
    public string ActionCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "BUSINESS_ID", Length = 100, IsNullable = true)]
    public string? BusinessId { get; set; }
    [SugarColumn(ColumnName = "REQUEST_PATH", Length = 300)]
    public string RequestPath { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "HTTP_METHOD", Length = 20)]
    public string HttpMethod { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "BEFORE_DATA", ColumnDataType = "CLOB", IsNullable = true)]
    public string? BeforeData { get; set; }
    [SugarColumn(ColumnName = "AFTER_DATA", ColumnDataType = "CLOB", IsNullable = true)]
    public string? AfterData { get; set; }
    [SugarColumn(ColumnName = "RESULT", Length = 30)]
    public string Result { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "IP_ADDRESS", Length = 100, IsNullable = true)]
    public string? IpAddress { get; set; }
    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }
}
