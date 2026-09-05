using HxsAiSystem.Application.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[Authorize]
[Route("api/files")]
public sealed class FilesController : ControllerBase
{
    private readonly IFileStorageService _service;
    public FilesController(IFileStorageService service) => _service = service;

    /// <summary>上传业务附件并保存文件元数据。</summary>
    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string businessType, [FromForm] string? businessId,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await _service.SaveAsync(stream, file.FileName, file.ContentType, file.Length, businessType, businessId, cancellationToken));
    }

    /// <summary>校验访问权限后下载指定附件。</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        try
        {
            var file = await _service.GetDownloadAsync(id);
            return PhysicalFile(file.FullPath, file.ContentType, file.OriginalName, enableRangeProcessing: true);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
