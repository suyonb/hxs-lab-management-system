using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.Audit;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ISqlSugarClient _db;
    public AuditLogService(ISqlSugarClient db) => _db = db;

    public async Task<PageResult<AuditLogDto>> GetPageAsync(int pageIndex, int pageSize, string? keyword, DateTime? startTime, DateTime? endTime)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var query = _db.Queryable<SysAuditLog>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var value = keyword.Trim();
            query = query.Where(x => x.UserName!.Contains(value) || x.ModuleCode.Contains(value) || x.ActionCode.Contains(value));
        }
        if (startTime.HasValue) query = query.Where(x => x.CreateTime >= startTime.Value);
        if (endTime.HasValue) query = query.Where(x => x.CreateTime <= endTime.Value);
        RefAsync<int> total = 0;
        var rows = await query.OrderBy(x => x.CreateTime, OrderByType.Desc).ToPageListAsync(pageIndex, pageSize, total);
        return new PageResult<AuditLogDto>
        {
            PageIndex = pageIndex, PageSize = pageSize, Total = total,
            Items = rows.Select(x => new AuditLogDto(RawGuidConverter.ToGuid(x.Id), x.UserId is null ? null : RawGuidConverter.ToGuid(x.UserId),
                x.UserName, x.ModuleCode, x.ActionCode, x.BusinessId, x.RequestPath, x.HttpMethod, x.Result, x.IpAddress, x.CreateTime)).ToList()
        };
    }
}
