namespace HxsAiSystem.Application.Files;

public sealed record FileRecordDto(Guid Id, string BusinessType, string? BusinessId, string OriginalName,
    string ContentType, long FileSize, Guid UploaderId, DateTime CreateTime);

public sealed record FileDownload(string FullPath, string OriginalName, string ContentType);
