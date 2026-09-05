using System.Security.Claims;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;

namespace HxsAiSystem.Application.Auth.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ISqlSugarClient _db;

    public PermissionAuthorizationHandler(ISqlSugarClient db) => _db = db;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var value = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId)) return;
        var userRaw = RawGuidConverter.ToRaw(userId);

        var userActive = await _db.Queryable<AppUser>().AnyAsync(x => x.Id == userRaw && x.IsActive == 1);
        if (!userActive) return;

        var isAdmin = await _db.Queryable<SysUserRole, SysRole>((ur, role) => ur.RoleId == role.Id)
            .Where((ur, role) => ur.UserId == userRaw && role.IsActive == 1 && role.RoleCode == "admin")
            .AnyAsync();
        if (isAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        var hasPermission = await _db.Queryable<SysUserRole, SysRole, SysRoleMenu, SysMenu>((ur, role, rm, menu) =>
                ur.RoleId == role.Id && rm.RoleId == role.Id && rm.MenuId == menu.Id)
            .Where((ur, role, rm, menu) => ur.UserId == userRaw && role.IsActive == 1 && menu.IsActive == 1 && menu.PermissionCode == requirement.PermissionCode)
            .AnyAsync();
        if (hasPermission) context.Succeed(requirement);
    }
}
