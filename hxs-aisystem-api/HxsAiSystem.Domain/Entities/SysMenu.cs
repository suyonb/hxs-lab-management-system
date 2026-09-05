using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>系统菜单和权限点。</summary>
[SugarTable("HXS_SYS_MENU")]
public class SysMenu
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "PARENT_ID", IsNullable = true)]
    public byte[]? ParentId { get; set; }

    [SugarColumn(ColumnName = "MENU_CODE", Length = 100)]
    public string MenuCode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "MENU_NAME", Length = 100)]
    public string MenuName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "MENU_TYPE", Length = 30)]
    public string MenuType { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "ROUTE_PATH", Length = 200, IsNullable = true)]
    public string? RoutePath { get; set; }

    [SugarColumn(ColumnName = "COMPONENT", Length = 200, IsNullable = true)]
    public string? Component { get; set; }

    [SugarColumn(ColumnName = "ICON", Length = 100, IsNullable = true)]
    public string? Icon { get; set; }

    [SugarColumn(ColumnName = "PERMISSION_CODE", Length = 100, IsNullable = true)]
    public string? PermissionCode { get; set; }

    [SugarColumn(ColumnName = "SORT_NO")]
    public int SortNo { get; set; }

    [SugarColumn(ColumnName = "IS_VISIBLE")]
    public int IsVisible { get; set; }

    [SugarColumn(ColumnName = "IS_ACTIVE")]
    public int IsActive { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnName = "UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }
}
