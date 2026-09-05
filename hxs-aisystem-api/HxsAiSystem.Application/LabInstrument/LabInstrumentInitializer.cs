using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabInstrument;

public sealed class LabInstrumentInitializer : ILabInstrumentInitializer
{
    private readonly ISqlSugarClient _db;
    public LabInstrumentInitializer(ISqlSugarClient db) => _db = db;
    public async Task InitializeAsync() { await EnsureTablesAsync(); await EnsureMenusAsync(); await EnsureDemoDataAsync(); }

    private async Task EnsureTablesAsync()
    {
        var tables = new (string, string)[]
        {
            ("HXS_LAB_INSTRUMENT", "CREATE TABLE HXS_LAB_INSTRUMENT (ID RAW(16) NOT NULL, INSTRUMENT_CODE VARCHAR2(50 CHAR) NOT NULL, INSTRUMENT_NAME VARCHAR2(150 CHAR) NOT NULL, CATEGORY_ID RAW(16), MODEL VARCHAR2(100 CHAR), MANUFACTURER VARCHAR2(150 CHAR), SUPPLIER_ID RAW(16), LAB_ID RAW(16) NOT NULL, LOCATION_ID RAW(16) NOT NULL, STATUS VARCHAR2(20 CHAR) DEFAULT 'normal' NOT NULL, DESCRIPTION VARCHAR2(500 CHAR), IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_INSTRUMENT PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_INSTRUMENT_CODE UNIQUE (INSTRUMENT_CODE))"),
            ("HXS_LAB_BOOKING", "CREATE TABLE HXS_LAB_BOOKING (ID RAW(16) NOT NULL, BOOKING_NO VARCHAR2(40 CHAR) NOT NULL, INSTRUMENT_ID RAW(16) NOT NULL, APPLICANT_ID RAW(16) NOT NULL, GROUP_ID RAW(16), START_TIME TIMESTAMP(6) NOT NULL, END_TIME TIMESTAMP(6) NOT NULL, PURPOSE VARCHAR2(500 CHAR) NOT NULL, STATUS VARCHAR2(20 CHAR) NOT NULL, APPROVER_ID RAW(16), APPROVE_TIME TIMESTAMP(6), APPROVE_REMARK VARCHAR2(500 CHAR), CANCEL_TIME TIMESTAMP(6), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_BOOKING PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_BOOKING_NO UNIQUE (BOOKING_NO))"),
            ("HXS_LAB_USAGE", "CREATE TABLE HXS_LAB_USAGE (ID RAW(16) NOT NULL, INSTRUMENT_ID RAW(16) NOT NULL, BOOKING_ID RAW(16), USER_ID RAW(16) NOT NULL, START_TIME TIMESTAMP(6) NOT NULL, END_TIME TIMESTAMP(6) NOT NULL, EXPERIMENT_CONTENT VARCHAR2(1000 CHAR) NOT NULL, REMARK VARCHAR2(500 CHAR), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_USAGE PRIMARY KEY (ID))"),
            ("HXS_LAB_REPAIR", "CREATE TABLE HXS_LAB_REPAIR (ID RAW(16) NOT NULL, REPAIR_NO VARCHAR2(40 CHAR) NOT NULL, INSTRUMENT_ID RAW(16) NOT NULL, REPORTER_ID RAW(16) NOT NULL, FAULT_DESCRIPTION VARCHAR2(1000 CHAR) NOT NULL, STATUS VARCHAR2(20 CHAR) NOT NULL, APPROVER_ID RAW(16), APPROVE_TIME TIMESTAMP(6), REPAIRER VARCHAR2(100 CHAR), REPAIR_CONTENT VARCHAR2(1000 CHAR), REPAIR_START_TIME TIMESTAMP(6), REPAIR_END_TIME TIMESTAMP(6), REMARK VARCHAR2(500 CHAR), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_REPAIR PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_REPAIR_NO UNIQUE (REPAIR_NO))")
        };
        foreach (var (name, ddl) in tables) if (await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :name", new SugarParameter(":name", name)) == 0) await _db.Ado.ExecuteCommandAsync(ddl);
    }

    private async Task EnsureMenusAsync()
    {
        var root = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == "lab") ?? throw new InvalidOperationException("实验室管理根菜单不存在。"); var now = DateTime.Now;
        var defs = new[] { ("lab:instruments", "仪器台账", "/lab/instruments", "views/lab/InstrumentView.vue", "experiment", "lab:instrument:view", 60), ("lab:bookings", "仪器预约", "/lab/bookings", "views/lab/BookingView.vue", "history", "lab:booking:view", 70), ("lab:booking-approvals", "预约审批", "/lab/booking-approvals", "views/lab/BookingApprovalView.vue", "shield", "lab:booking:approve", 80), ("lab:usages", "使用记录", "/lab/usages", "views/lab/UsageView.vue", "database", "lab:usage:view", 90), ("lab:repairs", "设备报修", "/lab/repairs", "views/lab/RepairView.vue", "setting", "lab:repair:view", 100) };
        var menus = new List<SysMenu> { root };
        foreach (var d in defs)
        {
            var row = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == d.Item1 && x.MenuType == "page");
            if (row is null)
            {
                row = new SysMenu { Id = Guid.NewGuid().ToByteArray(), ParentId = root.Id, MenuCode = d.Item1, MenuName = d.Item2, MenuType = "page", RoutePath = d.Item3, Component = d.Item4, Icon = d.Item5, PermissionCode = d.Item6, SortNo = d.Item7, IsVisible = 1, IsActive = 1, CreateTime = now, UpdateTime = now };
                await _db.Insertable(row).ExecuteCommandAsync();
            }
            else
            {
                row.ParentId = root.Id; row.MenuName = d.Item2; row.RoutePath = d.Item3; row.Component = d.Item4; row.Icon = d.Item5; row.PermissionCode = d.Item6; row.SortNo = d.Item7; row.IsVisible = 1; row.IsActive = 1; row.UpdateTime = now;
                await _db.Updateable(row).UpdateColumns(x => new { x.ParentId, x.MenuName, x.RoutePath, x.Component, x.Icon, x.PermissionCode, x.SortNo, x.IsVisible, x.IsActive, x.UpdateTime }).ExecuteCommandAsync();
            }
            menus.Add(row);
        }
        var permissions = new[]
        {
            (Code: "lab:instrument:manage", Name: "维护仪器", Parent: "lab:instruments"),
            (Code: "lab:booking:create", Name: "申请预约", Parent: "lab:bookings"),
            (Code: "lab:booking:cancel", Name: "取消预约", Parent: "lab:bookings"),
            (Code: "lab:booking:approve", Name: "审批预约", Parent: "lab:booking-approvals"),
            (Code: "lab:usage:create", Name: "登记使用", Parent: "lab:usages"),
            (Code: "lab:repair:create", Name: "提交报修", Parent: "lab:repairs"),
            (Code: "lab:repair:approve", Name: "审批报修", Parent: "lab:repairs"),
            (Code: "lab:repair:work", Name: "维修处理", Parent: "lab:repairs")
        };
        foreach (var definition in permissions)
        {
            var parent = menus.First(x => x.MenuCode == definition.Parent);
            var row = await _db.Queryable<SysMenu>().FirstAsync(x => x.PermissionCode == definition.Code && x.MenuType == "button");
            if (row is null)
            {
                row = new SysMenu { Id = Guid.NewGuid().ToByteArray(), ParentId = parent.Id, MenuCode = definition.Code, MenuName = definition.Name, MenuType = "button", PermissionCode = definition.Code, SortNo = 10, IsVisible = 0, IsActive = 1, CreateTime = now, UpdateTime = now };
                await _db.Insertable(row).ExecuteCommandAsync();
            }
            else if (row.ParentId is null || !row.ParentId.SequenceEqual(parent.Id) || row.MenuName != definition.Name)
            {
                row.ParentId = parent.Id; row.MenuName = definition.Name; row.UpdateTime = now;
                await _db.Updateable(row).UpdateColumns(x => new { x.ParentId, x.MenuName, x.UpdateTime }).ExecuteCommandAsync();
            }
            menus.Add(row);
        }
        var admin = await _db.Queryable<SysRole>().FirstAsync(x => x.RoleCode == "admin");
        if (admin is not null) await GrantMenusAsync(admin, menus, menus.Select(x => x.MenuCode), now);

