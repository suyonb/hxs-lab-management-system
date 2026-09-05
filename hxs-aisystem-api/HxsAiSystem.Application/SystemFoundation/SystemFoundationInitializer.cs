using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.SystemFoundation;

public sealed class SystemFoundationInitializer : ISystemFoundationInitializer
{
    private static readonly string[] Permissions =
    [
        "sys:audit:list", "sys:user:list", "sys:user:create", "sys:user:edit", "sys:user:delete", "sys:user:assign-role",
        "sys:role:list", "sys:role:create", "sys:role:edit", "sys:role:delete", "sys:role:assign-menu",
        "sys:menu:list", "sys:menu:create", "sys:menu:edit", "sys:menu:delete",
        "sys:org:list", "sys:org:create", "sys:org:edit", "sys:org:delete",
        "ai:reasoning:use", "lab:base:view", "lab:base:manage", "lab:instrument:view", "lab:instrument:create",
        "lab:instrument:edit", "lab:booking:create", "lab:booking:cancel", "lab:booking:approve", "lab:repair:create",
        "lab:repair:approve", "lab:inventory:view", "lab:stock:in", "lab:stock:adjust", "lab:requisition:create",
        "lab:requisition:cancel", "lab:requisition:approve", "lab:experiment:create", "lab:experiment:edit",
        "lab:experiment:archive", "lab:experiment:unarchive", "lab:material:manage", "lab:requisition:view"
    ];

    private readonly ISqlSugarClient _db;
    public SystemFoundationInitializer(ISqlSugarClient db) => _db = db;

    public async Task InitializeAsync()
    {
        if (!await ColumnExistsAsync("HXS_SYS_USER", "FAILED_LOGIN_COUNT"))
            await _db.Ado.ExecuteCommandAsync("ALTER TABLE HXS_SYS_USER ADD FAILED_LOGIN_COUNT NUMBER(10) DEFAULT 0 NOT NULL");
        if (!await ColumnExistsAsync("HXS_SYS_USER", "LOCKED_UNTIL"))
            await _db.Ado.ExecuteCommandAsync("ALTER TABLE HXS_SYS_USER ADD LOCKED_UNTIL TIMESTAMP(6)");

        if (!await TableExistsAsync("HXS_SYS_AUDIT_LOG"))
        {
            await _db.Ado.ExecuteCommandAsync(@"CREATE TABLE HXS_SYS_AUDIT_LOG (
                ID RAW(16) NOT NULL, USER_ID RAW(16), USER_NAME VARCHAR2(100 CHAR), MODULE_CODE VARCHAR2(100 CHAR) NOT NULL,
                ACTION_CODE VARCHAR2(100 CHAR) NOT NULL, BUSINESS_ID VARCHAR2(100 CHAR), REQUEST_PATH VARCHAR2(300 CHAR) NOT NULL,
                HTTP_METHOD VARCHAR2(20 CHAR) NOT NULL, BEFORE_DATA CLOB, AFTER_DATA CLOB, RESULT VARCHAR2(30 CHAR) NOT NULL,
                IP_ADDRESS VARCHAR2(100 CHAR), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL,
                CONSTRAINT PK_HXS_SYS_AUDIT_LOG PRIMARY KEY (ID))");
            await _db.Ado.ExecuteCommandAsync("CREATE INDEX IX_HXS_SYS_AUDIT_TIME ON HXS_SYS_AUDIT_LOG (CREATE_TIME)");
            await _db.Ado.ExecuteCommandAsync("CREATE INDEX IX_HXS_SYS_AUDIT_USER ON HXS_SYS_AUDIT_LOG (USER_ID, CREATE_TIME)");
        }

        if (!await TableExistsAsync("HXS_SYS_FILE"))
        {
            await _db.Ado.ExecuteCommandAsync(@"CREATE TABLE HXS_SYS_FILE (
                ID RAW(16) NOT NULL, BUSINESS_TYPE VARCHAR2(50 CHAR) NOT NULL, BUSINESS_ID VARCHAR2(100 CHAR),
                ORIGINAL_NAME VARCHAR2(255 CHAR) NOT NULL, STORAGE_NAME VARCHAR2(255 CHAR) NOT NULL,
                FILE_PATH VARCHAR2(500 CHAR) NOT NULL, CONTENT_TYPE VARCHAR2(150 CHAR) NOT NULL, FILE_SIZE NUMBER(19) NOT NULL,
                UPLOADER_ID RAW(16) NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL,
                CONSTRAINT PK_HXS_SYS_FILE PRIMARY KEY (ID), CONSTRAINT FK_HXS_SYS_FILE_USER FOREIGN KEY (UPLOADER_ID) REFERENCES HXS_SYS_USER (ID))");
            await _db.Ado.ExecuteCommandAsync("CREATE INDEX IX_HXS_SYS_FILE_BUSINESS ON HXS_SYS_FILE (BUSINESS_TYPE, BUSINESS_ID)");
        }

        await EnsureRoleAsync("lab_admin", "实验管理员", "管理所属实验室业务并执行审批");
        await EnsureRoleAsync("lab_user", "普通实验员", "提交申请并维护本人实验记录");
        await EnsureAuditMenuAsync();
        await EnsurePageComponentPathsAsync();
        await EnsurePermissionsAsync();
    }

