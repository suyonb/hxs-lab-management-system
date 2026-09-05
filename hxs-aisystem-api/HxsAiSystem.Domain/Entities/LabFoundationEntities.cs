using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>实验室业务实体通用审计字段。</summary>
public abstract class LabEntityBase
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID", ColumnDataType = "RAW(16)")]
    public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime CreateTime { get; set; }
    [SugarColumn(ColumnName = "UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }
}

/// <summary>实验室基础档案。</summary>
[SugarTable("HXS_LAB")]
public sealed class Lab : LabEntityBase
{
    [SugarColumn(ColumnName = "LAB_CODE", Length = 50)] public string LabCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "LAB_NAME", Length = 100)] public string LabName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "MANAGER_ID", IsNullable = true)] public byte[]? ManagerId { get; set; }
    [SugarColumn(ColumnName = "DESCRIPTION", Length = 500, IsNullable = true)] public string? Description { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>实验室楼宇、房间、区域或柜位节点。</summary>
[SugarTable("HXS_LAB_LOCATION")]
public sealed class LabLocation : LabEntityBase
{
    [SugarColumn(ColumnName = "LAB_ID")] public byte[] LabId { get; set; } = [];
    [SugarColumn(ColumnName = "PARENT_ID", IsNullable = true)] public byte[]? ParentId { get; set; }
    [SugarColumn(ColumnName = "LOCATION_CODE", Length = 50)] public string LocationCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "LOCATION_NAME", Length = 100)] public string LocationName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "LOCATION_TYPE", Length = 30)] public string LocationType { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "SORT_NO")] public int SortNo { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>实验室课题组。</summary>
[SugarTable("HXS_LAB_GROUP")]
public sealed class LabGroup : LabEntityBase
{
    [SugarColumn(ColumnName = "LAB_ID")] public byte[] LabId { get; set; } = [];
    [SugarColumn(ColumnName = "GROUP_CODE", Length = 50)] public string GroupCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "GROUP_NAME", Length = 100)] public string GroupName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "LEADER_ID", IsNullable = true)] public byte[]? LeaderId { get; set; }
    [SugarColumn(ColumnName = "DESCRIPTION", Length = 500, IsNullable = true)] public string? Description { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>课题组与系统用户的成员关系。</summary>
[SugarTable("HXS_LAB_GROUP_MEMBER")]
public sealed class LabGroupMember
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")] public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "GROUP_ID")] public byte[] GroupId { get; set; } = [];
    [SugarColumn(ColumnName = "USER_ID")] public byte[] UserId { get; set; } = [];
    [SugarColumn(ColumnName = "MEMBER_ROLE", Length = 30)] public string MemberRole { get; set; } = "member";
    [SugarColumn(ColumnName = "CREATE_TIME")] public DateTime CreateTime { get; set; }
}

/// <summary>实验室供应商档案。</summary>
[SugarTable("HXS_LAB_SUPPLIER")]
public sealed class LabSupplier : LabEntityBase
{
    [SugarColumn(ColumnName = "SUPPLIER_CODE", Length = 50)] public string SupplierCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "SUPPLIER_NAME", Length = 150)] public string SupplierName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "CONTACT_NAME", Length = 100, IsNullable = true)] public string? ContactName { get; set; }
    [SugarColumn(ColumnName = "PHONE", Length = 50, IsNullable = true)] public string? Phone { get; set; }
    [SugarColumn(ColumnName = "EMAIL", Length = 150, IsNullable = true)] public string? Email { get; set; }
    [SugarColumn(ColumnName = "ADDRESS", Length = 300, IsNullable = true)] public string? Address { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>实验室业务字典类型。</summary>
[SugarTable("HXS_SYS_DICT_TYPE")]
public sealed class SysDictType : LabEntityBase
{
    [SugarColumn(ColumnName = "DICT_CODE", Length = 50)] public string DictCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "DICT_NAME", Length = 100)] public string DictName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "DESCRIPTION", Length = 500, IsNullable = true)] public string? Description { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>实验室业务字典项。</summary>
[SugarTable("HXS_SYS_DICT_ITEM")]
public sealed class SysDictItem : LabEntityBase
{
    [SugarColumn(ColumnName = "DICT_TYPE_ID")] public byte[] DictTypeId { get; set; } = [];
    [SugarColumn(ColumnName = "ITEM_VALUE", Length = 100)] public string ItemValue { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "ITEM_LABEL", Length = 100)] public string ItemLabel { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "SORT_NO")] public int SortNo { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}
