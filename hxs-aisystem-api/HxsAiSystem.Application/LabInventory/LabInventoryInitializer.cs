using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabInventory;

public sealed class LabInventoryInitializer : ILabInventoryInitializer
{
    private readonly ISqlSugarClient _db;
    public LabInventoryInitializer(ISqlSugarClient db) => _db = db;

    public async Task InitializeAsync()
    {
        await EnsureTablesAsync();
        await EnsureMenusAsync();
        await EnsureDemoDataAsync();
    }

    private async Task EnsureTablesAsync()
    {
        var tables = new (string Name, string Ddl)[]
        {
            ("HXS_LAB_MATERIAL", @"CREATE TABLE HXS_LAB_MATERIAL (ID RAW(16) NOT NULL, MATERIAL_CODE VARCHAR2(50 CHAR) NOT NULL, MATERIAL_NAME VARCHAR2(150 CHAR) NOT NULL, MATERIAL_TYPE VARCHAR2(20 CHAR) NOT NULL, CATEGORY_ID RAW(16), SPECIFICATION VARCHAR2(100 CHAR), CAS_NO VARCHAR2(80 CHAR), UNIT_ID RAW(16) NOT NULL, SUPPLIER_ID RAW(16), STORAGE_LOCATION_ID RAW(16) NOT NULL, MIN_STOCK NUMBER(18,4) DEFAULT 0 NOT NULL, DESCRIPTION VARCHAR2(500 CHAR), IS_ACTIVE NUMBER(1) DEFAULT 1 NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_MATERIAL PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_MATERIAL_CODE UNIQUE (MATERIAL_CODE), CONSTRAINT CK_HXS_LAB_MATERIAL_TYPE CHECK (MATERIAL_TYPE IN ('reagent','consumable')), CONSTRAINT FK_MATERIAL_CATEGORY FOREIGN KEY (CATEGORY_ID) REFERENCES HXS_SYS_DICT_ITEM(ID), CONSTRAINT FK_MATERIAL_UNIT FOREIGN KEY (UNIT_ID) REFERENCES HXS_SYS_DICT_ITEM(ID), CONSTRAINT FK_MATERIAL_SUPPLIER FOREIGN KEY (SUPPLIER_ID) REFERENCES HXS_LAB_SUPPLIER(ID), CONSTRAINT FK_MATERIAL_LOCATION FOREIGN KEY (STORAGE_LOCATION_ID) REFERENCES HXS_LAB_LOCATION(ID))"),
            ("HXS_LAB_STOCK_BATCH", @"CREATE TABLE HXS_LAB_STOCK_BATCH (ID RAW(16) NOT NULL, MATERIAL_ID RAW(16) NOT NULL, BATCH_NO VARCHAR2(80 CHAR) NOT NULL, PRODUCTION_DATE DATE, EXPIRY_DATE DATE, IN_QUANTITY NUMBER(18,4) NOT NULL, AVAILABLE_QUANTITY NUMBER(18,4) NOT NULL, UNIT_PRICE NUMBER(18,4), STOCK_IN_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_STOCK_BATCH PRIMARY KEY (ID), CONSTRAINT UK_HXS_STOCK_BATCH UNIQUE (MATERIAL_ID,BATCH_NO), CONSTRAINT CK_HXS_STOCK_BATCH_QTY CHECK (IN_QUANTITY > 0 AND AVAILABLE_QUANTITY >= 0), CONSTRAINT FK_STOCK_BATCH_MATERIAL FOREIGN KEY (MATERIAL_ID) REFERENCES HXS_LAB_MATERIAL(ID))"),
            ("HXS_LAB_STOCK_FLOW", @"CREATE TABLE HXS_LAB_STOCK_FLOW (ID RAW(16) NOT NULL, FLOW_NO VARCHAR2(40 CHAR) NOT NULL, MATERIAL_ID RAW(16) NOT NULL, BATCH_ID RAW(16) NOT NULL, FLOW_TYPE VARCHAR2(20 CHAR) NOT NULL, QUANTITY NUMBER(18,4) NOT NULL, BEFORE_QUANTITY NUMBER(18,4) NOT NULL, AFTER_QUANTITY NUMBER(18,4) NOT NULL, SOURCE_TYPE VARCHAR2(30 CHAR) NOT NULL, SOURCE_ID RAW(16), OPERATOR_ID RAW(16) NOT NULL, REMARK VARCHAR2(500 CHAR), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_STOCK_FLOW PRIMARY KEY (ID), CONSTRAINT UK_HXS_STOCK_FLOW_NO UNIQUE (FLOW_NO), CONSTRAINT FK_STOCK_FLOW_MATERIAL FOREIGN KEY (MATERIAL_ID) REFERENCES HXS_LAB_MATERIAL(ID), CONSTRAINT FK_STOCK_FLOW_BATCH FOREIGN KEY (BATCH_ID) REFERENCES HXS_LAB_STOCK_BATCH(ID), CONSTRAINT FK_STOCK_FLOW_USER FOREIGN KEY (OPERATOR_ID) REFERENCES HXS_SYS_USER(ID))"),
            ("HXS_LAB_REQUISITION", @"CREATE TABLE HXS_LAB_REQUISITION (ID RAW(16) NOT NULL, REQUISITION_NO VARCHAR2(40 CHAR) NOT NULL, APPLICANT_ID RAW(16) NOT NULL, GROUP_ID RAW(16), PURPOSE VARCHAR2(500 CHAR) NOT NULL, STATUS VARCHAR2(20 CHAR) NOT NULL, APPROVER_ID RAW(16), APPROVE_TIME TIMESTAMP(6), APPROVE_REMARK VARCHAR2(500 CHAR), CREATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, UPDATE_TIME TIMESTAMP(6) DEFAULT SYSTIMESTAMP NOT NULL, CONSTRAINT PK_HXS_LAB_REQUISITION PRIMARY KEY (ID), CONSTRAINT UK_HXS_REQUISITION_NO UNIQUE (REQUISITION_NO), CONSTRAINT FK_REQ_APPLICANT FOREIGN KEY (APPLICANT_ID) REFERENCES HXS_SYS_USER(ID), CONSTRAINT FK_REQ_GROUP FOREIGN KEY (GROUP_ID) REFERENCES HXS_LAB_GROUP(ID), CONSTRAINT FK_REQ_APPROVER FOREIGN KEY (APPROVER_ID) REFERENCES HXS_SYS_USER(ID))"),
            ("HXS_LAB_REQUISITION_ITEM", @"CREATE TABLE HXS_LAB_REQUISITION_ITEM (ID RAW(16) NOT NULL, REQUISITION_ID RAW(16) NOT NULL, MATERIAL_ID RAW(16) NOT NULL, REQUEST_QUANTITY NUMBER(18,4) NOT NULL, APPROVED_QUANTITY NUMBER(18,4), CONSTRAINT PK_HXS_LAB_REQ_ITEM PRIMARY KEY (ID), CONSTRAINT UK_HXS_LAB_REQ_ITEM UNIQUE (REQUISITION_ID,MATERIAL_ID), CONSTRAINT FK_REQ_ITEM_REQ FOREIGN KEY (REQUISITION_ID) REFERENCES HXS_LAB_REQUISITION(ID), CONSTRAINT FK_REQ_ITEM_MATERIAL FOREIGN KEY (MATERIAL_ID) REFERENCES HXS_LAB_MATERIAL(ID))")
        };
        foreach (var table in tables)
            if (await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME=:name", new SugarParameter(":name", table.Name)) == 0)
                await _db.Ado.ExecuteCommandAsync(table.Ddl);
        if (await _db.Ado.GetIntAsync("SELECT COUNT(*) FROM USER_INDEXES WHERE INDEX_NAME='IX_HXS_STOCK_BATCH_PICK'") == 0)
            await _db.Ado.ExecuteCommandAsync("CREATE INDEX IX_HXS_STOCK_BATCH_PICK ON HXS_LAB_STOCK_BATCH(MATERIAL_ID,EXPIRY_DATE,AVAILABLE_QUANTITY)");
    }

    private async Task EnsureMenusAsync()
    {
        var root = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == "lab") ?? throw new InvalidOperationException("实验室管理根菜单不存在。");
        var now = DateTime.Now;
        var groupDefs = new[]
        {
            ("lab:foundation-group","基础资料","apps",10),
            ("lab:instrument-group","仪器业务","experiment",20),
            ("lab:inventory-group","库存业务","database",30),
            ("lab:approval-group","审批中心","check",40)
        };
        var groups = new Dictionary<string,SysMenu>(StringComparer.OrdinalIgnoreCase);
        foreach(var d in groupDefs)
        {
            var group=await _db.Queryable<SysMenu>().FirstAsync(x=>x.MenuCode==d.Item1);
            if(group is null){group=new SysMenu{Id=Guid.NewGuid().ToByteArray(),ParentId=root.Id,MenuCode=d.Item1,MenuName=d.Item2,MenuType="directory",Icon=d.Item3,SortNo=d.Item4,IsVisible=1,IsActive=1,CreateTime=now,UpdateTime=now};await _db.Insertable(group).ExecuteCommandAsync();}
            else{group.ParentId=root.Id;group.MenuName=d.Item2;group.MenuType="directory";group.Icon=d.Item3;group.SortNo=d.Item4;group.IsVisible=1;group.IsActive=1;group.UpdateTime=now;await _db.Updateable(group).ExecuteCommandAsync();}
            groups[d.Item1]=group;
        }

        var existingPageGroups = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
        {
            ["lab:labs"]="lab:foundation-group",["lab:locations"]="lab:foundation-group",["lab:groups"]="lab:foundation-group",["lab:suppliers"]="lab:foundation-group",["lab:dictionaries"]="lab:foundation-group",
            ["lab:instruments"]="lab:instrument-group",["lab:bookings"]="lab:instrument-group",["lab:usages"]="lab:instrument-group",["lab:repairs"]="lab:instrument-group",
            ["lab:booking-approvals"]="lab:approval-group"
        };
        var existingPages=await _db.Queryable<SysMenu>().Where(x=>x.MenuType=="page").ToListAsync();
        foreach(var page in existingPages.Where(x=>existingPageGroups.ContainsKey(x.MenuCode))){page.ParentId=groups[existingPageGroups[page.MenuCode]].Id;page.UpdateTime=now;await _db.Updateable(page).UpdateColumns(x=>new{x.ParentId,x.UpdateTime}).ExecuteCommandAsync();}
        var defs = new[]
        {
            ("lab:materials","试剂耗材","/lab/materials","views/lab/MaterialView.vue","shop","lab:inventory:view",10,"lab:inventory-group"),
            ("lab:stock-batches","批次库存","/lab/stock-batches","views/lab/StockBatchView.vue","database","lab:inventory:view",20,"lab:inventory-group"),
            ("lab:stock-flows","库存流水","/lab/stock-flows","views/lab/StockFlowView.vue","history","lab:inventory:view",30,"lab:inventory-group"),
            ("lab:requisitions","领用申请","/lab/requisitions","views/lab/RequisitionView.vue","file","lab:requisition:view",40,"lab:inventory-group"),
            ("lab:requisition-approvals","领用审批","/lab/requisition-approvals","views/lab/RequisitionApprovalView.vue","check","lab:requisition:approve",20,"lab:approval-group"),
            ("lab:inventory-warnings","库存预警","/lab/inventory-warnings","views/lab/InventoryWarningView.vue","chart","lab:inventory:view",50,"lab:inventory-group")
        };
        var pages = new List<SysMenu>();
        foreach (var d in defs)
        {
            var row = await _db.Queryable<SysMenu>().FirstAsync(x => x.MenuCode == d.Item1);
            if (row is null) { row = new SysMenu { Id=Guid.NewGuid().ToByteArray(),ParentId=groups[d.Item8].Id,MenuCode=d.Item1,MenuName=d.Item2,MenuType="page",RoutePath=d.Item3,Component=d.Item4,Icon=d.Item5,PermissionCode=d.Item6,SortNo=d.Item7,IsVisible=1,IsActive=1,CreateTime=now,UpdateTime=now }; await _db.Insertable(row).ExecuteCommandAsync(); }
            else { row.ParentId=groups[d.Item8].Id;row.MenuName=d.Item2;row.RoutePath=d.Item3;row.Component=d.Item4;row.Icon=d.Item5;row.PermissionCode=d.Item6;row.SortNo=d.Item7;row.IsVisible=1;row.IsActive=1;row.UpdateTime=now;await _db.Updateable(row).ExecuteCommandAsync(); }
            pages.Add(row);
        }
        var permissions = new[] { ("lab:material:manage","维护试剂耗材","lab:materials"),("lab:stock:in","登记入库","lab:stock-batches"),("lab:stock:adjust","调整库存","lab:stock-batches"),("lab:requisition:create","提交领用","lab:requisitions"),("lab:requisition:cancel","取消领用","lab:requisitions"),("lab:requisition:approve","审批领用","lab:requisition-approvals") };
        var buttons = new List<SysMenu>();
        foreach (var p in permissions)
        {
            var parent = pages.First(x=>x.MenuCode==p.Item3); var row=await _db.Queryable<SysMenu>().FirstAsync(x=>x.PermissionCode==p.Item1&&x.MenuType=="button");
            if(row is null){row=new SysMenu{Id=Guid.NewGuid().ToByteArray(),ParentId=parent.Id,MenuCode="permission:"+p.Item1,MenuName=p.Item2,MenuType="button",PermissionCode=p.Item1,SortNo=1000,IsVisible=0,IsActive=1,CreateTime=now,UpdateTime=now};await _db.Insertable(row).ExecuteCommandAsync();}
            else{row.ParentId=parent.Id;row.MenuCode="permission:"+p.Item1;row.MenuName=p.Item2;row.IsVisible=0;row.IsActive=1;row.UpdateTime=now;await _db.Updateable(row).UpdateColumns(x=>new{x.ParentId,x.MenuCode,x.MenuName,x.IsVisible,x.IsActive,x.UpdateTime}).ExecuteCommandAsync();}
            buttons.Add(row);
        }
        foreach(var roleCode in new[]{"admin","lab_admin","lab_user"})
        {
            var role=await _db.Queryable<SysRole>().FirstAsync(x=>x.RoleCode==roleCode);if(role is null)continue;
            var allowed=roleCode=="lab_user"?new[]{root}.Concat(groups.Values.Where(x=>x.MenuCode!="lab:approval-group")).Concat(pages.Where(x=>x.MenuCode!="lab:requisition-approvals")).Concat(buttons.Where(x=>x.PermissionCode is "lab:requisition:create" or "lab:requisition:cancel")):new[]{root}.Concat(groups.Values).Concat(pages).Concat(buttons);
            foreach(var menu in allowed)if(!await _db.Queryable<SysRoleMenu>().AnyAsync(x=>x.RoleId==role.Id&&x.MenuId==menu.Id))await _db.Insertable(new SysRoleMenu{Id=Guid.NewGuid().ToByteArray(),RoleId=role.Id,MenuId=menu.Id,CreateTime=now}).ExecuteCommandAsync();
        }
    }

