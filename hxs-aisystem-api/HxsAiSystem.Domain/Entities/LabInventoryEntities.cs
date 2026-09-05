using SqlSugar;

namespace HxsAiSystem.Domain.Entities;

/// <summary>试剂或耗材档案。</summary>
[SugarTable("HXS_LAB_MATERIAL")]
public sealed class LabMaterial : LabEntityBase
{
    [SugarColumn(ColumnName = "MATERIAL_CODE", Length = 50)] public string MaterialCode { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "MATERIAL_NAME", Length = 150)] public string MaterialName { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "MATERIAL_TYPE", Length = 20)] public string MaterialType { get; set; } = "reagent";
    [SugarColumn(ColumnName = "CATEGORY_ID", IsNullable = true)] public byte[]? CategoryId { get; set; }
    [SugarColumn(ColumnName = "SPECIFICATION", Length = 100, IsNullable = true)] public string? Specification { get; set; }
    [SugarColumn(ColumnName = "CAS_NO", Length = 80, IsNullable = true)] public string? CasNo { get; set; }
    [SugarColumn(ColumnName = "UNIT_ID")] public byte[] UnitId { get; set; } = [];
    [SugarColumn(ColumnName = "SUPPLIER_ID", IsNullable = true)] public byte[]? SupplierId { get; set; }
    [SugarColumn(ColumnName = "STORAGE_LOCATION_ID")] public byte[] StorageLocationId { get; set; } = [];
    [SugarColumn(ColumnName = "MIN_STOCK")] public decimal MinStock { get; set; }
    [SugarColumn(ColumnName = "DESCRIPTION", Length = 500, IsNullable = true)] public string? Description { get; set; }
    [SugarColumn(ColumnName = "IS_ACTIVE")] public int IsActive { get; set; }
}

/// <summary>物资入库批次及当前可用库存。</summary>
[SugarTable("HXS_LAB_STOCK_BATCH")]
public sealed class LabStockBatch : LabEntityBase
{
    [SugarColumn(ColumnName = "MATERIAL_ID")] public byte[] MaterialId { get; set; } = [];
    [SugarColumn(ColumnName = "BATCH_NO", Length = 80)] public string BatchNo { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "PRODUCTION_DATE", IsNullable = true)] public DateTime? ProductionDate { get; set; }
    [SugarColumn(ColumnName = "EXPIRY_DATE", IsNullable = true)] public DateTime? ExpiryDate { get; set; }
    [SugarColumn(ColumnName = "IN_QUANTITY")] public decimal InQuantity { get; set; }
    [SugarColumn(ColumnName = "AVAILABLE_QUANTITY")] public decimal AvailableQuantity { get; set; }
    [SugarColumn(ColumnName = "UNIT_PRICE", IsNullable = true)] public decimal? UnitPrice { get; set; }
    [SugarColumn(ColumnName = "STOCK_IN_TIME")] public DateTime StockInTime { get; set; }
}

/// <summary>库存数量变化流水。</summary>
[SugarTable("HXS_LAB_STOCK_FLOW")]
public sealed class LabStockFlow
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")] public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "FLOW_NO", Length = 40)] public string FlowNo { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "MATERIAL_ID")] public byte[] MaterialId { get; set; } = [];
    [SugarColumn(ColumnName = "BATCH_ID")] public byte[] BatchId { get; set; } = [];
    [SugarColumn(ColumnName = "FLOW_TYPE", Length = 20)] public string FlowType { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "QUANTITY")] public decimal Quantity { get; set; }
    [SugarColumn(ColumnName = "BEFORE_QUANTITY")] public decimal BeforeQuantity { get; set; }
    [SugarColumn(ColumnName = "AFTER_QUANTITY")] public decimal AfterQuantity { get; set; }
    [SugarColumn(ColumnName = "SOURCE_TYPE", Length = 30)] public string SourceType { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "SOURCE_ID", IsNullable = true)] public byte[]? SourceId { get; set; }
    [SugarColumn(ColumnName = "OPERATOR_ID")] public byte[] OperatorId { get; set; } = [];
    [SugarColumn(ColumnName = "REMARK", Length = 500, IsNullable = true)] public string? Remark { get; set; }
    [SugarColumn(ColumnName = "CREATE_TIME")] public DateTime CreateTime { get; set; }
}

/// <summary>物资领用申请及审批信息。</summary>
[SugarTable("HXS_LAB_REQUISITION")]
public sealed class LabRequisition : LabEntityBase
{
    [SugarColumn(ColumnName = "REQUISITION_NO", Length = 40)] public string RequisitionNo { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "APPLICANT_ID")] public byte[] ApplicantId { get; set; } = [];
    [SugarColumn(ColumnName = "GROUP_ID", IsNullable = true)] public byte[]? GroupId { get; set; }
    [SugarColumn(ColumnName = "PURPOSE", Length = 500)] public string Purpose { get; set; } = string.Empty;
    [SugarColumn(ColumnName = "STATUS", Length = 20)] public string Status { get; set; } = "pending";
    [SugarColumn(ColumnName = "APPROVER_ID", IsNullable = true)] public byte[]? ApproverId { get; set; }
    [SugarColumn(ColumnName = "APPROVE_TIME", IsNullable = true)] public DateTime? ApproveTime { get; set; }
    [SugarColumn(ColumnName = "APPROVE_REMARK", Length = 500, IsNullable = true)] public string? ApproveRemark { get; set; }
}

/// <summary>领用申请物资明细。</summary>
[SugarTable("HXS_LAB_REQUISITION_ITEM")]
public sealed class LabRequisitionItem
{
    [SugarColumn(IsPrimaryKey = true, ColumnName = "ID")] public byte[] Id { get; set; } = [];
    [SugarColumn(ColumnName = "REQUISITION_ID")] public byte[] RequisitionId { get; set; } = [];
    [SugarColumn(ColumnName = "MATERIAL_ID")] public byte[] MaterialId { get; set; } = [];
    [SugarColumn(ColumnName = "REQUEST_QUANTITY")] public decimal RequestQuantity { get; set; }
    [SugarColumn(ColumnName = "APPROVED_QUANTITY", IsNullable = true)] public decimal? ApprovedQuantity { get; set; }
}
