using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;
using LabInstrumentEntity = HxsAiSystem.Domain.Entities.LabInstrument;

namespace HxsAiSystem.Application.LabVisualization;

public sealed class LabVisualizationService(ISqlSugarClient db,IDataScopeService dataScope):ILabVisualizationService
{
    public async Task<List<Lab3dSceneDto>> GetScenesAsync()
    {
        var labs=(await db.Queryable<Lab>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));
        return (await db.Queryable<Lab3dScene>().Where(x=>x.IsActive==1).OrderBy(x=>x.SceneName).ToListAsync()).Select(x=>Map(x,labs)).ToList();
    }

    public async Task<Lab3dSceneDetailDto> GetSceneAsync(Guid id)
    {
        var scene=await FindScene(id);var lab=await db.Queryable<Lab>().FirstAsync(x=>x.Id==scene.LabId);
        var nodes=await db.Queryable<Lab3dNode>().Where(x=>x.SceneId==scene.Id).OrderBy(x=>x.SortNo).ToListAsync();
        var bindings=await db.Queryable<Lab3dBinding>().ToListAsync();
        var labs=lab is null?new Dictionary<Guid,Lab>():new Dictionary<Guid,Lab>{{RawGuidConverter.ToGuid(scene.LabId),lab}};
        return new(Map(scene,labs),await MapNodes(nodes,bindings));
    }

    public async Task<List<Lab3dNodeStatusDto>> GetStatusesAsync(Guid id)
    {
        var scene=await GetSceneAsync(id);return scene.Nodes.Select(x=>new Lab3dNodeStatusDto(x.Id,x.Status,x.Detail)).ToList();
    }

    public async Task<List<Lab3dSceneManageDto>> GetManageScenesAsync()
    {
        await EnsureManager();var labs=(await db.Queryable<Lab>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));
        var scenes=await db.Queryable<Lab3dScene>().OrderByDescending(x=>x.UpdateTime).ToListAsync();var nodes=await db.Queryable<Lab3dNode>().ToListAsync();
        return scenes.Select(x=>new Lab3dSceneManageDto(Map(x,labs),nodes.Count(n=>n.SceneId.SequenceEqual(x.Id)),x.CreateTime,x.UpdateTime)).ToList();
    }

    public async Task<Lab3dSceneManageDto> CreateSceneAsync(Lab3dSceneRequest request)
    {
        await EnsureManager();LabVisualizationRules.ValidateScene(request);var labId=RawGuidConverter.ToRaw(request.LabId);var lab=await FindLab(labId);
        if(await db.Queryable<Lab3dScene>().AnyAsync(x=>x.LabId==labId&&x.SceneName==request.SceneName.Trim()))throw new InvalidOperationException("该实验室已存在同名场景。");
        var now=DateTime.Now;var row=new Lab3dScene{Id=Guid.NewGuid().ToByteArray(),LabId=labId,SceneName=request.SceneName.Trim(),BackgroundColor=request.BackgroundColor.Trim(),Version=1,IsActive=request.IsActive?1:0,CreateTime=now,UpdateTime=now};
        await db.Insertable(row).ExecuteCommandAsync();return new(Map(row,new(){{request.LabId,lab}}),0,now,now);
    }

    public async Task<Lab3dSceneManageDto> UpdateSceneAsync(Guid id,Lab3dSceneRequest request)
    {
        await EnsureManager();LabVisualizationRules.ValidateScene(request);var row=await FindScene(id);var labId=RawGuidConverter.ToRaw(request.LabId);var lab=await FindLab(labId);
        if(await db.Queryable<Lab3dScene>().AnyAsync(x=>x.Id!=row.Id&&x.LabId==labId&&x.SceneName==request.SceneName.Trim()))throw new InvalidOperationException("该实验室已存在同名场景。");
        row.LabId=labId;row.SceneName=request.SceneName.Trim();row.BackgroundColor=request.BackgroundColor.Trim();row.IsActive=request.IsActive?1:0;row.UpdateTime=DateTime.Now;
        await db.Updateable(row).UpdateColumns(x=>new{x.LabId,x.SceneName,x.BackgroundColor,x.IsActive,x.UpdateTime}).ExecuteCommandAsync();
        var count=await db.Queryable<Lab3dNode>().CountAsync(x=>x.SceneId==row.Id);return new(Map(row,new(){{request.LabId,lab}}),count,row.CreateTime,row.UpdateTime);
    }

    public async Task DeleteSceneAsync(Guid id)
    {
        await EnsureManager();var row=await FindScene(id);var nodes=await db.Queryable<Lab3dNode>().Where(x=>x.SceneId==row.Id).ToListAsync();
        await db.Ado.BeginTranAsync();try{foreach(var node in nodes)await db.Deleteable<Lab3dBinding>().Where(x=>x.NodeId==node.Id).ExecuteCommandAsync();await db.Deleteable<Lab3dNode>().Where(x=>x.SceneId==row.Id).ExecuteCommandAsync();await db.Deleteable<Lab3dScene>().Where(x=>x.Id==row.Id).ExecuteCommandAsync();await db.Ado.CommitTranAsync();}catch{await db.Ado.RollbackTranAsync();throw;}
    }

    public async Task<Lab3dNodeDto> CreateNodeAsync(Guid sceneId,Lab3dNodeRequest request)
    {
        await EnsureManager();LabVisualizationRules.ValidateNode(request);var scene=await FindScene(sceneId);
        if(await db.Queryable<Lab3dNode>().AnyAsync(x=>x.SceneId==scene.Id&&x.NodeCode==request.Code.Trim()))throw new InvalidOperationException("场景内节点编码不能重复。");
        var row=MapNode(new Lab3dNode{Id=Guid.NewGuid().ToByteArray(),SceneId=scene.Id,CreateTime=DateTime.Now},request);await db.Insertable(row).ExecuteCommandAsync();return (await MapNodes([row],[]))[0];
    }

    public async Task<Lab3dNodeDto> UpdateNodeAsync(Guid nodeId,Lab3dNodeRequest request)
    {
        await EnsureManager();LabVisualizationRules.ValidateNode(request);var row=await FindNode(nodeId);
        if(await db.Queryable<Lab3dNode>().AnyAsync(x=>x.Id!=row.Id&&x.SceneId==row.SceneId&&x.NodeCode==request.Code.Trim()))throw new InvalidOperationException("场景内节点编码不能重复。");
        MapNode(row,request);await db.Updateable(row).ExecuteCommandAsync();var binding=await db.Queryable<Lab3dBinding>().Where(x=>x.NodeId==row.Id).ToListAsync();return (await MapNodes([row],binding))[0];
    }

    public async Task DeleteNodeAsync(Guid nodeId)
    {
        await EnsureManager();var row=await FindNode(nodeId);await db.Ado.BeginTranAsync();try{await db.Deleteable<Lab3dBinding>().Where(x=>x.NodeId==row.Id).ExecuteCommandAsync();await db.Deleteable<Lab3dNode>().Where(x=>x.Id==row.Id).ExecuteCommandAsync();await db.Ado.CommitTranAsync();}catch{await db.Ado.RollbackTranAsync();throw;}
    }

    public async Task<Lab3dNodeDto> SetBindingAsync(Guid nodeId,Lab3dBindingRequest request)
    {
        await EnsureManager();var type=request.BusinessType.Trim().ToLowerInvariant();if(!LabVisualizationRules.SupportedTypes.Contains(type))throw new InvalidOperationException("业务绑定类型无效。");var node=await FindNode(nodeId);var scene=await db.Queryable<Lab3dScene>().FirstAsync(x=>x.Id==node.SceneId)??throw new KeyNotFoundException("三维场景不存在。");
        await ValidateBusiness(scene,type,request.BusinessId);var current=await db.Queryable<Lab3dBinding>().FirstAsync(x=>x.NodeId==node.Id);var now=DateTime.Now;
        if(current is null){current=new Lab3dBinding{Id=Guid.NewGuid().ToByteArray(),NodeId=node.Id,BusinessType=type,BusinessId=RawGuidConverter.ToRaw(request.BusinessId),CreateTime=now,UpdateTime=now};await db.Insertable(current).ExecuteCommandAsync();}
        else{current.BusinessType=type;current.BusinessId=RawGuidConverter.ToRaw(request.BusinessId);current.UpdateTime=now;await db.Updateable(current).ExecuteCommandAsync();}
        return (await MapNodes([node],[current]))[0];
    }

    public async Task RemoveBindingAsync(Guid nodeId){await EnsureManager();var row=await FindNode(nodeId);await db.Deleteable<Lab3dBinding>().Where(x=>x.NodeId==row.Id).ExecuteCommandAsync();}

    public async Task AttachModelAsync(Guid sceneId,Guid fileId)
    {
        await EnsureManager();var scene=await FindScene(sceneId);var file=await db.Queryable<SysFileRecord>().FirstAsync(x=>x.Id==RawGuidConverter.ToRaw(fileId))??throw new KeyNotFoundException("模型文件不存在。");
        if(file.BusinessType!="lab-3d-model"||file.BusinessId!=sceneId.ToString())throw new InvalidOperationException("模型文件与场景不匹配。");
        scene.Version=scene.ModelFileId is null?1:Math.Max(1,scene.Version)+1;scene.ModelFileId=file.Id;scene.ModelUrl=$"/api/lab/3d/scenes/{sceneId}/model";scene.UpdateTime=DateTime.Now;
        await db.Updateable(scene).UpdateColumns(x=>new{x.ModelFileId,x.ModelUrl,x.Version,x.UpdateTime}).ExecuteCommandAsync();
    }

    public async Task ActivateModelVersionAsync(Guid sceneId,Guid fileId)
    {
        await EnsureManager();var scene=await FindScene(sceneId);var file=await db.Queryable<SysFileRecord>().FirstAsync(x=>x.Id==RawGuidConverter.ToRaw(fileId))??throw new KeyNotFoundException("模型版本不存在。");
        if(file.BusinessType!="lab-3d-model"||file.BusinessId!=sceneId.ToString())throw new InvalidOperationException("模型版本与场景不匹配。");
        if(scene.ModelFileId?.SequenceEqual(file.Id)==true)return;scene.ModelFileId=file.Id;scene.ModelUrl=$"/api/lab/3d/scenes/{sceneId}/model";scene.Version=Math.Max(1,scene.Version)+1;scene.UpdateTime=DateTime.Now;
        await db.Updateable(scene).UpdateColumns(x=>new{x.ModelFileId,x.ModelUrl,x.Version,x.UpdateTime}).ExecuteCommandAsync();
    }

    public async Task<Guid> GetModelFileIdAsync(Guid sceneId){var scene=await FindScene(sceneId);return scene.ModelFileId is null?throw new KeyNotFoundException("该场景尚未上传模型。"):RawGuidConverter.ToGuid(scene.ModelFileId);}

    public async Task<List<Lab3dModelVersionDto>> GetModelVersionsAsync(Guid sceneId)
    {
        await EnsureManager();var scene=await FindScene(sceneId);var id=sceneId.ToString();var rows=await db.Queryable<SysFileRecord>().Where(x=>x.BusinessType=="lab-3d-model"&&x.BusinessId==id).OrderByDescending(x=>x.CreateTime).ToListAsync();
        return rows.Select(x=>new Lab3dModelVersionDto(RawGuidConverter.ToGuid(x.Id),x.OriginalName,x.FileSize,RawGuidConverter.ToGuid(x.UploaderId),x.CreateTime,scene.ModelFileId?.SequenceEqual(x.Id)==true)).ToList();
    }

    public async Task<List<LabSpatialLabDto>> GetSpatialLayoutAsync()
    {
        var now=DateTime.Now;var labs=await db.Queryable<Lab>().Where(x=>x.IsActive==1).OrderBy(x=>x.LabName).ToListAsync();var locations=await db.Queryable<LabLocation>().Where(x=>x.IsActive==1).OrderBy(x=>x.SortNo).ToListAsync();var instruments=await db.Queryable<LabInstrumentEntity>().Where(x=>x.IsActive==1).ToListAsync();var bookings=await db.Queryable<LabBooking>().Where(x=>x.EndTime>=now&&x.Status=="approved").ToListAsync();var repairs=await db.Queryable<LabRepair>().Where(x=>x.Status=="pending"||x.Status=="approved"||x.Status=="repairing").ToListAsync();var locMap=locations.ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));
        string Path(LabLocation x){var names=new List<string>{x.LocationName};var cursor=x;while(cursor.ParentId is not null&&locMap.TryGetValue(RawGuidConverter.ToGuid(cursor.ParentId),out var parent)){names.Insert(0,parent.LocationName);cursor=parent;}return string.Join(" / ",names);}
        LabLocation? RoomOf(LabInstrumentEntity x){if(!locMap.TryGetValue(RawGuidConverter.ToGuid(x.LocationId),out var cursor))return null;var fallback=cursor;while(true){if(cursor.LocationType=="room")return cursor;if(cursor.ParentId is null||!locMap.TryGetValue(RawGuidConverter.ToGuid(cursor.ParentId),out var parent))return fallback;cursor=parent;}}
        var result=new List<LabSpatialLabDto>();foreach(var lab in labs){var labId=RawGuidConverter.ToGuid(lab.Id);var labLocations=locations.Where(x=>RawGuidConverter.ToGuid(x.LabId)==labId).ToList();var rooms=labLocations.Where(x=>x.LocationType=="room").ToList();if(rooms.Count==0)rooms=labLocations.Where(x=>x.ParentId is null).ToList();var roomDtos=new List<LabSpatialRoomDto>();foreach(var room in rooms){var roomId=RawGuidConverter.ToGuid(room.Id);var path=Path(room);var pathParts=path.Split(" / ");var building=pathParts.FirstOrDefault()??lab.LabName;var floorName=pathParts.FirstOrDefault(x=>x.Contains('层'))??"1 层";var floorNumber=ParseFloor(floorName);var items=instruments.Where(x=>RawGuidConverter.ToGuid(x.LabId)==labId&&RoomOf(x)?.Id.SequenceEqual(room.Id)==true).Select(x=>{var itemBookings=bookings.Where(b=>b.InstrumentId.SequenceEqual(x.Id)).OrderBy(b=>b.StartTime).ToList();var repair=repairs.Where(r=>r.InstrumentId.SequenceEqual(x.Id)).OrderByDescending(r=>r.CreateTime).FirstOrDefault();var status=repair is null?x.Status:"repair";return new LabSpatialInstrumentDto(RawGuidConverter.ToGuid(x.Id),x.InstrumentCode,x.InstrumentName,status,x.Model,locMap.GetValueOrDefault(RawGuidConverter.ToGuid(x.LocationId))?.LocationName??room.LocationName,itemBookings.Count,itemBookings.FirstOrDefault()?.StartTime,repair?.Status);}).ToList();roomDtos.Add(new(roomId,room.LocationCode,room.LocationName,building,floorName,floorNumber,path,items.Sum(x=>x.UpcomingBookingCount),items.Count(x=>x.Status=="repair"||x.Status=="repairing"),items));}result.Add(new(labId,lab.LabCode,lab.LabName,lab.Description,roomDtos.OrderBy(x=>x.FloorNumber).ThenBy(x=>x.Code).ToList()));}return result;
    }

    public async Task<List<LabSpatialStatusDto>> GetSpatialStatusesAsync(Guid labId)
    {
        var lab=(await GetSpatialLayoutAsync()).FirstOrDefault(x=>x.Id==labId)??throw new KeyNotFoundException("实验室不存在或已停用。");var result=new List<LabSpatialStatusDto>();
        foreach(var room in lab.Rooms){var roomStatus=room.RepairingInstrumentCount>0?"repair":room.Instruments.Any(x=>x.Status=="stopped")?"stopped":"normal";var nextBooking=room.Instruments.Where(x=>x.NextBookingTime.HasValue).Select(x=>x.NextBookingTime).DefaultIfEmpty().Min();result.Add(new(room.Id,"room",roomStatus,$"{room.FullPath} · {room.Instruments.Count} 台仪器 · {room.UpcomingBookingCount} 个待执行预约",room.UpcomingBookingCount,room.RepairingInstrumentCount,nextBooking));foreach(var instrument in room.Instruments)result.Add(new(instrument.Id,"instrument",instrument.Status,$"{room.FullPath} / {instrument.LocationName} · {instrument.UpcomingBookingCount} 个待执行预约",instrument.UpcomingBookingCount,instrument.RepairStatus is null?0:1,instrument.NextBookingTime));}
        return result;
    }

    private async Task<List<Lab3dNodeDto>> MapNodes(List<Lab3dNode> nodes,List<Lab3dBinding> bindings){var instruments=await db.Queryable<LabInstrumentEntity>().ToListAsync();var locations=await db.Queryable<LabLocation>().ToListAsync();var labs=await db.Queryable<Lab>().ToListAsync();return nodes.Select(n=>{var b=bindings.FirstOrDefault(x=>x.NodeId.SequenceEqual(n.Id));var status="normal";string? detail=null;if(b?.BusinessType=="instrument"){var x=instruments.FirstOrDefault(y=>y.Id.SequenceEqual(b.BusinessId));status=x?.Status??"unknown";detail=x is null?null:$"{x.InstrumentCode} · {x.InstrumentName}";}else if(b?.BusinessType=="location"){var x=locations.FirstOrDefault(y=>y.Id.SequenceEqual(b.BusinessId));detail=x is null?null:$"{x.LocationCode} · {x.LocationName}";}else if(b?.BusinessType=="lab"){var x=labs.FirstOrDefault(y=>y.Id.SequenceEqual(b.BusinessId));detail=x is null?null:$"{x.LabCode} · {x.LabName}";}return new Lab3dNodeDto(RawGuidConverter.ToGuid(n.Id),n.NodeCode,n.NodeName,n.NodeType,n.PositionX,n.PositionY,n.PositionZ,n.ScaleX,n.ScaleY,n.ScaleZ,b?.BusinessType,b is null?null:RawGuidConverter.ToGuid(b.BusinessId),status,detail);}).ToList();}
    private static Lab3dSceneDto Map(Lab3dScene x,Dictionary<Guid,Lab> labs){var id=RawGuidConverter.ToGuid(x.LabId);return new(RawGuidConverter.ToGuid(x.Id),id,labs.GetValueOrDefault(id)?.LabName??"实验室",x.SceneName,x.ModelUrl,x.BackgroundColor,x.ModelFileId is null?null:RawGuidConverter.ToGuid(x.ModelFileId),Math.Max(1,x.Version),x.IsActive==1);}
    private async Task EnsureManager(){if(await dataScope.GetCurrentScopeAsync()==DataScope.Self)throw new UnauthorizedAccessException("仅实验管理员可以维护三维场景。");}
    private async Task<Lab3dScene> FindScene(Guid id)=>await db.Queryable<Lab3dScene>().FirstAsync(x=>x.Id==RawGuidConverter.ToRaw(id))??throw new KeyNotFoundException("三维场景不存在。");
    private async Task<Lab3dNode> FindNode(Guid id)=>await db.Queryable<Lab3dNode>().FirstAsync(x=>x.Id==RawGuidConverter.ToRaw(id))??throw new KeyNotFoundException("三维节点不存在。");
    private async Task<Lab> FindLab(byte[] id)=>await db.Queryable<Lab>().FirstAsync(x=>x.Id==id&&x.IsActive==1)??throw new KeyNotFoundException("实验室不存在或已停用。");
    private static Lab3dNode MapNode(Lab3dNode x,Lab3dNodeRequest r){x.NodeCode=r.Code.Trim();x.NodeName=r.Name.Trim();x.NodeType=r.Type.Trim().ToLowerInvariant();x.PositionX=r.X;x.PositionY=r.Y;x.PositionZ=r.Z;x.ScaleX=r.ScaleX;x.ScaleY=r.ScaleY;x.ScaleZ=r.ScaleZ;x.SortNo=r.SortNo;x.UpdateTime=DateTime.Now;return x;}
    private async Task ValidateBusiness(Lab3dScene scene,string type,Guid id){var raw=RawGuidConverter.ToRaw(id);var valid=type switch{"lab"=>scene.LabId.SequenceEqual(raw)&&await db.Queryable<Lab>().AnyAsync(x=>x.Id==raw),"location"=>await db.Queryable<LabLocation>().AnyAsync(x=>x.Id==raw&&x.LabId==scene.LabId),"instrument"=>await db.Queryable<LabInstrumentEntity>().AnyAsync(x=>x.Id==raw&&x.LabId==scene.LabId),_=>false};if(!valid)throw new InvalidOperationException("绑定对象不存在或不属于当前实验室。");}
    private static int ParseFloor(string value){var digits=new string(value.Where(char.IsDigit).ToArray());return int.TryParse(digits,out var floor)&&floor>0?floor:1;}
}
