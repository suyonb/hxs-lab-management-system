using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>系统组织架构。</summary>
[SugarTable("HXS_SYS_ORG")]
public class SysOrg
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")]
    public byte[] Id { get; set; } = [];

    [SugarColumn(ColumnName = "PARENT_ID", IsNullable = true)]
    public byte[]? ParentId { get; set; }

    [SugarColumn(ColumnName = "ORG_NAME", Length = 100)]
    public string OrgName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "ORG_CODE", Length = 50)]
    public string OrgCode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "ORG_TYPE", Length = 30)]
    public string OrgType { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "SORT_NO")]
    public int SortNo { get; set; }

    [SugarColumn(ColumnName = "IS_ACTIVE")]
    public int IsActive { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }

    [SugarColumn(ColumnName = "UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }
}
