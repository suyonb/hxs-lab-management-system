namespace HxsAiSystem.Application.LabInventory;

public sealed record MaterialDto(Guid Id, string MaterialCode, string MaterialName, string MaterialType, Guid? CategoryId, string? CategoryName, string? Specification, string? CasNo, Guid UnitId, string? UnitName, Guid? SupplierId, string? SupplierName, Guid StorageLocationId, string? StorageLocationName, decimal MinStock, decimal CurrentStock, string? Description, bool IsActive);
public sealed record StockBatchDto(Guid Id, Guid MaterialId, string? MaterialName, string BatchNo, DateTime? ProductionDate, DateTime? ExpiryDate, decimal InQuantity, decimal AvailableQuantity, decimal? UnitPrice, DateTime StockInTime, string WarningStatus);
public sealed record StockFlowDto(Guid Id, string FlowNo, Guid MaterialId, string? MaterialName, Guid BatchId, string? BatchNo, string FlowType, decimal Quantity, decimal BeforeQuantity, decimal AfterQuantity, string SourceType, Guid? SourceId, Guid OperatorId, string? OperatorName, string? Remark, DateTime CreateTime);
public sealed record RequisitionItemDto(Guid Id, Guid MaterialId, string? MaterialName, string? UnitName, decimal RequestQuantity, decimal? ApprovedQuantity);
public sealed record RequisitionDto(Guid Id, string RequisitionNo, Guid ApplicantId, string? ApplicantName, Guid? GroupId, string? GroupName, string Purpose, string Status, Guid? ApproverId, string? ApproverName, DateTime? ApproveTime, string? ApproveRemark, DateTime CreateTime, List<RequisitionItemDto> Items);
public sealed record InventoryWarningDto(Guid MaterialId, string MaterialCode, string MaterialName, decimal CurrentStock, decimal MinStock, int ExpiringBatchCount, int ExpiredBatchCount, string WarningStatus);

public sealed class MaterialRequest { public string MaterialCode { get; set; } = ""; public string MaterialName { get; set; } = ""; public string MaterialType { get; set; } = "reagent"; public Guid? CategoryId { get; set; } public string? Specification { get; set; } public string? CasNo { get; set; } public Guid UnitId { get; set; } public Guid? SupplierId { get; set; } public Guid StorageLocationId { get; set; } public decimal MinStock { get; set; } public string? Description { get; set; } public bool IsActive { get; set; } = true; }
public sealed class StockInRequest { public Guid MaterialId { get; set; } public string BatchNo { get; set; } = ""; public DateTime? ProductionDate { get; set; } public DateTime? ExpiryDate { get; set; } public decimal Quantity { get; set; } public decimal? UnitPrice { get; set; } public string? Remark { get; set; } }
public sealed class StockAdjustRequest { public decimal Quantity { get; set; } public string Reason { get; set; } = ""; }
public sealed class RequisitionItemRequest { public Guid MaterialId { get; set; } public decimal Quantity { get; set; } }
public sealed class RequisitionRequest { public Guid? GroupId { get; set; } public string Purpose { get; set; } = ""; public List<RequisitionItemRequest> Items { get; set; } = []; }
public sealed class RequisitionApprovalItemRequest { public Guid ItemId { get; set; } public decimal ApprovedQuantity { get; set; } }
public sealed class RequisitionApprovalRequest { public string? Remark { get; set; } public List<RequisitionApprovalItemRequest> Items { get; set; } = []; }
public sealed class RequisitionDecisionRequest { public string? Remark { get; set; } }
