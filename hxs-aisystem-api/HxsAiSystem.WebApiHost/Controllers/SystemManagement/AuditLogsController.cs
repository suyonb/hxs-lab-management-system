using HxsAiSystem.Application.Audit;
using HxsAiSystem.Application.Auth.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers.SystemManagement;

[ApiController]
[Route("api/system/audit-logs")]
[PermissionAuthorize("sys:audit:list")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;
    public AuditLogsController(IAuditLogService service) => _service = service;

    /// <summary>分页查询用户操作审计日志。</summary>
    [HttpGet]
    public Task<HxsAiSystem.Application.Common.PageResult<AuditLogDto>> GetPage(
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null,
        [FromQuery] DateTime? startTime = null, [FromQuery] DateTime? endTime = null) =>
        _service.GetPageAsync(pageIndex, pageSize, keyword, startTime, endTime);
}
