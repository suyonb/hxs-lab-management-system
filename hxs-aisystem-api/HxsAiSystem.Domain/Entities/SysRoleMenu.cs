using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>角色菜单关联。</summary>
[SugarTable("HXS_SYS_ROLE_MENU")]
public class SysRoleMenu
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "ROLE_ID")]
    public byte[] RoleId { get; set; } = [];

    [SugarColumn(ColumnName = "MENU_ID")]
    public byte[] MenuId { get; set; } = [];

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }
}
