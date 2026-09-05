using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabFoundation;

public sealed class LabFoundationInitializer : ILabFoundationInitializer
{
    private static readonly (string Code, string Name, string[] Items)[] BuiltInDictionaries =
    [
        ("instrument_category", "仪器分类", ["分析仪器", "通用设备", "制备设备"]),
        ("reagent_category", "试剂分类", ["无机试剂", "有机试剂", "生化试剂"]),
        ("consumable_category", "耗材分类", ["玻璃器皿", "一次性耗材", "防护用品"]),
        ("measurement_unit", "计量单位", ["台", "个", "瓶", "盒", "毫升", "克"]),
        ("instrument_status", "仪器状态", ["正常", "维修", "停用"]),
        ("reagent_hazard", "试剂危险类别", ["普通", "易燃", "腐蚀", "有毒", "易制毒"])
    ];

    private readonly ISqlSugarClient _db;
    public LabFoundationInitializer(ISqlSugarClient db) => _db = db;

    public async Task InitializeAsync()
    {
        await EnsureTablesAsync();
        await EnsureMenusAsync();
        await EnsureDictionariesAsync();
        await EnsureDemoDataAsync();
    }

    private async Task EnsureTablesAsync()
    {
        var tables = new (string Name, string Ddl)[]
        {
            ("HXS_LAB", @"CREATE TABLE HXS_LAB (ID RAW(16) NOT NULL, LAB_CODE VARCHAR2(50 CHAR) NOT NULL, LAB_NAME VARCHAR2(100 CHAR) NOT NULL, MANAGER_ID RAW(16), DESCRIPTION VARCHAR2(500 CHAR), IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_CODE UNIQUE (LAB_CODE))"),
            ("HXS_LAB_LOCATION", @"CREATE TABLE HXS_LAB_LOCATION (ID RAW(16) NOT NULL, LAB_ID RAW(16) NOT NULL, PARENT_ID RAW(16), LOCATION_CODE VARCHAR2(50 CHAR) NOT NULL, LOCATION_NAME VARCHAR2(100 CHAR) NOT NULL, LOCATION_TYPE VARCHAR2(30 CHAR) NOT NULL, SORT_NO NUMBER(10) DEFAULT 0 NOT NULL, IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_LOCATION PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_LOCATION_CODE UNIQUE (LOCATION_CODE))"),
            ("HXS_LAB_GROUP", @"CREATE TABLE HXS_LAB_GROUP (ID RAW(16) NOT NULL, LAB_ID RAW(16) NOT NULL, GROUP_CODE VARCHAR2(50 CHAR) NOT NULL, GROUP_NAME VARCHAR2(100 CHAR) NOT NULL, LEADER_ID RAW(16), DESCRIPTION VARCHAR2(500 CHAR), IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_GROUP PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_GROUP_CODE UNIQUE (GROUP_CODE))"),
            ("HXS_LAB_GROUP_MEMBER", @"CREATE TABLE HXS_LAB_GROUP_MEMBER (ID RAW(16) NOT NULL, GROUP_ID RAW(16) NOT NULL, USER_ID RAW(16) NOT NULL, MEMBER_ROLE VARCHAR2(30 CHAR) NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_GROUP_MEMBER PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_GROUP_MEMBER UNIQUE (GROUP_ID, USER_ID))"),
            ("HXS_LAB_SUPPLIER", @"CREATE TABLE HXS_LAB_SUPPLIER (ID RAW(16) NOT NULL, SUPPLIER_CODE VARCHAR2(50 CHAR) NOT NULL, SUPPLIER_NAME VARCHAR2(150 CHAR) NOT NULL, CONTACT_NAME VARCHAR2(100 CHAR), PHONE VARCHAR2(50 CHAR), EMAIL VARCHAR2(150 CHAR), ADDRESS VARCHAR2(300 CHAR), IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_SUPPLIER PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_SUPPLIER_CODE UNIQUE (SUPPLIER_CODE))"),
            ("HXS_SYS_DICT_TYPE", @"CREATE TABLE HXS_SYS_DICT_TYPE (ID RAW(16) NOT NULL, DICT_CODE VARCHAR2(50 CHAR) NOT NULL, DICT_NAME VARCHAR2(100 CHAR) NOT NULL, DESCRIPTION VARCHAR2(500 CHAR), IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_SYS_DICT_TYPE PRIMARY KEY (ID), CONSTRAINT UK_HXS_SYS_DICT_TYPE_CODE UNIQUE (DICT_CODE))"),
            ("HXS_SYS_DICT_ITEM", @"CREATE TABLE HXS_SYS_DICT_ITEM (ID RAW(16) NOT NULL, DICT_TYPE_ID RAW(16) NOT NULL, ITEM_VALUE VARCHAR2(100 CHAR) NOT NULL, ITEM_LABEL VARCHAR2(100 CHAR) NOT NULL, SORT_NO NUMBER(10) DEFAULT 0 NOT NULL, IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_SYS_DICT_ITEM PRIMARY KEY (ID), CONSTRAINT UK_HXS_SYS_DICT_ITEM UNIQUE (DICT_TYPE_ID, ITEM_VALUE))")
        };
        foreach (var table in tables)
        {
            var count = await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :name", new SugarParameter(":name", table.Name));
            if (count > 0)
            {
                var rawIdCount = await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :name AND COLUMN_NAME = 'ID' AND DATA_TYPE = 'RAW'", new SugarParameter(":name", table.Name));
                if (rawIdCount == 0)
                    throw new InvalidOperationException($"{table.Name} 的 ID 列不是 RAW(16)，请备份数据后执行阶段2迁移重建该表。");
            }
            if (count == 0) await _db.Ado.ExecuteCommandAsync(table.Ddl);
        }
    }

