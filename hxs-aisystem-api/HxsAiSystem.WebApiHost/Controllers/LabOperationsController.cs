using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.LabOperations;
using Microsoft.AspNetCore.Mvc;
namespace HxsAiSystem.WebApiHost.Controllers;
[ApiController,Route("api/lab/operations")]
public sealed class LabOperationsController:ControllerBase
{
    private readonly ILabOperationsService _service;public LabOperationsController(ILabOperationsService service)=>_service=service;
    /// <summary>统一查询我的申请、待审批、已审批及取消驳回记录。</summary>
    [HttpGet("approvals"),PermissionAuthorize("lab:approval:center:view")]public Task<List<UnifiedApprovalDto>> Approvals([FromQuery]string view="mine",[FromQuery]string? businessType=null,[FromQuery]string? status=null,[FromQuery]string? keyword=null,[FromQuery]DateTime? startTime=null,[FromQuery]DateTime? endTime=null)=>_service.GetApprovalsAsync(view,businessType,status,keyword,startTime,endTime);
    /// <summary>获取实验室首页待办、预约、维修、库存、实验和趋势统计。</summary>
    [HttpGet("dashboard"),PermissionAuthorize("dashboard:view")]public Task<DashboardSummaryDto> Dashboard([FromQuery]int days=7)=>_service.GetDashboardAsync(days);
    /// <summary>按当前筛选和数据权限导出仪器、库存、预约、领用或实验数据，最多 5000 行。</summary>
    [HttpGet("exports/{type}"),PermissionAuthorize("lab:export")]public async Task<IActionResult> Export(string type,[FromQuery]string? keyword=null,[FromQuery]string? status=null,[FromQuery]DateTime? startTime=null,[FromQuery]DateTime? endTime=null){var file=await _service.ExportAsync(type,keyword,status,startTime,endTime);return File(file.Content,"application/vnd.ms-excel",file.FileName);}
}
