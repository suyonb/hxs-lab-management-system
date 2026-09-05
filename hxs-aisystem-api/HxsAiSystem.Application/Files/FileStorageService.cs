using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace HxsAiSystem.Application.Files;

public sealed class FileStorageService : IFileStorageService
{
    private readonly ISqlSugarClient _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDataScopeService _dataScope;
    private readonly FileStorageOptions _options;
    private readonly string _rootPath;

    public FileStorageService(ISqlSugarClient db, ICurrentUserService currentUser, IDataScopeService dataScope,
        IOptions<FileStorageOptions> options, IHostEnvironment environment)
    {
        _db = db;
        _currentUser = currentUser;
        _dataScope = dataScope;
        _options = options.Value;
        _rootPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, _options.RootPath));
    }

    public async Task<FileRecordDto> SaveAsync(Stream stream, string originalName, string contentType, long fileSize,
        string businessType, string? businessId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetUserId() ?? throw new UnauthorizedAccessException();
        if (fileSize <= 0 || fileSize > _options.MaxFileSizeBytes) throw new InvalidOperationException("文件为空或超过大小限制。");
        var safeOriginalName = Path.GetFileName(originalName);
        var extension = Path.GetExtension(safeOriginalName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("不支持该文件类型。");
        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("文件内容类型不受支持。");
        if (string.IsNullOrWhiteSpace(businessType) || businessType.Length > 50) throw new InvalidOperationException("业务类型不合法。");
        if (string.Equals(businessType,"experiment",StringComparison.OrdinalIgnoreCase))
        {
            if(!Guid.TryParse(businessId,out var experimentId))throw new InvalidOperationException("实验附件必须关联有效实验。");
            var experimentRaw=RawGuidConverter.ToRaw(experimentId);var experiment=await _db.Queryable<HxsAiSystem.Domain.Entities.LabExperiment>().FirstAsync(x=>x.Id==experimentRaw)??throw new KeyNotFoundException("实验任务不存在。");
            if(experiment.Status=="archived")throw new InvalidOperationException("已归档实验不能上传附件。");
            if(!experiment.OwnerId.SequenceEqual(userId.ToByteArray())&&await _dataScope.GetCurrentScopeAsync()!=DataScope.All)throw new UnauthorizedAccessException("只能为本人实验上传附件。");
            var used=await _db.Queryable<SysFileRecord>().Where(x=>x.BusinessType=="experiment"&&x.BusinessId==businessId).SumAsync(x=>x.FileSize);
            if(used+fileSize>_options.MaxBusinessSizeBytes)throw new InvalidOperationException("该实验附件总大小超过限制。");
        }

        if(string.Equals(businessType,"lab-3d-model",StringComparison.OrdinalIgnoreCase))
        {
            if(!string.Equals(extension,".glb",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("三维场景仅支持 GLB 模型。");
            if(!Guid.TryParse(businessId,out var sceneId))throw new InvalidOperationException("三维模型必须关联有效场景。");
            if(await _dataScope.GetCurrentScopeAsync()==DataScope.Self)throw new UnauthorizedAccessException("仅实验管理员可以上传三维模型。");
            if(!await _db.Queryable<Lab3dScene>().AnyAsync(x=>x.Id==RawGuidConverter.ToRaw(sceneId)))throw new KeyNotFoundException("三维场景不存在。");
        }

        var datePath = DateTime.Now.ToString("yyyy/MM");
        var directory = Path.Combine(_rootPath, datePath);
        Directory.CreateDirectory(directory);
        var storageName = Guid.NewGuid().ToString("N") + extension;
        var fullPath = Path.Combine(directory, storageName);
        await using (var target = File.Create(fullPath))
            await stream.CopyToAsync(target, cancellationToken);

        var row = new SysFileRecord
        {
            Id = Guid.NewGuid().ToByteArray(), BusinessType = businessType.Trim(), BusinessId = businessId?.Trim(),
            OriginalName = safeOriginalName, StorageName = storageName, FilePath = Path.Combine(datePath, storageName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = fileSize, UploaderId = userId.ToByteArray(), CreateTime = DateTime.Now
        };
        try
        {
            await _db.Insertable(row).ExecuteCommandAsync();
        }
        catch
        {
            File.Delete(fullPath);
            throw;
        }
        return ToDto(row);
    }

    public async Task<FileDownload> GetDownloadAsync(Guid id)
    {
        var raw = RawGuidConverter.ToRaw(id);
        var row = await _db.Queryable<SysFileRecord>().FirstAsync(x => x.Id == raw) ?? throw new KeyNotFoundException("文件不存在。");
        var userId = _currentUser.GetUserId() ?? throw new UnauthorizedAccessException();
        var isOwner = RawGuidConverter.ToGuid(row.UploaderId) == userId;
        var canAccessExperiment=false;
        if(string.Equals(row.BusinessType,"experiment",StringComparison.OrdinalIgnoreCase)&&Guid.TryParse(row.BusinessId,out var experimentId))
        {
            var experiment=await _db.Queryable<HxsAiSystem.Domain.Entities.LabExperiment>().FirstAsync(x=>x.Id==RawGuidConverter.ToRaw(experimentId));
            canAccessExperiment=experiment is not null&&(experiment.OwnerId.SequenceEqual(userId.ToByteArray())||await _dataScope.GetCurrentScopeAsync()!=DataScope.Self);
        }
        if (!isOwner&&!canAccessExperiment&&await _dataScope.GetCurrentScopeAsync()!=DataScope.All) throw new UnauthorizedAccessException("无权访问该文件。");
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, row.FilePath));
        if (!fullPath.StartsWith(_rootPath, StringComparison.Ordinal) || !File.Exists(fullPath)) throw new KeyNotFoundException("文件不存在。");
        return new FileDownload(fullPath, row.OriginalName, row.ContentType);
    }

    public async Task<FileDownload> GetBusinessDownloadAsync(Guid id,string businessType,string businessId)
    {
        var row=await _db.Queryable<SysFileRecord>().FirstAsync(x=>x.Id==RawGuidConverter.ToRaw(id))??throw new KeyNotFoundException("文件不存在。");
        if(!string.Equals(row.BusinessType,businessType,StringComparison.OrdinalIgnoreCase)||row.BusinessId!=businessId)throw new UnauthorizedAccessException("文件与业务数据不匹配。");
        var fullPath=Path.GetFullPath(Path.Combine(_rootPath,row.FilePath));
        if(!fullPath.StartsWith(_rootPath,StringComparison.Ordinal)||!File.Exists(fullPath))throw new KeyNotFoundException("文件不存在。");
        return new FileDownload(fullPath,row.OriginalName,row.ContentType);
    }

    private static FileRecordDto ToDto(SysFileRecord x) => new(RawGuidConverter.ToGuid(x.Id), x.BusinessType, x.BusinessId,
        x.OriginalName, x.ContentType, x.FileSize, RawGuidConverter.ToGuid(x.UploaderId), x.CreateTime);
}
