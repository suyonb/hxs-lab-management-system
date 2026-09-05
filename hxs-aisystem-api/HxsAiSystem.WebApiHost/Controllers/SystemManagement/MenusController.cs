using HxsAiSystem.Application.SystemManagement;
using HxsAiSystem.WebApiHost.Filters;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers.SystemManagement;

[ApiController]
[PermissionAuthorize("sys:menu:list")]
[Route("api/system/menus")]
public class MenusController : ControllerBase
{
    private readonly ISystemManagementService _service;

    public MenusController(ISystemManagementService service)
    {
        _service = service;
    }

    /// <summary>查询全部菜单和权限点列表。</summary>
    [HttpGet]
    public Task<List<MenuDto>> GetList() => _service.GetMenusAsync();

    /// <summary>以树形结构查询全部菜单和权限点。</summary>
    [HttpGet("tree")]
    public Task<List<MenuTreeDto>> GetTree() => _service.GetMenuTreeAsync();

    /// <summary>创建菜单、目录或按钮权限点。</summary>
    [HttpPost]
    [PermissionAuthorize("sys:menu:create")]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
    {
        try { return Ok(await _service.CreateMenuAsync(request)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>修改指定菜单或权限点。</summary>
    [HttpPut("{id:guid}")]
    [PermissionAuthorize("sys:menu:edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMenuRequest request)
    {
        try { return await _service.UpdateMenuAsync(id, request) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>删除没有子节点的指定菜单或权限点。</summary>
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("sys:menu:delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { return await _service.DeleteMenuAsync(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
