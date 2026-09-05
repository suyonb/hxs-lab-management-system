using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.SystemManagement;

public sealed class SystemManagementService : ISystemManagementService
{
    private readonly ISqlSugarClient _db;
    private readonly IPasswordHasher _passwordHasher;

    public SystemManagementService(ISqlSugarClient db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<OrgDto>> GetOrgsAsync()
    {
        var items = await _db.Queryable<SysOrg>().OrderBy(x => x.SortNo).ToListAsync();
        return items.Select(ToOrgDto).ToList();
    }

    public async Task<List<OrgTreeDto>> GetOrgTreeAsync()
    {
        var items = await GetOrgsAsync();
        var nodes = items.Select(x => new OrgTreeDto
        {
            Id = x.Id,
            ParentId = x.ParentId,
            OrgName = x.OrgName,
            OrgCode = x.OrgCode,
            OrgType = x.OrgType,
            SortNo = x.SortNo,
            IsActive = x.IsActive
        }).ToDictionary(x => x.Id);

        foreach (var node in nodes.Values.OrderBy(x => x.SortNo))
        {
            if (node.ParentId.HasValue && nodes.TryGetValue(node.ParentId.Value, out var parent))
                parent.Children.Add(node);
        }

        return nodes.Values.Where(x => !x.ParentId.HasValue || !nodes.ContainsKey(x.ParentId.Value)).OrderBy(x => x.SortNo).ToList();
    }

    public async Task<OrgDto> CreateOrgAsync(CreateOrgRequest request)
    {
        EnsureRequired(request.OrgName, "组织名称不能为空。");
        EnsureRequired(request.OrgCode, "组织编码不能为空。");

        var entity = new SysOrg
        {
            Id = Guid.NewGuid().ToByteArray(),
            ParentId = RawGuidConverter.ToNullableRaw(request.ParentId),
            OrgName = request.OrgName.Trim(),
            OrgCode = request.OrgCode.Trim(),
            OrgType = request.OrgType.Trim(),
            SortNo = request.SortNo,
            IsActive = request.IsActive ? 1 : 0,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };

        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToOrgDto(entity);
    }

    public async Task<bool> UpdateOrgAsync(Guid id, UpdateOrgRequest request)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        var entity = await _db.Queryable<SysOrg>().FirstAsync(x => x.Id == rawId);
        if (entity is null)
            return false;

        entity.ParentId = RawGuidConverter.ToNullableRaw(request.ParentId);
        entity.OrgName = request.OrgName.Trim();
        entity.OrgCode = request.OrgCode.Trim();
        entity.OrgType = request.OrgType.Trim();
        entity.SortNo = request.SortNo;
        entity.IsActive = request.IsActive ? 1 : 0;
        entity.UpdateTime = DateTime.Now;
        await _db.Updateable(entity).ExecuteCommandAsync();
        return true;
    }

    public async Task<bool> DeleteOrgAsync(Guid id)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        var childCount = await _db.Queryable<SysOrg>().CountAsync(x => x.ParentId == rawId);
        var userCount = await _db.Queryable<AppUser>().CountAsync(x => x.OrgId == rawId);
        if (childCount > 0 || userCount > 0)
            throw new InvalidOperationException("组织下存在子组织或用户，不能删除。");