    private async Task EnsureAuditMenuAsync()
    {
        if (await _db.Queryable<SysMenu>().AnyAsync(x => x.MenuCode == "sys:audit")) return;
        var parent = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == "sys");
        var now = DateTime.Now;
        await _db.Insertable(new SysMenu
        {
            Id = Guid.NewGuid().ToByteArray(), ParentId = parent?.Id, MenuCode = "sys:audit", MenuName = "操作日志",
            MenuType = "page", RoutePath = "/system/audit-logs", Component = "system/audit/index", Icon = "history",
            PermissionCode = "sys:audit:list", SortNo = 50, IsVisible = 1, IsActive = 1, CreateTime = now, UpdateTime = now
        }).ExecuteCommandAsync();
    }

    private async Task EnsurePageComponentPathsAsync()
    {
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["dashboard"] = "views/dashboard/DashboardView.vue",
            ["ai:reasoning"] = "views/ai/ReasoningView.vue",
            ["sys:user"] = "views/system/UserView.vue",
            ["sys:role"] = "views/system/RoleView.vue",
            ["sys:menu"] = "views/system/MenuView.vue",
            ["sys:org"] = "views/system/OrgView.vue",
            ["sys:audit"] = "views/system/AuditLogView.vue"
        };
        var pages = await _db.Queryable<SysMenu>().Where(x => x.MenuType == "page").ToListAsync();
        foreach (var page in pages.Where(x => paths.ContainsKey(x.MenuCode)))
        {
            var component = paths[page.MenuCode];
            if (page.Component == component) continue;
            page.Component = component;
            page.UpdateTime = DateTime.Now;
            await _db.Updateable(page).UpdateColumns(x => new { x.Component, x.UpdateTime }).ExecuteCommandAsync();
        }
    }

    private async Task EnsureRoleAsync(string code, string name, string description)
    {
        if (await _db.Queryable<SysRole>().AnyAsync(x => x.RoleCode == code)) return;
        var now = DateTime.Now;
        await _db.Insertable(new SysRole
        {
            Id = Guid.NewGuid().ToByteArray(), RoleCode = code, RoleName = name, Description = description,
            IsActive = 1, CreateTime = now, UpdateTime = now
        }).ExecuteCommandAsync();
    }

    private async Task EnsurePermissionsAsync()
    {
        var menuPages = await _db.Queryable<SysMenu>()
            .Where(x => x.MenuType == "page")
            .ToListAsync();
        var pageIdsByCode = menuPages.ToDictionary(x => x.MenuCode, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var existing = await _db.Queryable<SysMenu>().Where(x => x.PermissionCode != null)
            .Select(x => x.PermissionCode!).ToListAsync();
        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.Now;
        var missing = Permissions.Where(x => !existingSet.Contains(x)).Select((permission, index) => new SysMenu
        {
            Id = Guid.NewGuid().ToByteArray(), MenuCode = "permission:" + permission, MenuName = permission,
            ParentId = FindPermissionParentId(permission, pageIdsByCode),
            MenuType = "button", PermissionCode = permission, SortNo = 1000 + index,
            IsVisible = 0, IsActive = 1, CreateTime = now, UpdateTime = now
        }).ToList();
        if (missing.Count > 0) await _db.Insertable(missing).ExecuteCommandAsync();

        var orphanPermissions = await _db.Queryable<SysMenu>()
            .Where(x => x.MenuType == "button" && x.ParentId == null && x.PermissionCode != null)
            .ToListAsync();
        foreach (var permission in orphanPermissions)
        {
            permission.ParentId = FindPermissionParentId(permission.PermissionCode!, pageIdsByCode);
            if (permission.ParentId is not null)
                permission.UpdateTime = now;
        }
        var repairedPermissions = orphanPermissions.Where(x => x.ParentId is not null).ToList();
        foreach (var permission in repairedPermissions)
        {
            await _db.Updateable(permission)
                .UpdateColumns(x => new { x.ParentId, x.UpdateTime })
                .ExecuteCommandAsync();
        }

        var adminRole = await _db.Queryable<SysRole>().FirstAsync(x => x.RoleCode == "admin");
        if (adminRole is null) return;
        var permissionMenus = await _db.Queryable<SysMenu>().Where(x => x.PermissionCode != null).ToListAsync();
        var assigned = await _db.Queryable<SysRoleMenu>().Where(x => x.RoleId == adminRole.Id).Select(x => x.MenuId).ToListAsync();
        var assignedIds = assigned.Select(RawGuidConverter.ToGuid).ToHashSet();
        var mappings = permissionMenus.Where(x => !assignedIds.Contains(RawGuidConverter.ToGuid(x.Id))).Select(x => new SysRoleMenu
        {
            Id = Guid.NewGuid().ToByteArray(), RoleId = adminRole.Id, MenuId = x.Id, CreateTime = now
        }).ToList();
        if (mappings.Count > 0) await _db.Insertable(mappings).ExecuteCommandAsync();
    }

    private static byte[]? FindPermissionParentId(string permission, IReadOnlyDictionary<string, byte[]> pageIdsByCode)
    {
        var segments = permission.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2) return null;
        return pageIdsByCode.GetValueOrDefault($"{segments[0]}:{segments[1]}");
    }

    private async Task<bool> TableExistsAsync(string tableName) =>
        await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :name", new SugarParameter(":name", tableName)) > 0;

    private async Task<bool> ColumnExistsAsync(string tableName, string columnName) =>
        await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :tableName AND COLUMN_NAME = :columnName",
            new SugarParameter(":tableName", tableName), new SugarParameter(":columnName", columnName)) > 0;
}