    private async Task EnsureDemoDataAsync()
    {
        if (await _db.Queryable<LabMaterial>().AnyAsync()) return;
        var location=await _db.Queryable<LabLocation>().FirstAsync(x=>x.IsActive==1); var supplier=await _db.Queryable<LabSupplier>().FirstAsync(x=>x.IsActive==1);
        var types=await _db.Queryable<SysDictType>().ToListAsync(); var typeIds=types.ToDictionary(x=>x.DictCode,x=>x.Id);
        if(location is null||!typeIds.TryGetValue("measurement_unit",out var unitType))return;
        var unit=await _db.Queryable<SysDictItem>().FirstAsync(x=>x.DictTypeId==unitType&&x.IsActive==1);if(unit is null)return;
        byte[]? category=null;if(typeIds.TryGetValue("reagent_category",out var ct))category=(await _db.Queryable<SysDictItem>().FirstAsync(x=>x.DictTypeId==ct&&x.IsActive==1))?.Id;
        var now=DateTime.Now;var material=new LabMaterial{Id=Guid.NewGuid().ToByteArray(),MaterialCode="RGT-001",MaterialName="无水乙醇",MaterialType="reagent",CategoryId=category,Specification="AR 500ml",CasNo="64-17-5",UnitId=unit.Id,SupplierId=supplier?.Id,StorageLocationId=location.Id,MinStock=10,Description="阶段4演示试剂",IsActive=1,CreateTime=now,UpdateTime=now};await _db.Insertable(material).ExecuteCommandAsync();
        var batch=new LabStockBatch{Id=Guid.NewGuid().ToByteArray(),MaterialId=material.Id,BatchNo="DEMO-202608",ProductionDate=now.Date.AddMonths(-1),ExpiryDate=now.Date.AddYears(2),InQuantity=50,AvailableQuantity=50,UnitPrice=25.8m,StockInTime=now,CreateTime=now,UpdateTime=now};await _db.Insertable(batch).ExecuteCommandAsync();
        var admin=await _db.Queryable<AppUser>().FirstAsync(x=>x.UserName=="admin");if(admin is not null)await _db.Insertable(new LabStockFlow{Id=Guid.NewGuid().ToByteArray(),FlowNo="FL"+now.ToString("yyyyMMddHHmmssfff"),MaterialId=material.Id,BatchId=batch.Id,FlowType="in",Quantity=50,BeforeQuantity=0,AfterQuantity=50,SourceType="stock_in",OperatorId=admin.Id,Remark="阶段4演示入库",CreateTime=now}).ExecuteCommandAsync();
    }
}
