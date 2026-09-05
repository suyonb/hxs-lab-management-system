using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>系统角色。</summary>
[SugarTable("HXS_SYS_ROLE")]
public class SysRole
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "ROLE_CODE", Length = 50)]
    public string RoleCode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "ROLE_NAME", Length = 100)]
    public string RoleName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "DESCRIPTION", Length = 300, IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnName = "IS_ACTIVE")]
    public int IsActive { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnName = "UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }
}
