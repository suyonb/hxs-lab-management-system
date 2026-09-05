using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.LabInventory;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[Route("api/lab/inventory")]
public sealed class LabInventoryController : ControllerBase
{
    private readonly ILabInventoryService _service; public LabInventoryController(ILabInventoryService service)=>_service=service;
    /// <summary>查询试剂耗材档案及当前汇总库存。</summary>
    [HttpGet("materials"),PermissionAuthorize("lab:inventory:view")] public Task<List<MaterialDto>> Materials([FromQuery]bool enabledOnly=false)=>_service.GetMaterialsAsync(enabledOnly);
    /// <summary>新增试剂或耗材档案并校验编码、单位和存放位置。</summary>
    [HttpPost("materials"),PermissionAuthorize("lab:material:manage")] public Task<MaterialDto> CreateMaterial(MaterialRequest request)=>_service.CreateMaterialAsync(request);
    /// <summary>修改试剂耗材档案，物资编码保持不变。</summary>
    [HttpPut("materials/{id:guid}"),PermissionAuthorize("lab:material:manage")] public async Task<IActionResult> UpdateMaterial(Guid id,MaterialRequest request){await _service.UpdateMaterialAsync(id,request);return NoContent();}
    /// <summary>查询入库批次、可用数量和临期过期状态。</summary>
    [HttpGet("batches"),PermissionAuthorize("lab:inventory:view")] public Task<List<StockBatchDto>> Batches([FromQuery]Guid? materialId=null)=>_service.GetBatchesAsync(materialId);
    /// <summary>登记物资入库批次并同时生成入库流水。</summary>
    [HttpPost("batches"),PermissionAuthorize("lab:stock:in")] public Task<StockBatchDto> StockIn(StockInRequest request)=>_service.StockInAsync(request);
    /// <summary>调整指定批次库存，禁止负库存并强制填写原因。</summary>
    [HttpPost("batches/{id:guid}/adjust"),PermissionAuthorize("lab:stock:adjust")] public async Task<IActionResult> Adjust(Guid id,StockAdjustRequest request){await _service.AdjustStockAsync(id,request);return NoContent();}
    /// <summary>查询入库、领用和调整产生的完整库存流水。</summary>
    [HttpGet("flows"),PermissionAuthorize("lab:inventory:view")] public Task<List<StockFlowDto>> Flows([FromQuery]Guid? materialId=null)=>_service.GetFlowsAsync(materialId);
    /// <summary>查询领用申请；普通实验员仅能查看本人申请。</summary>
    [HttpGet("requisitions"),PermissionAuthorize("lab:requisition:view")] public Task<List<RequisitionDto>> Requisitions([FromQuery]bool mine=false,[FromQuery]string? status=null)=>_service.GetRequisitionsAsync(mine,status);
    /// <summary>提交包含一条或多条物资明细的领用申请。</summary>
    [HttpPost("requisitions"),PermissionAuthorize("lab:requisition:create")] public Task<RequisitionDto> CreateRequisition(RequisitionRequest request)=>_service.CreateRequisitionAsync(request);
    /// <summary>取消本人尚未审批的领用申请。</summary>
    [HttpPost("requisitions/{id:guid}/cancel"),PermissionAuthorize("lab:requisition:cancel")] public async Task<IActionResult> Cancel(Guid id){await _service.CancelRequisitionAsync(id);return NoContent();}
    /// <summary>事务审批领用申请，按有效期优先扣减批次并生成流水。</summary>
    [HttpPost("requisitions/{id:guid}/approve"),PermissionAuthorize("lab:requisition:approve")] public async Task<IActionResult> Approve(Guid id,RequisitionApprovalRequest request){await _service.ApproveRequisitionAsync(id,request);return NoContent();}
    /// <summary>驳回待审核领用申请并记录审批意见。</summary>
    [HttpPost("requisitions/{id:guid}/reject"),PermissionAuthorize("lab:requisition:approve")] public async Task<IActionResult> Reject(Guid id,RequisitionDecisionRequest request){await _service.RejectRequisitionAsync(id,request);return NoContent();}
    /// <summary>查询低库存、临期及过期库存预警。</summary>
    [HttpGet("warnings"),PermissionAuthorize("lab:inventory:view")] public Task<List<InventoryWarningDto>> Warnings([FromQuery]int expiryDays=30)=>_service.GetWarningsAsync(expiryDays);
}
