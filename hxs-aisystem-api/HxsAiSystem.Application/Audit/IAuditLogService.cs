using HxsAiSystem.Application.Common;

namespace HxsAiSystem.Application.Audit;

public interface IAuditLogService
{
    Task<PageResult<AuditLogDto>> GetPageAsync(int pageIndex, int pageSize, string? keyword, DateTime? startTime, DateTime? endTime);
}
