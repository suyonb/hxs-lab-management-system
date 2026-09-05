namespace HxsAiSystem.Application.Files;

public interface IFileStorageService
{
    Task<FileRecordDto> SaveAsync(Stream stream, string originalName, string contentType, long fileSize,
        string businessType, string? businessId, CancellationToken cancellationToken = default);
    Task<FileDownload> GetDownloadAsync(Guid id);
    Task<FileDownload> GetBusinessDownloadAsync(Guid id,string businessType,string businessId);
}
