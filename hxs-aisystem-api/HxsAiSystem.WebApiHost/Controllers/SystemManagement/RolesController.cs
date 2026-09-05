using HxsAiSystem.Application.SystemManagement;
using HxsAiSystem.WebApiHost.Filters;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers.SystemManagement;

[ApiController]
[PermissionAuthorize("sys:role:list")]
[Route("api/system/roles")]
public class RolesController : ControllerBase
{
    private readonly ISystemManagementService _service;

    public RolesController(ISystemManagementService service)
    {
        _service = service;
    }

    /// <summary>查询系统角色列表。</summary>
    [HttpGet]
    public Task<List<RoleDto>> GetList() => _service.GetRolesAsync();

    /// <summary>创建新的系统角色。</summary>
    [HttpPost]
    [PermissionAuthorize("sys:role:create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        try { return Ok(await _service.CreateRoleAsync(request)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>修改指定角色的名称、描述或状态。</summary>
    [HttpPut("{id:guid}")]
    [PermissionAuthorize("sys:role:edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        try { return await _service.UpdateRoleAsync(id, request) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>删除指定角色及其用户和菜单关联。</summary>
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("sys:role:delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteRoleAsync(id) ? NoContent() : NotFound();
    }

    /// <summary>查询指定角色已分配的菜单和权限。</summary>
    [HttpGet("{id:guid}/menus")]
    public Task<List<MenuDto>> GetMenus(Guid id) => _service.GetRoleMenusAsync(id);

    /// <summary>重新分配指定角色拥有的菜单和权限。</summary>
    [HttpPut("{id:guid}/menus")]
    [PermissionAuthorize("sys:role:assign-menu")]
    public async Task<IActionResult> AssignMenus(Guid id, [FromBody] AssignMenusRequest request)
    {
        await _service.AssignRoleMenusAsync(id, request.MenuIds);
        return NoContent();
    }
}