        var labAdmin = await _db.Queryable<SysRole>().FirstAsync(x => x.RoleCode == "lab_admin");
        if (labAdmin is not null)
        {
            await GrantMenusAsync(labAdmin, menus,
            ["lab", "lab:instruments", "lab:bookings", "lab:booking-approvals", "lab:usages", "lab:repairs", "lab:instrument:manage", "lab:booking:create", "lab:booking:cancel", "lab:booking:approve", "lab:usage:create", "lab:repair:create", "lab:repair:approve", "lab:repair:work"], now);
            await GrantFoundationPermissionsAsync(labAdmin, includeManage: true, now);
        }

        var labUser = await _db.Queryable<SysRole>().FirstAsync(x => x.RoleCode == "lab_user");
        if (labUser is not null)
        {
            await GrantMenusAsync(labUser, menus,
            ["lab", "lab:instruments", "lab:bookings", "lab:usages", "lab:repairs", "lab:booking:create", "lab:booking:cancel", "lab:usage:create", "lab:repair:create"], now);
            await GrantFoundationPermissionsAsync(labUser, includeManage: false, now);
        }
    }

    private async Task GrantFoundationPermissionsAsync(SysRole role, bool includeManage, DateTime now)
    {
        var codes = includeManage ? new[] { "lab:base:view", "lab:base:manage" } : new[] { "lab:base:view" };
        var permissions = await _db.Queryable<SysMenu>().Where(x => x.MenuType == "button" && codes.Contains(x.PermissionCode!)).ToListAsync();
        foreach (var permission in permissions)
            if (!await _db.Queryable<SysRoleMenu>().AnyAsync(x => x.RoleId == role.Id && x.MenuId == permission.Id))
                await _db.Insertable(new SysRoleMenu { Id = Guid.NewGuid().ToByteArray(), RoleId = role.Id, MenuId = permission.Id, CreateTime = now }).ExecuteCommandAsync();
    }

    private async Task GrantMenusAsync(SysRole role, IEnumerable<SysMenu> menus, IEnumerable<string> menuCodes, DateTime now)
    {
        var allowed = menuCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var menu in menus.Where(x => allowed.Contains(x.MenuCode) || (!string.IsNullOrWhiteSpace(x.PermissionCode) && allowed.Contains(x.PermissionCode))))
            if (!await _db.Queryable<SysRoleMenu>().AnyAsync(x => x.RoleId == role.Id && x.MenuId == menu.Id))
                await _db.Insertable(new SysRoleMenu { Id = Guid.NewGuid().ToByteArray(), RoleId = role.Id, MenuId = menu.Id, CreateTime = now }).ExecuteCommandAsync();
    }

    private async Task EnsureDemoDataAsync()
    {
        if (await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().AnyAsync()) return; var lab = await _db.Queryable<Lab>().FirstAsync(x => x.LabCode == "LAB-INST"); if (lab is null) return; var locations = await _db.Queryable<LabLocation>().Where(x => x.LabId == lab.Id).ToListAsync(); if (locations.Count == 0) return; var supplier = await _db.Queryable<LabSupplier>().FirstAsync(x => x.SupplierCode == "SUP-002"); var dict = await _db.Queryable<SysDictType>().FirstAsync(x => x.DictCode == "instrument_category"); var category = dict is null ? null : await _db.Queryable<SysDictItem>().FirstAsync(x => x.DictTypeId == dict.Id); var now = DateTime.Now;
        var rows = new[] { new HxsAiSystem.Domain.Entities.LabInstrument { Id = Guid.NewGuid().ToByteArray(), InstrumentCode = "INS-HPLC-001", InstrumentName = "高效液相色谱仪", CategoryId = category?.Id, Model = "Vanquish Core", Manufacturer = "Thermo Fisher", SupplierId = supplier?.Id, LabId = lab.Id, LocationId = locations[0].Id, Status = "normal", Description = "公共分析检测设备", IsActive = 1, CreateTime = now, UpdateTime = now }, new HxsAiSystem.Domain.Entities.LabInstrument { Id = Guid.NewGuid().ToByteArray(), InstrumentCode = "INS-UV-001", InstrumentName = "紫外可见分光光度计", CategoryId = category?.Id, Model = "UV-2600i", Manufacturer = "Shimadzu", SupplierId = supplier?.Id, LabId = lab.Id, LocationId = locations[0].Id, Status = "normal", Description = "常规光谱分析设备", IsActive = 1, CreateTime = now, UpdateTime = now } }; await _db.Insertable(rows).ExecuteCommandAsync();
    }
}
