using HxsAiSystem.Application.SystemManagement;
using HxsAiSystem.WebApiHost.Filters;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers.SystemManagement;

[ApiController]
[PermissionAuthorize("sys:org:list")]
[Route("api/system/orgs")]
public class OrgsController : ControllerBase
{
    private readonly ISystemManagementService _service;

    public OrgsController(ISystemManagementService service)
    {
        _service = service;
    }

    /// <summary>查询全部组织节点列表。</summary>
    [HttpGet]
    public Task<List<OrgDto>> GetList() => _service.GetOrgsAsync();

    /// <summary>以树形结构查询组织架构。</summary>
    [HttpGet("tree")]
    public Task<List<OrgTreeDto>> GetTree() => _service.GetOrgTreeAsync();

    /// <summary>创建新的组织、部门或小组节点。</summary>
    [HttpPost]
    [PermissionAuthorize("sys:org:create")]
    public async Task<IActionResult> Create([FromBody] CreateOrgRequest request)
    {
        try { return Ok(await _service.CreateOrgAsync(request)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>修改指定组织节点的信息和状态。</summary>
    [HttpPut("{id:guid}")]
    [PermissionAuthorize("sys:org:edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrgRequest request)
    {
        try { return await _service.UpdateOrgAsync(id, request) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>删除没有子节点和关联用户的组织节点。</summary>
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("sys:org:delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await _service.DeleteOrgAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