        return await _db.Deleteable<SysOrg>().Where(x => x.Id == rawId).ExecuteCommandAsync() > 0;
    }

    public async Task<List<UserDto>> GetUsersAsync(string? keyword = null)
    {
        var query = _db.Queryable<AppUser>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var value = keyword.Trim();
            query = query.Where(x => x.UserName.Contains(value) || x.DisplayName!.Contains(value));
        }

        var items = await query.OrderBy(x => x.UserName).ToListAsync();
        return items.Select(ToUserDto).ToList();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        EnsureRequired(request.UserName, "用户名不能为空。");
        EnsureRequired(request.Password, "密码不能为空。");

        var entity = new AppUser
        {
            Id = Guid.NewGuid().ToByteArray(),
            OrgId = RawGuidConverter.ToNullableRaw(request.OrgId),
            UserName = request.UserName.Trim(),
            DisplayName = request.DisplayName?.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            IsActive = request.IsActive ? 1 : 0,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };

        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToUserDto(entity);
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        var entity = await _db.Queryable<AppUser>().FirstAsync(x => x.Id == rawId);
        if (entity is null)
            return false;

        entity.OrgId = RawGuidConverter.ToNullableRaw(request.OrgId);
        entity.DisplayName = request.DisplayName?.Trim();
        if (!string.IsNullOrWhiteSpace(request.Password))
            entity.PasswordHash = _passwordHasher.Hash(request.Password);
        entity.Phone = request.Phone?.Trim();
        entity.Email = request.Email?.Trim();
        entity.IsActive = request.IsActive ? 1 : 0;
        entity.UpdateTime = DateTime.Now;
        await _db.Updateable(entity).ExecuteCommandAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        await _db.Deleteable<SysUserRole>().Where(x => x.UserId == rawId).ExecuteCommandAsync();
        return await _db.Deleteable<AppUser>().Where(x => x.Id == rawId).ExecuteCommandAsync() > 0;
    }

    public async Task<List<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        var rawUserId = RawGuidConverter.ToRaw(userId);
        var roles = await _db.Queryable<SysUserRole, SysRole>((ur, r) => ur.RoleId == r.Id)
            .Where((ur, r) => ur.UserId == rawUserId)
            .OrderBy((ur, r) => r.RoleCode)
            .Select((ur, r) => r)
            .ToListAsync();
        return roles.Select(ToRoleDto).ToList();
    }

    public async Task AssignUserRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds)
    {
        var rawUserId = RawGuidConverter.ToRaw(userId);
        await _db.Deleteable<SysUserRole>().Where(x => x.UserId == rawUserId).ExecuteCommandAsync();
        var rows = roleIds.Distinct().Select(roleId => new SysUserRole
        {
            Id = Guid.NewGuid().ToByteArray(),
            UserId = rawUserId,
            RoleId = RawGuidConverter.ToRaw(roleId),
            CreateTime = DateTime.Now
        }).ToList();
        if (rows.Count > 0)
            await _db.Insertable(rows).ExecuteCommandAsync();
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var items = await _db.Queryable<SysRole>().OrderBy(x => x.RoleCode).ToListAsync();
        return items.Select(ToRoleDto).ToList();
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        EnsureRequired(request.RoleCode, "角色编码不能为空。");
        EnsureRequired(request.RoleName, "角色名称不能为空。");

        var entity = new SysRole
        {
            Id = Guid.NewGuid().ToByteArray(),
            RoleCode = request.RoleCode.Trim(),
            RoleName = request.RoleName.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive ? 1 : 0,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };
        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToRoleDto(entity);
    }

    public async Task<bool> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        var entity = await _db.Queryable<SysRole>().FirstAsync(x => x.Id == rawId);
        if (entity is null)
            return false;

        entity.RoleCode = request.RoleCode.Trim();
        entity.RoleName = request.RoleName.Trim();
        entity.Description = request.Description?.Trim();
        entity.IsActive = request.IsActive ? 1 : 0;
        entity.UpdateTime = DateTime.Now;
        await _db.Updateable(entity).ExecuteCommandAsync();
        return true;
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        await _db.Deleteable<SysUserRole>().Where(x => x.RoleId == rawId).ExecuteCommandAsync();
        await _db.Deleteable<SysRoleMenu>().Where(x => x.RoleId == rawId).ExecuteCommandAsync();
        return await _db.Deleteable<SysRole>().Where(x => x.Id == rawId).ExecuteCommandAsync() > 0;
    }

    public async Task<List<MenuDto>> GetRoleMenusAsync(Guid roleId)
    {
        var rawRoleId = RawGuidConverter.ToRaw(roleId);
        var menus = await _db.Queryable<SysRoleMenu, SysMenu>((rm, m) => rm.MenuId == m.Id)
            .Where((rm, m) => rm.RoleId == rawRoleId)
            .OrderBy((rm, m) => m.SortNo)
            .Select((rm, m) => m)
            .ToListAsync();
        return menus.Select(ToMenuDto).ToList();
    }

    public async Task AssignRoleMenusAsync(Guid roleId, IReadOnlyCollection<Guid> menuIds)
    {
        var rawRoleId = RawGuidConverter.ToRaw(roleId);
        await _db.Deleteable<SysRoleMenu>().Where(x => x.RoleId == rawRoleId).ExecuteCommandAsync();
        var rows = menuIds.Distinct().Select(menuId => new SysRoleMenu
        {
            Id = Guid.NewGuid().ToByteArray(),
            RoleId = rawRoleId,
            MenuId = RawGuidConverter.ToRaw(menuId),
            CreateTime = DateTime.Now
        }).ToList();
        if (rows.Count > 0)
            await _db.Insertable(rows).ExecuteCommandAsync();
    }

    public async Task<List<MenuDto>> GetMenusAsync()
    {
        var items = await _db.Queryable<SysMenu>().OrderBy(x => x.SortNo).ToListAsync();
        return items.Select(ToMenuDto).ToList();
    }

    public async Task<List<MenuTreeDto>> GetMenuTreeAsync()
    {
        return BuildMenuTree(await GetMenusAsync());
    }

    public async Task<List<MenuTreeDto>> GetUserMenuTreeAsync(Guid userId)
    {
        var rawUserId = RawGuidConverter.ToRaw(userId);
        var menus = await _db.Queryable<SysUserRole, SysRole, SysRoleMenu, SysMenu>((ur, r, rm, m) =>
                ur.RoleId == r.Id && rm.RoleId == r.Id && rm.MenuId == m.Id)
            .Where((ur, r, rm, m) => ur.UserId == rawUserId && r.IsActive == 1 && m.IsActive == 1 && m.IsVisible == 1)
            .OrderBy((ur, r, rm, m) => m.SortNo)
            .Select((ur, r, rm, m) => m)
            .ToListAsync();
        return BuildMenuTree(menus.Select(ToMenuDto).ToList());
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var rawUserId = RawGuidConverter.ToRaw(userId);
        var isAdmin = await _db.Queryable<SysUserRole, SysRole>((ur, role) => ur.RoleId == role.Id)
            .Where((ur, role) => ur.UserId == rawUserId && role.IsActive == 1 && role.RoleCode == "admin")
            .AnyAsync();
        if (isAdmin)
            return await _db.Queryable<SysMenu>().Where(x => x.IsActive == 1 && x.PermissionCode != null)
                .Select(x => x.PermissionCode!).Distinct().ToListAsync();

        return await _db.Queryable<SysUserRole, SysRole, SysRoleMenu, SysMenu>((ur, role, rm, menu) =>
                ur.RoleId == role.Id && rm.RoleId == role.Id && rm.MenuId == menu.Id)
            .Where((ur, role, rm, menu) => ur.UserId == rawUserId && role.IsActive == 1 && menu.IsActive == 1 && menu.PermissionCode != null)
            .Select((ur, role, rm, menu) => menu.PermissionCode!).Distinct().ToListAsync();
    }

    public async Task<MenuDto> CreateMenuAsync(CreateMenuRequest request)
    {
        EnsureRequired(request.MenuCode, "菜单编码不能为空。");
        EnsureRequired(request.MenuName, "菜单名称不能为空。");
        ValidatePageRoute(request);

        var entity = new SysMenu
        {
            Id = Guid.NewGuid().ToByteArray(),
            ParentId = RawGuidConverter.ToNullableRaw(request.ParentId),
            MenuCode = request.MenuCode.Trim(),
            MenuName = request.MenuName.Trim(),
            MenuType = request.MenuType.Trim(),
            RoutePath = request.RoutePath?.Trim(),
            Component = request.Component?.Trim(),
            Icon = request.Icon?.Trim(),
            PermissionCode = request.PermissionCode?.Trim(),
            SortNo = request.SortNo,
            IsVisible = request.IsVisible ? 1 : 0,
            IsActive = request.IsActive ? 1 : 0,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };

        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToMenuDto(entity);
    }

    public async Task<bool> UpdateMenuAsync(Guid id, UpdateMenuRequest request)
    {
        EnsureRequired(request.MenuCode, "菜单编码不能为空。");
        EnsureRequired(request.MenuName, "菜单名称不能为空。");
        ValidatePageRoute(request);
        var rawId = RawGuidConverter.ToRaw(id);
        var entity = await _db.Queryable<SysMenu>().FirstAsync(x => x.Id == rawId);
        if (entity is null)
            return false;

        entity.ParentId = RawGuidConverter.ToNullableRaw(request.ParentId);
        entity.MenuCode = request.MenuCode.Trim();
        entity.MenuName = request.MenuName.Trim();
        entity.MenuType = request.MenuType.Trim();
        entity.RoutePath = request.RoutePath?.Trim();
        entity.Component = request.Component?.Trim();
        entity.Icon = request.Icon?.Trim();
        entity.PermissionCode = request.PermissionCode?.Trim();
        entity.SortNo = request.SortNo;
        entity.IsVisible = request.IsVisible ? 1 : 0;
        entity.IsActive = request.IsActive ? 1 : 0;
        entity.UpdateTime = DateTime.Now;
        await _db.Updateable(entity).ExecuteCommandAsync();
        return true;
    }

    private static void ValidatePageRoute(CreateMenuRequest request)
    {
        if (!string.Equals(request.MenuType, "page", StringComparison.OrdinalIgnoreCase)) return;
        EnsureRequired(request.RoutePath, "页面菜单的路由路径不能为空。");
        EnsureRequired(request.Component, "页面菜单的组件文件不能为空。");
        if (!request.RoutePath!.StartsWith('/'))
            throw new ArgumentException("页面路由路径必须以 / 开头。");
        var component = request.Component!.Replace('\\', '/');
        if (!component.StartsWith("views/", StringComparison.OrdinalIgnoreCase) ||
            !component.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) || component.Contains("..", StringComparison.Ordinal))
            throw new ArgumentException("组件文件必须使用 views/.../*.vue 格式。");
    }

    public async Task<bool> DeleteMenuAsync(Guid id)
    {
        var rawId = RawGuidConverter.ToRaw(id);
        var childCount = await _db.Queryable<SysMenu>().CountAsync(x => x.ParentId == rawId);
        if (childCount > 0)
            throw new InvalidOperationException("菜单下存在子菜单，不能删除。");

        await _db.Deleteable<SysRoleMenu>().Where(x => x.MenuId == rawId).ExecuteCommandAsync();
        return await _db.Deleteable<SysMenu>().Where(x => x.Id == rawId).ExecuteCommandAsync() > 0;
    }

    private static List<MenuTreeDto> BuildMenuTree(List<MenuDto> items)
    {
        var nodes = items
            .GroupBy(x => x.Id)
            .Select(x => x.OrderBy(y => y.SortNo).First())
            .Select(x => new MenuTreeDto
        {
            Id = x.Id,
            ParentId = x.ParentId,
            MenuCode = x.MenuCode,
            MenuName = x.MenuName,
            MenuType = x.MenuType,
            RoutePath = x.RoutePath,
            Component = x.Component,
            Icon = x.Icon,
            PermissionCode = x.PermissionCode,
            SortNo = x.SortNo,
            IsVisible = x.IsVisible,
            IsActive = x.IsActive
        }).ToDictionary(x => x.Id);

        foreach (var node in nodes.Values.OrderBy(x => x.SortNo))
        {
            if (node.ParentId.HasValue && nodes.TryGetValue(node.ParentId.Value, out var parent))
                parent.Children.Add(node);
        }

        return nodes.Values.Where(x => !x.ParentId.HasValue || !nodes.ContainsKey(x.ParentId.Value)).OrderBy(x => x.SortNo).ToList();
    }

    private static OrgDto ToOrgDto(SysOrg x) => new(
        RawGuidConverter.ToGuid(x.Id),
        x.ParentId is null ? null : RawGuidConverter.ToGuid(x.ParentId),
        x.OrgName,
        x.OrgCode,
        x.OrgType,
        x.SortNo,
        x.IsActive == 1);

    private static UserDto ToUserDto(AppUser x) => new(
        RawGuidConverter.ToGuid(x.Id),
        x.OrgId is null ? null : RawGuidConverter.ToGuid(x.OrgId),
        x.UserName,
        x.DisplayName,
        x.Phone,
        x.Email,
        x.IsActive == 1,
        x.LastLoginTime);

    private static RoleDto ToRoleDto(SysRole x) => new(
        RawGuidConverter.ToGuid(x.Id),
        x.RoleCode,
        x.RoleName,
        x.Description,
        x.IsActive == 1);

    private static MenuDto ToMenuDto(SysMenu x) => new(
        RawGuidConverter.ToGuid(x.Id),
        x.ParentId is null ? null : RawGuidConverter.ToGuid(x.ParentId),
        x.MenuCode,
        x.MenuName,
        x.MenuType,
        x.RoutePath,
        x.Component,
        x.Icon,
        x.PermissionCode,
        x.SortNo,
        x.IsVisible == 1,
        x.IsActive == 1);

    private static void EnsureRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(message);
    }
}
