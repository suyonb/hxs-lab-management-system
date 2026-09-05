using HxsAiSystem.Application.SystemManagement;
using HxsAiSystem.WebApiHost.Filters;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers.SystemManagement;

[ApiController]
[PermissionAuthorize("sys:user:list")]
[Route("api/system/users")]
public class UsersController : ControllerBase
{
    private readonly ISystemManagementService _service;

    public UsersController(ISystemManagementService service)
    {
        _service = service;
    }

    /// <summary>查询系统用户列表，可按用户名或显示名称搜索。</summary>
    [HttpGet]
    public Task<List<UserDto>> GetList([FromQuery] string? keyword) => _service.GetUsersAsync(keyword);

    /// <summary>创建新的系统用户。</summary>
    [HttpPost]
    [PermissionAuthorize("sys:user:create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try { return Ok(await _service.CreateUserAsync(request)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>修改指定用户的资料、状态或密码。</summary>
    [HttpPut("{id:guid}")]
    [PermissionAuthorize("sys:user:edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        try { return await _service.UpdateUserAsync(id, request) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>删除指定系统用户及其角色关联。</summary>
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("sys:user:delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteUserAsync(id) ? NoContent() : NotFound();
    }

    /// <summary>查询指定用户当前拥有的角色。</summary>
    [HttpGet("{id:guid}/roles")]
    public Task<List<RoleDto>> GetRoles(Guid id) => _service.GetUserRolesAsync(id);

    /// <summary>重新分配指定用户拥有的角色。</summary>
    [HttpPut("{id:guid}/roles")]
    [PermissionAuthorize("sys:user:assign-role")]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request)
    {
        await _service.AssignUserRolesAsync(id, request.RoleIds);
        return NoContent();
    }
}
