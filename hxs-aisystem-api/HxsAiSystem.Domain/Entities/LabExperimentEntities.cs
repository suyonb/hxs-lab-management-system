using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>实验任务及归档状态。</summary>
[SugarTable("HXS_LAB_EXPERIMENT")]
public sealed class LabExperiment : LabEntityBase
{
    [SugarColumn(ColumnName = "EXPERIMENT_NO", Length = 40)] public string ExperimentNo { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "EXPERIMENT_NAME", Length = 150)] public string ExperimentName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "GROUP_ID", IsNullable = true)] public byte[]? GroupId { get; set; }
    [SugarColumn(ColumnName = "OWNER_ID")] public byte[] OwnerId { get; set; } = [];
    [SugarColumn(ColumnName = "TOPIC_NAME", Length = 150, IsNullable = true)] public string? TopicName { get; set; }
    [SugarColumn(ColumnName = "PURPOSE", Length = 1000)] public string Purpose { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "STATUS", Length = 20)] public string Status { get; set; } = "draft";
    [SugarColumn(ColumnName = "START_TIME", IsNullable = true)] public DateTime? StartTime { get; set; }
    [SugarColumn(ColumnName = "END_TIME", IsNullable = true)] public DateTime? EndTime { get; set; }
    [SugarColumn(ColumnName = "ARCHIVE_USER_ID", IsNullable = true)] public byte[]? ArchiveUserId { get; set; }
    [SugarColumn(ColumnName = "ARCHIVE_TIME", IsNullable = true)] public DateTime? ArchiveTime { get; set; }
}

/// <summary>实验关联仪器及已通过预约。</summary>
[SugarTable("HXS_LAB_EXPERIMENT_INSTRUMENT")]
public sealed class LabExperimentInstrument
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")] public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "EXPERIMENT_ID")] public byte[] ExperimentId { get; set; } = [];
    [SugarColumn(ColumnName = "INSTRUMENT_ID")] public byte[] InstrumentId { get; set; } = [];
    [SugarColumn(ColumnName = "BOOKING_ID", IsNullable = true)] public byte[]? BookingId { get; set; }
}

/// <summary>实验关联物资及已通过领用单。</summary>
[SugarTable("HXS_LAB_EXPERIMENT_MATERIAL")]
public sealed class LabExperimentMaterial
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")] public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "EXPERIMENT_ID")] public byte[] ExperimentId { get; set; } = [];
    [SugarColumn(ColumnName = "MATERIAL_ID")] public byte[] MaterialId { get; set; } = [];
    [SugarColumn(ColumnName = "REQUISITION_ID", IsNullable = true)] public byte[]? RequisitionId { get; set; }
    [SugarColumn(ColumnName = "QUANTITY")] public decimal Quantity { get; set; }
}

/// <summary>实验过程文字、结果或原始数据说明。</summary>
[SugarTable("HXS_LAB_EXPERIMENT_RECORD")]
public sealed class LabExperimentRecord
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")] public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "EXPERIMENT_ID")] public byte[] ExperimentId { get; set; } = [];
    [SugarColumn(ColumnName = "RECORD_TYPE", Length = 30)] public string RecordType { get; set; } = "process";
    [SugarColumn(ColumnName = "CONTENT", Length = 4000)] public string Content { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "RECORD_TIME")] public DateTime RecordTime { get; set; }
    [SugarColumn(ColumnName = "CREATOR_ID")] public byte[] CreatorId { get; set; } = [];
}
