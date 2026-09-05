using HxsAiSystem.Domain.Entities;
using SqlSugar;
using LabInstrumentEntity = HxsAiSystem.Domain.Entities.LabInstrument;

namespace HxsAiSystem.Application.LabVisualization;

internal static class LabSpatialDemoSeeder
{
    public static async Task Seed(ISqlSugarClient db, DateTime now)
    {
        var chemistry = await db.Queryable<Lab>().FirstAsync(x => x.LabCode == "LAB-CHEM");
        var instrument = await db.Queryable<Lab>().FirstAsync(x => x.LabCode == "LAB-INST");
        if (chemistry is not null)
        {
            var building = await Location(db, chemistry.Id, null, "CHEM-B1", "实验楼 A 栋", "building", 10, now);
            var floors = new List<LabLocation>();
            for (var floor = 1; floor <= 3; floor++)
                floors.Add(await Location(db, chemistry.Id, building.Id, $"CHEM-F{floor}", $"{floor} 层", "floor", floor * 10, now));
            var rooms = new[]
            {
                await Location(db,chemistry.Id,floors[0].Id,"CHEM-R101","101 样品制备室","room",11,now),
                await Location(db,chemistry.Id,floors[0].Id,"CHEM-R102","102 试剂配制室","room",12,now),
                await Location(db,chemistry.Id,floors[1].Id,"CHEM-R201","201 分析检测室","room",21,now),
                await Location(db,chemistry.Id,floors[1].Id,"CHEM-R202","202 精密称量室","room",22,now),
                await Location(db,chemistry.Id,floors[2].Id,"CHEM-R301","301 化学实验室","room",31,now),
                await Location(db,chemistry.Id,floors[2].Id,"CHEM-R302","302 安全操作室","room",32,now)
            };
            await Instrument(db,"VIS-CHEM-101","恒温磁力搅拌器",chemistry.Id,rooms[0].Id,"normal",now);
            await Instrument(db,"VIS-CHEM-102","纯水制备系统",chemistry.Id,rooms[1].Id,"normal",now);
            await Instrument(db,"VIS-CHEM-201","气相色谱仪",chemistry.Id,rooms[2].Id,"repair",now);
            await Instrument(db,"VIS-CHEM-202","电子分析天平",chemistry.Id,rooms[3].Id,"normal",now);
            await Instrument(db,"VIS-CHEM-301","高速离心机",chemistry.Id,rooms[4].Id,"normal",now);
            await Instrument(db,"VIS-CHEM-302","防爆通风橱",chemistry.Id,rooms[5].Id,"stopped",now);
        }
        if (instrument is not null)
        {
            var building = await Location(db,instrument.Id,null,"INST-B1","实验楼 B 栋","building",10,now);
            var floors=new List<LabLocation>();for(var floor=1;floor<=3;floor++)floors.Add(await Location(db,instrument.Id,building.Id,$"INST-F{floor}",$"{floor} 层","floor",floor*10,now));
            await Location(db,instrument.Id,floors[1].Id,"INST-R201","201 仪器共享中心","room",21,now);
        }
    }

    private static async Task<LabLocation> Location(ISqlSugarClient db,byte[] labId,byte[]? parentId,string code,string name,string type,int sort,DateTime now)
    {
        var row=await db.Queryable<LabLocation>().FirstAsync(x=>x.LocationCode==code);if(row is null){row=new LabLocation{Id=Guid.NewGuid().ToByteArray(),LabId=labId,ParentId=parentId,LocationCode=code,LocationName=name,LocationType=type,SortNo=sort,IsActive=1,CreateTime=now,UpdateTime=now};await db.Insertable(row).ExecuteCommandAsync();}else if(parentId is not null&&(row.ParentId is null||!row.ParentId.SequenceEqual(parentId))){row.ParentId=parentId;row.UpdateTime=now;await db.Updateable(row).UpdateColumns(x=>new{x.ParentId,x.UpdateTime}).ExecuteCommandAsync();}return row;
    }
    private static async Task Instrument(ISqlSugarClient db,string code,string name,byte[] labId,byte[] locationId,string status,DateTime now)
    {
        if(await db.Queryable<LabInstrumentEntity>().AnyAsync(x=>x.InstrumentCode==code))return;await db.Insertable(new LabInstrumentEntity{Id=Guid.NewGuid().ToByteArray(),InstrumentCode=code,InstrumentName=name,LabId=labId,LocationId=locationId,Status=status,Description="空间可视化基础演示数据",IsActive=1,CreateTime=now,UpdateTime=now}).ExecuteCommandAsync();
    }
}
