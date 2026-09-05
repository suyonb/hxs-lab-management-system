using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>实验室三维场景配置。</summary>
[SugarTable("HXS_LAB_3D_SCENE")]
public sealed class Lab3dScene : LabEntityBase
{
    [SugarColumn(ColumnName = "LAB_ID", ColumnDataType = "RAW(16)")] public byte[] LabId { get; set; } = [];
    [SugarColumn(ColumnName = "SCENE_NAME", Length = 100)] public string SceneName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "MODEL_URL", Length = 500, IsNullable = true)] public string? ModelUrl { get; set; }
    [SugarColumn(ColumnName = "MODEL_FILE_ID", ColumnDataType = "RAW(16)", IsNullable = true)] public byte[]? ModelFileId { get; set; }
    [SugarColumn(ColumnName = "VERSION")] public int Version { get; set; } = 1;
    [SugarColumn(ColumnName = "BACKGROUND_COLOR", Length = 20)] public string BackgroundColor { get; set; } = "#eef3f5";
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>三维场景中的空间或设备节点。</summary>
[SugarTable("HXS_LAB_3D_NODE")]
public sealed class Lab3dNode : LabEntityBase
{
    [SugarColumn(ColumnName = "SCENE_ID", ColumnDataType = "RAW(16)")] public byte[] SceneId { get; set; } = [];
    [SugarColumn(ColumnName = "NODE_CODE", Length = 80)] public string NodeCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "NODE_NAME", Length = 100)] public string NodeName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "NODE_TYPE", Length = 30)] public string NodeType { get; set; } = "instrument";
    [SugarColumn(ColumnName = "POSITION_X")] public decimal PositionX { get; set; }
    [SugarColumn(ColumnName = "POSITION_Y")] public decimal PositionY { get; set; }
    [SugarColumn(ColumnName = "POSITION_Z")] public decimal PositionZ { get; set; }
    [SugarColumn(ColumnName = "SCALE_X")] public decimal ScaleX { get; set; } = 1;
    [SugarColumn(ColumnName = "SCALE_Y")] public decimal ScaleY { get; set; } = 1;
    [SugarColumn(ColumnName = "SCALE_Z")] public decimal ScaleZ { get; set; } = 1;
    [SugarColumn(ColumnName = "SORT_NO")] public int SortNo { get; set; }
}

/// <summary>三维节点与业务数据的绑定关系。</summary>
[SugarTable("HXS_LAB_3D_BINDING")]
public sealed class Lab3dBinding : LabEntityBase
{
    [SugarColumn(ColumnName = "NODE_ID", ColumnDataType = "RAW(16)")] public byte[] NodeId { get; set; } = [];
    [SugarColumn(ColumnName = "BUSINESS_TYPE", Length = 30)] public string BusinessType { get; set; } = "instrument";
    [SugarColumn(ColumnName = "BUSINESS_ID", ColumnDataType = "RAW(16)")] public byte[] BusinessId { get; set; } = [];
}
