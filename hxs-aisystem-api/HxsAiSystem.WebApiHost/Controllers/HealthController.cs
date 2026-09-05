using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

/// <summary>服务健康检查。</summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>检查后端服务是否正常运行。</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", service = "hxs-aisystem-api" });
}
