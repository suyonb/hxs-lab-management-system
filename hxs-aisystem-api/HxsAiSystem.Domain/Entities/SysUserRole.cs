using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>用户角色关联。</summary>
[SugarTable("HXS_SYS_USER_ROLE")]
public class SysUserRole
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "USER_ID")]
    public byte[] UserId { get; set; } = [];

    [SugarColumn(ColumnName = "ROLE_ID")]
    public byte[] RoleId { get; set; } = [];

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }
}
