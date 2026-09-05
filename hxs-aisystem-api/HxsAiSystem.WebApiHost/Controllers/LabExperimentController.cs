using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.LabExperiment;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[Route("api/lab/experiments")]
public sealed class LabExperimentController : ControllerBase
{
    private readonly ILabExperimentService _service;
    public LabExperimentController(ILabExperimentService service)=>_service=service;

    /// <summary>按权限范围查询实验任务，支持关键字、状态和创建时间筛选。</summary>
    [HttpGet,PermissionAuthorize("lab:experiment:view")]
    public Task<List<ExperimentDto>> Get([FromQuery]bool mine=false,[FromQuery]string? keyword=null,[FromQuery]string? status=null,[FromQuery]DateTime? startTime=null,[FromQuery]DateTime? endTime=null)=>_service.GetAsync(mine,keyword,status,startTime,endTime);
    /// <summary>查询实验详情、关联仪器物资、过程记录及附件。</summary>
    [HttpGet("{id:guid}"),PermissionAuthorize("lab:experiment:view")] public Task<ExperimentDto> GetById(Guid id)=>_service.GetByIdAsync(id);
    /// <summary>新建草稿实验并关联已通过的预约和领用单。</summary>
    [HttpPost,PermissionAuthorize("lab:experiment:create")] public Task<ExperimentDto> Create(ExperimentRequest request)=>_service.CreateAsync(request);
    /// <summary>编辑本人草稿实验的基本信息及关联数据。</summary>
    [HttpPut("{id:guid}"),PermissionAuthorize("lab:experiment:edit")] public async Task<IActionResult> Update(Guid id,ExperimentRequest request){await _service.UpdateAsync(id,request);return NoContent();}
    /// <summary>将草稿实验开始为进行中状态。</summary>
    [HttpPost("{id:guid}/start"),PermissionAuthorize("lab:experiment:edit")] public async Task<IActionResult> Start(Guid id){await _service.StartAsync(id);return NoContent();}
    /// <summary>完成进行中的实验，完成后停止增加普通过程记录。</summary>
    [HttpPost("{id:guid}/complete"),PermissionAuthorize("lab:experiment:edit")] public async Task<IActionResult> Complete(Guid id){await _service.CompleteAsync(id);return NoContent();}
    /// <summary>填写原因后将已完成实验退回进行中。</summary>
    [HttpPost("{id:guid}/reopen"),PermissionAuthorize("lab:experiment:edit")] public async Task<IActionResult> Reopen(Guid id,ExperimentReasonRequest request){await _service.ReopenAsync(id,request);return NoContent();}
    /// <summary>归档已完成实验，归档后进入只读状态。</summary>
    [HttpPost("{id:guid}/archive"),PermissionAuthorize("lab:experiment:archive")] public async Task<IActionResult> Archive(Guid id){await _service.ArchiveAsync(id);return NoContent();}
    /// <summary>仅系统管理员可填写原因并解档实验。</summary>
    [HttpPost("{id:guid}/unarchive"),PermissionAuthorize("lab:experiment:unarchive")] public async Task<IActionResult> Unarchive(Guid id,ExperimentReasonRequest request){await _service.UnarchiveAsync(id,request);return NoContent();}
    /// <summary>为进行中的实验增加过程、结果或原始数据说明。</summary>
    [HttpPost("{id:guid}/records"),PermissionAuthorize("lab:experiment:record")] public Task<ExperimentRecordDto> AddRecord(Guid id,ExperimentRecordRequest request)=>_service.AddRecordAsync(id,request);
}
