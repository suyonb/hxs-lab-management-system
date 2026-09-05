namespace HxsAiSystem.Application.LabInventory;

public interface ILabInventoryService
{
    Task<List<MaterialDto>> GetMaterialsAsync(bool enabledOnly = false);
    Task<MaterialDto> CreateMaterialAsync(MaterialRequest request);
    Task UpdateMaterialAsync(Guid id, MaterialRequest request);
    Task<List<StockBatchDto>> GetBatchesAsync(Guid? materialId = null);
    Task<StockBatchDto> StockInAsync(StockInRequest request);
    Task AdjustStockAsync(Guid batchId, StockAdjustRequest request);
    Task<List<StockFlowDto>> GetFlowsAsync(Guid? materialId = null);
    Task<List<RequisitionDto>> GetRequisitionsAsync(bool mine = false, string? status = null);
    Task<RequisitionDto> CreateRequisitionAsync(RequisitionRequest request);
    Task CancelRequisitionAsync(Guid id);
    Task ApproveRequisitionAsync(Guid id, RequisitionApprovalRequest request);
    Task RejectRequisitionAsync(Guid id, RequisitionDecisionRequest request);
    Task<List<InventoryWarningDto>> GetWarningsAsync(int expiryDays = 30);
}
