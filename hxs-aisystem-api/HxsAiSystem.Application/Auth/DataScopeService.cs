using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.Auth;

public sealed class DataScopeService : IDataScopeService
{
    private readonly ISqlSugarClient _db;
    private readonly ICurrentUserService _currentUser;

    public DataScopeService(ISqlSugarClient db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DataScope> GetCurrentScopeAsync()
    {
        var userId = _currentUser.GetUserId();
        if (!userId.HasValue) return DataScope.Self;
        var raw = RawGuidConverter.ToRaw(userId.Value);
        var roleCodes = await _db.Queryable<SysUserRole, SysRole>((ur, role) => ur.RoleId == role.Id)
            .Where((ur, role) => ur.UserId == raw && role.IsActive == 1)
            .Select((ur, role) => role.RoleCode)
            .ToListAsync();
        if (roleCodes.Contains("admin")) return DataScope.All;
        if (roleCodes.Contains("lab_admin")) return DataScope.Laboratory;
        return DataScope.Self;
    }
}