    private async Task EnsureMenusAsync()
    {
        var now = DateTime.Now;
        var root = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == "lab");
        if (root is null)
        {
            root = new SysMenu
            {
                Id = Guid.NewGuid().ToByteArray(), MenuCode = "lab", MenuName = "实验室管理", MenuType = "directory",
                Icon = "experiment", SortNo = 20, IsVisible = 1, IsActive = 1, CreateTime = now, UpdateTime = now
            };
            await _db.Insertable(root).ExecuteCommandAsync();
        }

        var page = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == "lab:base" || x.MenuCode == "lab:labs");
        if (page is null)
        {
            page = new SysMenu
            {
                Id = Guid.NewGuid().ToByteArray(), ParentId = root.Id, MenuCode = "lab:labs", MenuName = "实验室管理",
                MenuType = "page", RoutePath = "/lab/labs", Component = "views/lab/LabView.vue", Icon = "experiment",
                PermissionCode = "lab:base:view", SortNo = 10, IsVisible = 1, IsActive = 1, CreateTime = now, UpdateTime = now
            };
            await _db.Insertable(page).ExecuteCommandAsync();
        }
        else
        {
            page.MenuCode = "lab:labs"; page.MenuName = "实验室管理"; page.RoutePath = "/lab/labs"; page.Component = "views/lab/LabView.vue"; page.Icon = "experiment"; page.UpdateTime = now;
            await _db.Updateable(page).UpdateColumns(x => new { x.MenuCode, x.MenuName, x.RoutePath, x.Component, x.Icon, x.UpdateTime }).ExecuteCommandAsync();
        }

        var pageDefinitions = new[]
        {
            ("lab:locations", "位置管理", "/lab/locations", "views/lab/LocationView.vue", "apartment", 20),
            ("lab:groups", "课题组管理", "/lab/groups", "views/lab/GroupView.vue", "users", 30),
            ("lab:suppliers", "供应商管理", "/lab/suppliers", "views/lab/SupplierView.vue", "shop", 40),
            ("lab:dictionaries", "数据字典", "/lab/dictionaries", "views/lab/DictionaryView.vue", "database", 50)
        };
        var pages = new List<SysMenu> { page };
        foreach (var definition in pageDefinitions)
        {
            var child = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == definition.Item1);
            if (child is null)
            {
                child = new SysMenu { Id = Guid.NewGuid().ToByteArray(), ParentId = root.Id, MenuCode = definition.Item1, MenuName = definition.Item2, MenuType = "page", RoutePath = definition.Item3, Component = definition.Item4, Icon = definition.Item5, PermissionCode = "lab:base:view", SortNo = definition.Item6, IsVisible = 1, IsActive = 1, CreateTime = now, UpdateTime = now };
                await _db.Insertable(child).ExecuteCommandAsync();
            }
            else
            {
                child.ParentId = root.Id; child.MenuName = definition.Item2; child.RoutePath = definition.Item3; child.Component = definition.Item4; child.Icon = definition.Item5; child.SortNo = definition.Item6; child.UpdateTime = now;
                await _db.Updateable(child).UpdateColumns(x => new { x.ParentId, x.MenuName, x.RoutePath, x.Component, x.Icon, x.SortNo, x.UpdateTime }).ExecuteCommandAsync();
            }
            pages.Add(child);
        }

        var buttons = await _db.Queryable<SysMenu>()
            .Where(x => x.MenuType == "button" && (x.PermissionCode == "lab:base:view" || x.PermissionCode == "lab:base:manage"))
            .ToListAsync();
        foreach (var button in buttons)
        {
            button.ParentId = page.Id;
            button.UpdateTime = now;
            await _db.Updateable(button).UpdateColumns(x => new { x.ParentId, x.UpdateTime }).ExecuteCommandAsync();
        }

        var admin = await _db.Queryable<SysRole>().FirstAsync(x => x.RoleCode == "admin");
        if (admin is null) return;
        foreach (var menu in new[] { root }.Concat(pages).Concat(buttons))
        {
            var exists = await _db.Queryable<SysRoleMenu>().AnyAsync(x => x.RoleId == admin.Id && x.MenuId == menu.Id);
            if (!exists)
                await _db.Insertable(new SysRoleMenu { Id = Guid.NewGuid().ToByteArray(), RoleId = admin.Id, MenuId = menu.Id, CreateTime = now }).ExecuteCommandAsync();
        }
    }

    private async Task EnsureDictionariesAsync()
    {
        var now = DateTime.Now;
        foreach (var definition in BuiltInDictionaries)
        {
            var type = await _db.Queryable<SysDictType>().FirstAsync(x => x.DictCode == definition.Code);
            if (type is null)
            {
                type = new SysDictType { Id = Guid.NewGuid().ToByteArray(), DictCode = definition.Code, DictName = definition.Name, IsActive = 1, CreateTime = now, UpdateTime = now };
                await _db.Insertable(type).ExecuteCommandAsync();
            }
            for (var index = 0; index < definition.Items.Length; index++)
            {
                var label = definition.Items[index];
                var value = $"item_{index + 1}";
                if (!await _db.Queryable<SysDictItem>().AnyAsync(x => x.DictTypeId == type.Id && x.ItemValue == value))
                    await _db.Insertable(new SysDictItem { Id = Guid.NewGuid().ToByteArray(), DictTypeId = type.Id, ItemValue = value, ItemLabel = label, SortNo = index + 1, IsActive = 1, CreateTime = now, UpdateTime = now }).ExecuteCommandAsync();
            }
        }
    }

    private async Task EnsureDemoDataAsync()
    {
        var now = DateTime.Now;
        var admin = await _db.Queryable<AppUser>().FirstAsync(x => x.UserName == "admin");
        var chemistry = await EnsureLabAsync("LAB-CHEM", "化学分析实验室", admin?.Id, "承担试剂分析、样品检测和基础化学实验。", now);
        var instrument = await EnsureLabAsync("LAB-INST", "公共仪器实验室", admin?.Id, "集中管理精密仪器和共享测试设备。", now);

        var building = await EnsureLocationAsync(chemistry.Id, null, "CHEM-B1", "实验楼 A 栋", "building", 10, now);
        var room = await EnsureLocationAsync(chemistry.Id, building.Id, "CHEM-R301", "301 化学实验室", "room", 20, now);
        await EnsureLocationAsync(chemistry.Id, room.Id, "CHEM-A-WET", "湿化学操作区", "area", 30, now);
        var instrumentRoom = await EnsureLocationAsync(instrument.Id, null, "INST-R201", "201 仪器共享中心", "room", 10, now);
        await EnsureLocationAsync(instrument.Id, instrumentRoom.Id, "INST-A-PREP", "样品前处理区", "area", 20, now);

        var analysisGroup = await EnsureGroupAsync(chemistry.Id, "GRP-ANALYSIS", "分析检测课题组", admin?.Id, "负责常规分析检测方法建设。", now);
        await EnsureGroupAsync(instrument.Id, "GRP-INSTRUMENT", "仪器平台主管组", admin?.Id, "负责共享仪器运行和培训。", now);
        if (admin is not null && !await _db.Queryable<LabGroupMember>().AnyAsync(x => x.GroupId == analysisGroup.Id && x.UserId == admin.Id))
            await _db.Insertable(new LabGroupMember { Id = Guid.NewGuid().ToByteArray(), GroupId = analysisGroup.Id, UserId = admin.Id, MemberRole = "leader", CreateTime = now }).ExecuteCommandAsync();

        await EnsureSupplierAsync("SUP-001", "国药集团化学试剂有限公司", "张经理", "021-55550001", "service@example.com", now);
        await EnsureSupplierAsync("SUP-002", "赛默飞世尔科技", "李工程师", "400-650-5118", "support@example.com", now);
        await EnsureSupplierAsync("SUP-003", "本地实验耗材供应中心", "王女士", "010-55550003", "sales@example.com", now);
    }

    private async Task<Lab> EnsureLabAsync(string code, string name, byte[]? managerId, string description, DateTime now)
    {
        var row = await _db.Queryable<Lab>().FirstAsync(x => x.LabCode == code);
        if (row is not null) return row;
        row = new Lab { Id = Guid.NewGuid().ToByteArray(), LabCode = code, LabName = name, ManagerId = managerId, Description = description, IsActive = 1, CreateTime = now, UpdateTime = now };
        await _db.Insertable(row).ExecuteCommandAsync(); return row;
    }

    private async Task<LabLocation> EnsureLocationAsync(byte[] labId, byte[]? parentId, string code, string name, string type, int sortNo, DateTime now)
    {
        var row = await _db.Queryable<LabLocation>().FirstAsync(x => x.LocationCode == code);
        if (row is not null) return row;
        row = new LabLocation { Id = Guid.NewGuid().ToByteArray(), LabId = labId, ParentId = parentId, LocationCode = code, LocationName = name, LocationType = type, SortNo = sortNo, IsActive = 1, CreateTime = now, UpdateTime = now };
        await _db.Insertable(row).ExecuteCommandAsync(); return row;
    }

    private async Task<LabGroup> EnsureGroupAsync(byte[] labId, string code, string name, byte[]? leaderId, string description, DateTime now)
    {
        var row = await _db.Queryable<LabGroup>().FirstAsync(x => x.GroupCode == code);
        if (row is not null) return row;
        row = new LabGroup { Id = Guid.NewGuid().ToByteArray(), LabId = labId, GroupCode = code, GroupName = name, LeaderId = leaderId, Description = description, IsActive = 1, CreateTime = now, UpdateTime = now };
        await _db.Insertable(row).ExecuteCommandAsync(); return row;
    }

    private async Task EnsureSupplierAsync(string code, string name, string contact, string phone, string email, DateTime now)
    {
        if (await _db.Queryable<LabSupplier>().AnyAsync(x => x.SupplierCode == code)) return;
        await _db.Insertable(new LabSupplier { Id = Guid.NewGuid().ToByteArray(), SupplierCode = code, SupplierName = name, ContactName = contact, Phone = phone, Email = email, Address = "示例供应商地址", IsActive = 1, CreateTime = now, UpdateTime = now }).ExecuteCommandAsync();
    }
}
