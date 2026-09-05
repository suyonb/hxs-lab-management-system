using HxsAiSystem.Domain.Entities;
using SqlSugar;
using LabInstrumentEntity=HxsAiSystem.Domain.Entities.LabInstrument;

namespace HxsAiSystem.Application.LabVisualization;

public sealed class LabVisualizationInitializer(ISqlSugarClient db):ILabVisualizationInitializer
{
    public async Task InitializeAsync()
    {
        await LabVisualizationSchema.EnsureTables(db);var root=await db.Queryable<SysMenu>().FirstAsync(x=>x.MenuCode=="lab")??throw new InvalidOperationException("实验室菜单不存在。");var now=DateTime.Now;
        await LabSpatialDemoSeeder.Seed(db,now);
        var group=await Menu("lab:visual-group","空间可视化","directory",null,null,"apps",null,80,root.Id,now);
        var page=await Menu("lab:3d","3D 实验室","page","/lab/3d","views/lab/Lab3dView.vue","experiment","lab:3d:view",1,group.Id,now);
        var view=await Menu("permission:lab:3d:view","查看三维实验室","button",null,null,"check","lab:3d:view",1,page.Id,now);
        var managePage=await Menu("lab:3d-manage","3D 场景管理","page","/lab/3d/manage","views/lab/Lab3dManageView.vue","setting","lab:3d:manage",2,group.Id,now);
        var manage=await Menu("permission:lab:3d:manage","维护三维场景","button",null,null,"edit","lab:3d:manage",1,managePage.Id,now);
        foreach(var code in new[]{"admin","lab_admin","lab_user"})
        {
            var role=await db.Queryable<SysRole>().FirstAsync(x=>x.RoleCode==code);if(role is null)continue;
            foreach(var menu in new[]{root,group,page,view})await Grant(role,menu,now);
            if(code is "admin" or "lab_admin")foreach(var menu in new[]{managePage,manage})await Grant(role,menu,now);
        }
        await Seed(now);
    }

    private async Task Seed(DateTime now)
    {
        var scenes=await db.Queryable<Lab3dScene>().ToListAsync();
        if(scenes.Count==0)
        {
            var lab=await db.Queryable<Lab>().FirstAsync(x=>x.IsActive==1);if(lab is null)return;
            var scene=new Lab3dScene{Id=Guid.NewGuid().ToByteArray(),LabId=lab.Id,SceneName=$"{lab.LabName}数字空间",BackgroundColor="#eef3f5",Version=1,IsActive=1,CreateTime=now,UpdateTime=now};
            await db.Insertable(scene).ExecuteCommandAsync();scenes.Add(scene);
        }

        foreach(var scene in scenes)
        {
            if(await db.Queryable<Lab3dNode>().AnyAsync(x=>x.SceneId==scene.Id))continue;
            var sort=0;
            var locations=(await db.Queryable<LabLocation>().Where(x=>x.LabId==scene.LabId&&x.IsActive==1).OrderBy(x=>x.SortNo).ToListAsync()).Where(x=>x.LocationType=="room").Take(12).ToList();
            foreach(var location in locations)
            {
                sort++;
                await AddNode(scene,$"LOC-{sort:00}",location.LocationName,"location",location.Id,(sort-1)%4*3-4.5m,.12m,(sort-1)/4*3-3m,sort,now);
            }

            var instruments=(await db.Queryable<LabInstrumentEntity>().Where(x=>x.LabId==scene.LabId&&x.IsActive==1).ToListAsync()).Take(12).ToList();
            foreach(var instrument in instruments)
            {
                sort++;
                var index=sort-locations.Count-1;
                await AddNode(scene,$"INS-{index+1:00}",instrument.InstrumentName,"instrument",instrument.Id,index%4*3-4.5m,.65m,index/4*3-3m,sort,now);
            }

            if(sort==0)
            {
                var lab=await db.Queryable<Lab>().FirstAsync(x=>x.Id==scene.LabId);
                if(lab is not null)await AddNode(scene,"LAB-01",lab.LabName,"lab",lab.Id,0,0,0,1,now);
            }
        }
    }

    private async Task AddNode(Lab3dScene scene,string code,string name,string type,byte[] businessId,decimal x,decimal y,decimal z,int sort,DateTime now)
    {
        var node=new Lab3dNode{Id=Guid.NewGuid().ToByteArray(),SceneId=scene.Id,NodeCode=code,NodeName=name,NodeType=type,PositionX=x,PositionY=y,PositionZ=z,ScaleX=1,ScaleY=1,ScaleZ=1,SortNo=sort,CreateTime=now,UpdateTime=now};
        await db.Insertable(node).ExecuteCommandAsync();
        await db.Insertable(new Lab3dBinding{Id=Guid.NewGuid().ToByteArray(),NodeId=node.Id,BusinessType=type,BusinessId=businessId,CreateTime=now,UpdateTime=now}).ExecuteCommandAsync();
    }

    private async Task Grant(SysRole role,SysMenu menu,DateTime now){if(!await db.Queryable<SysRoleMenu>().AnyAsync(x=>x.RoleId==role.Id&&x.MenuId==menu.Id))await db.Insertable(new SysRoleMenu{Id=Guid.NewGuid().ToByteArray(),RoleId=role.Id,MenuId=menu.Id,CreateTime=now}).ExecuteCommandAsync();}
    private async Task<SysMenu> Menu(string code,string name,string type,string? route,string? component,string icon,string? permission,int sort,byte[] parent,DateTime now){var x=await db.Queryable<SysMenu>().FirstAsync(y=>y.MenuCode==code);if(x is null)x=new SysMenu{Id=Guid.NewGuid().ToByteArray(),CreateTime=now};x.ParentId=parent;x.MenuCode=code;x.MenuName=name;x.MenuType=type;x.RoutePath=route;x.Component=component;x.Icon=icon;x.PermissionCode=permission;x.SortNo=sort;x.IsVisible=type=="button"?0:1;x.IsActive=1;x.UpdateTime=now;if(await db.Queryable<SysMenu>().AnyAsync(y=>y.Id==x.Id))await db.Updateable(x).ExecuteCommandAsync();else await db.Insertable(x).ExecuteCommandAsync();return x;}
}
