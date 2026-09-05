using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.SystemManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

/// <summary>认证接口。</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemManagementService _systemManagementService;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        ISystemManagementService systemManagementService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _systemManagementService = systemManagementService;
    }

    /// <summary>使用用户名和密码登录并获取访问令牌。</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return response is null ? Unauthorized(new { message = "用户名或密码错误。" }) : Ok(response);
        }
        catch (AccountLockedException ex)
        {
            return StatusCode(StatusCodes.Status423Locked, new { message = ex.Message });
        }
    }

    /// <summary>获取当前登录用户有权查看的菜单树。</summary>
    [HttpGet("menus")]
    [Authorize]
    public async Task<IActionResult> GetMenus()
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "登录已失效，请重新登录。" });

        return Ok(await _systemManagementService.GetUserMenuTreeAsync(userId.Value));
    }

    /// <summary>获取当前登录用户拥有的全部权限编码。</summary>
    [HttpGet("permissions")]
    [Authorize]
    public async Task<IActionResult> GetPermissions()
    {
        var userId = _currentUserService.GetUserId();
        return userId.HasValue
            ? Ok(await _systemManagementService.GetUserPermissionsAsync(userId.Value))
            : Unauthorized(new { message = "登录已失效，请重新登录。" });
    }

    /// <summary>退出当前登录会话。</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() => NoContent();
}
