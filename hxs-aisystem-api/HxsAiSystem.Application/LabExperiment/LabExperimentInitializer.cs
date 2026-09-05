using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabExperiment;

public sealed class LabExperimentInitializer : ILabExperimentInitializer
{
    private readonly ISqlSugarClient _db;
    public LabExperimentInitializer(ISqlSugarClient db) => _db = db;

    public async Task InitializeAsync()
    {
        await EnsureTablesAsync();
        await EnsureMenusAsync();
    }

    private async Task EnsureTablesAsync()
    {
        var tables = new (string Name, string Ddl)[]
        {
            ("HXS_LAB_EXPERIMENT", "CREATE TABLE HXS_LAB_EXPERIMENT (ID RAW(16) NOT NULL, EXPERIMENT_NO VARCHAR2(40 CHAR) NOT NULL, EXPERIMENT_NAME VARCHAR2(150 CHAR) NOT NULL, GROUP_ID RAW(16), OWNER_ID RAW(16) NOT NULL, TOPIC_NAME VARCHAR2(150 CHAR), PURPOSE VARCHAR2(1000 CHAR) NOT NULL, STATUS VARCHAR2(20 CHAR) NOT NULL, START_TIME TIMESTAMP(6), END_TIME TIMESTAMP(6), ARCHIVE_USER_ID RAW(16), ARCHIVE_TIME TIMESTAMP(6), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_EXPERIMENT PRIMARY KEY(ID), CONSTRAINT UK_HXS_LAB_EXPERIMENT_NO UNIQUE(EXPERIMENT_NO), CONSTRAINT CK_HXS_LAB_EXPERIMENT_STATUS CHECK(STATUS IN ('draft','in_progress','completed','archived')), CONSTRAINT FK_EXP_GROUP FOREIGN KEY(GROUP_ID) REFERENCES HXS_LAB_GROUP(ID), CONSTRAINT FK_EXP_OWNER FOREIGN KEY(OWNER_ID) REFERENCES HXS_SYS_USER(ID), CONSTRAINT FK_EXP_ARCHIVE_USER FOREIGN KEY(ARCHIVE_USER_ID) REFERENCES HXS_SYS_USER(ID))"),
            ("HXS_LAB_EXPERIMENT_INSTRUMENT", "CREATE TABLE HXS_LAB_EXPERIMENT_INSTRUMENT (ID RAW(16) NOT NULL, EXPERIMENT_ID RAW(16) NOT NULL, INSTRUMENT_ID RAW(16) NOT NULL, BOOKING_ID RAW(16), CONSTRAINT PK_HXS_LAB_EXP_INSTRUMENT PRIMARY KEY(ID), CONSTRAINT UK_HXS_LAB_EXP_INSTRUMENT UNIQUE(EXPERIMENT_ID,INSTRUMENT_ID), CONSTRAINT FK_EXP_INS_EXP FOREIGN KEY(EXPERIMENT_ID) REFERENCES HXS_LAB_EXPERIMENT(ID), CONSTRAINT FK_EXP_INS_INS FOREIGN KEY(INSTRUMENT_ID) REFERENCES HXS_LAB_INSTRUMENT(ID), CONSTRAINT FK_EXP_INS_BOOKING FOREIGN KEY(BOOKING_ID) REFERENCES HXS_LAB_BOOKING(ID))"),
            ("HXS_LAB_EXPERIMENT_MATERIAL", "CREATE TABLE HXS_LAB_EXPERIMENT_MATERIAL (ID RAW(16) NOT NULL, EXPERIMENT_ID RAW(16) NOT NULL, MATERIAL_ID RAW(16) NOT NULL, REQUISITION_ID RAW(16), QUANTITY NUMBER(18,4) NOT NULL, CONSTRAINT PK_HXS_LAB_EXP_MATERIAL PRIMARY KEY(ID), CONSTRAINT UK_HXS_LAB_EXP_MATERIAL UNIQUE(EXPERIMENT_ID,MATERIAL_ID), CONSTRAINT CK_HXS_LAB_EXP_MAT_QTY CHECK(QUANTITY>0), CONSTRAINT FK_EXP_MAT_EXP FOREIGN KEY(EXPERIMENT_ID) REFERENCES HXS_LAB_EXPERIMENT(ID), CONSTRAINT FK_EXP_MAT_MAT FOREIGN KEY(MATERIAL_ID) REFERENCES HXS_LAB_MATERIAL(ID), CONSTRAINT FK_EXP_MAT_REQ FOREIGN KEY(REQUISITION_ID) REFERENCES HXS_LAB_REQUISITION(ID))"),
            ("HXS_LAB_EXPERIMENT_RECORD", "CREATE TABLE HXS_LAB_EXPERIMENT_RECORD (ID RAW(16) NOT NULL, EXPERIMENT_ID RAW(16) NOT NULL, RECORD_TYPE VARCHAR2(30 CHAR) NOT NULL, CONTENT VARCHAR2(4000 CHAR) NOT NULL, RECORD_TIME TIMESTAMP(6) NOT NULL, CREATOR_ID RAW(16) NOT NULL, CONSTRAINT PK_HXS_LAB_EXP_RECORD PRIMARY KEY(ID), CONSTRAINT FK_EXP_RECORD_EXP FOREIGN KEY(EXPERIMENT_ID) REFERENCES HXS_LAB_EXPERIMENT(ID), CONSTRAINT FK_EXP_RECORD_USER FOREIGN KEY(CREATOR_ID) REFERENCES HXS_SYS_USER(ID))")
        };
        foreach (var table in tables)
            if (await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME=:name", new SugarParameter(":name", table.Name)) == 0)
                await _db.Ado.ExecuteCommandAsync(table.Ddl);
    }

    private async Task EnsureMenusAsync()
    {
        var root = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == "lab") ?? throw new InvalidOperationException("实验室管理根菜单不存在。");
        var now = DateTime.Now;
        var group = await UpsertMenuAsync("lab:experiment-group", "实验任务", "directory", null, null, "file", null, 50, root.Id, now);
        var mine = await UpsertMenuAsync("lab:experiments-mine", "我的实验", "page", "/lab/experiments", "views/lab/ExperimentView.vue", "file", "lab:experiment:view", 10, group.Id, now);
        var query = await UpsertMenuAsync("lab:experiments-query", "实验任务查询", "page", "/lab/experiment-query", "views/lab/ExperimentQueryView.vue", "search", "lab:experiment:view", 20, group.Id, now);
        var permissionDefs = new[]
        {
            ("lab:experiment:create", "新建实验", mine), ("lab:experiment:edit", "编辑实验", mine),
            ("lab:experiment:record", "记录实验", mine), ("lab:experiment:archive", "归档实验", mine),
            ("lab:experiment:unarchive", "解档实验", query)
        };
        var buttons = new List<SysMenu>();
        foreach (var (code, name, parent) in permissionDefs)
        {
            var button = await _db.Queryable<SysMenu>().FirstAsync(x => x.PermissionCode == code && x.MenuType == "button");
            if (button is null)
            {
                button = new SysMenu { Id=Guid.NewGuid().ToByteArray(),ParentId=parent.Id,MenuCode="permission:"+code,MenuName=name,MenuType="button",PermissionCode=code,SortNo=1000,IsVisible=0,IsActive=1,CreateTime=now,UpdateTime=now };
                await _db.Insertable(button).ExecuteCommandAsync();
            }
            else
            {
                button.ParentId=parent.Id;button.MenuName=name;button.IsVisible=0;button.IsActive=1;button.UpdateTime=now;
                await _db.Updateable(button).UpdateColumns(x=>new{x.ParentId,x.MenuName,x.IsVisible,x.IsActive,x.UpdateTime}).ExecuteCommandAsync();
            }
            buttons.Add(button);
        }
        foreach (var roleCode in new[] { "admin", "lab_admin", "lab_user" })
        {
            var role = await _db.Queryable<SysRole>().FirstAsync(x => x.RoleCode == roleCode); if (role is null) continue;
            IEnumerable<SysMenu> allowed = roleCode switch
            {
                "admin" => new[] { root, group, mine, query }.Concat(buttons),
                "lab_admin" => new[] { root, group, query },
                _ => new[] { root, group, mine }.Concat(buttons.Where(x => x.PermissionCode != "lab:experiment:unarchive"))
            };
            foreach (var menu in allowed)
                if (!await _db.Queryable<SysRoleMenu>().AnyAsync(x => x.RoleId == role.Id && x.MenuId == menu.Id))
                    await _db.Insertable(new SysRoleMenu { Id=Guid.NewGuid().ToByteArray(),RoleId=role.Id,MenuId=menu.Id,CreateTime=now }).ExecuteCommandAsync();
        }
    }

    private async Task<SysMenu> UpsertMenuAsync(string code,string name,string type,string? route,string? component,string icon,string? permission,int sort,byte[] parentId,DateTime now)
    {
        var row=await _db.Queryable<SysMenu>().FirstAsync(x=>x.MenuCode==code);
        if(row is null){row=new SysMenu{Id=Guid.NewGuid().ToByteArray(),ParentId=parentId,MenuCode=code,MenuName=name,MenuType=type,RoutePath=route,Component=component,Icon=icon,PermissionCode=permission,SortNo=sort,IsVisible=1,IsActive=1,CreateTime=now,UpdateTime=now};await _db.Insertable(row).ExecuteCommandAsync();}
        else{row.ParentId=parentId;row.MenuName=name;row.MenuType=type;row.RoutePath=route;row.Component=component;row.Icon=icon;row.PermissionCode=permission;row.SortNo=sort;row.IsVisible=1;row.IsActive=1;row.UpdateTime=now;await _db.Updateable(row).ExecuteCommandAsync();}
        return row;
    }
}
