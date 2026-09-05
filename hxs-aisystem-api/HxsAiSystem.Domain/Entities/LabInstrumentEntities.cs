using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>实验室仪器设备台账。</summary>
[SugarTable("HXS_LAB_INSTRUMENT")]
public sealed class LabInstrument : LabEntityBase
{
    [SugarColumn(ColumnName = "INSTRUMENT_CODE", Length = 50)] public string InstrumentCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "INSTRUMENT_NAME", Length = 150)] public string InstrumentName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "CATEGORY_ID", IsNullable = true)] public byte[]? CategoryId { get; set; }
    [SugarColumn(ColumnName = "MODEL", Length = 100, IsNullable = true)] public string? Model { get; set; }
    [SugarColumn(ColumnName = "MANUFACTURER", Length = 150, IsNullable = true)] public string? Manufacturer { get; set; }
    [SugarColumn(ColumnName = "SUPPLIER_ID", IsNullable = true)] public byte[]? SupplierId { get; set; }
    [SugarColumn(ColumnName = "LAB_ID")] public byte[] LabId { get; set; } = [];
    [SugarColumn(ColumnName = "LOCATION_ID")] public byte[] LocationId { get; set; } = [];
    [SugarColumn(ColumnName = "STATUS", Length = 20)] public string Status { get; set; } = "normal";
    [SugarColumn(ColumnName = "DESCRIPTION", Length = 500, IsNullable = true)] public string? Description { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>仪器预约申请及审批记录。</summary>
[SugarTable("HXS_LAB_BOOKING")]
public sealed class LabBooking : LabEntityBase
{
    [SugarColumn(ColumnName = "BOOKING_NO", Length = 40)] public string BookingNo { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "INSTRUMENT_ID")] public byte[] InstrumentId { get; set; } = [];
    [SugarColumn(ColumnName = "APPLICANT_ID")] public byte[] ApplicantId { get; set; } = [];
    [SugarColumn(ColumnName = "GROUP_ID", IsNullable = true)] public byte[]? GroupId { get; set; }
    [SugarColumn(ColumnName = "START_TIME")] public DateTime StartTime { get; set; }
    [SugarColumn(ColumnName = "END_TIME")] public DateTime EndTime { get; set; }
    [SugarColumn(ColumnName = "PURPOSE", Length = 500)] public string Purpose { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "STATUS", Length = 20)] public string Status { get; set; } = "pending";
    [SugarColumn(ColumnName = "APPROVER_ID", IsNullable = true)] public byte[]? ApproverId { get; set; }
    [SugarColumn(ColumnName = "APPROVE_TIME", IsNullable = true)] public DateTime? ApproveTime { get; set; }
    [SugarColumn(ColumnName = "APPROVE_REMARK", Length = 500, IsNullable = true)] public string? ApproveRemark { get; set; }
    [SugarColumn(ColumnName = "CANCEL_TIME", IsNullable = true)] public DateTime? CancelTime { get; set; }
}

/// <summary>仪器实际使用记录。</summary>
[SugarTable("HXS_LAB_USAGE")]
public sealed class LabUsage : LabEntityBase
{
    [SugarColumn(ColumnName = "INSTRUMENT_ID")] public byte[] InstrumentId { get; set; } = [];
    [SugarColumn(ColumnName = "BOOKING_ID", IsNullable = true)] public byte[]? BookingId { get; set; }
    [SugarColumn(ColumnName = "USER_ID")] public byte[] UserId { get; set; } = [];
    [SugarColumn(ColumnName = "START_TIME")] public DateTime StartTime { get; set; }
    [SugarColumn(ColumnName = "END_TIME")] public DateTime EndTime { get; set; }
    [SugarColumn(ColumnName = "EXPERIMENT_CONTENT", Length = 1000)] public string ExperimentContent { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "REMARK", Length = 500, IsNullable = true)] public string? Remark { get; set; }
}

/// <summary>仪器故障报修及维修处理记录。</summary>
[SugarTable("HXS_LAB_REPAIR")]
public sealed class LabRepair : LabEntityBase
{
    [SugarColumn(ColumnName = "REPAIR_NO", Length = 40)] public string RepairNo { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "INSTRUMENT_ID")] public byte[] InstrumentId { get; set; } = [];
    [SugarColumn(ColumnName = "REPORTER_ID")] public byte[] ReporterId { get; set; } = [];
    [SugarColumn(ColumnName = "FAULT_DESCRIPTION", Length = 1000)] public string FaultDescription { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "STATUS", Length = 20)] public string Status { get; set; } = "pending";
    [SugarColumn(ColumnName = "APPROVER_ID", IsNullable = true)] public byte[]? ApproverId { get; set; }
    [SugarColumn(ColumnName = "APPROVE_TIME", IsNullable = true)] public DateTime? ApproveTime { get; set; }
    [SugarColumn(ColumnName = "REPAIRER", Length = 100, IsNullable = true)] public string? Repairer { get; set; }
    [SugarColumn(ColumnName = "REPAIR_CONTENT", Length = 1000, IsNullable = true)] public string? RepairContent { get; set; }
    [SugarColumn(ColumnName = "REPAIR_START_TIME", IsNullable = true)] public DateTime? RepairStartTime { get; set; }
    [SugarColumn(ColumnName = "REPAIR_END_TIME", IsNullable = true)] public DateTime? RepairEndTime { get; set; }
    [SugarColumn(ColumnName = "REMARK", Length = 500, IsNullable = true)] public string? Remark { get; set; }
}
