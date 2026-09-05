using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabExperiment;

public sealed class LabExperimentService : ILabExperimentService
{
    private readonly ISqlSugarClient _db; private readonly ICurrentUserService _currentUser; private readonly IDataScopeService _dataScope;
    public LabExperimentService(ISqlSugarClient db,ICurrentUserService currentUser,IDataScopeService dataScope){_db=db;_currentUser=currentUser;_dataScope=dataScope;}

    public async Task<List<ExperimentDto>> GetAsync(bool mine=false,string? keyword=null,string? status=null,DateTime? startTime=null,DateTime? endTime=null)
    {
        var q=_db.Queryable<Domain.Entities.LabExperiment>();var scope=await _dataScope.GetCurrentScopeAsync();
        if(mine||scope==DataScope.Self){var user=CurrentRaw();q=q.Where(x=>x.OwnerId==user);}
        if(!string.IsNullOrWhiteSpace(keyword)){var value=keyword.Trim();q=q.Where(x=>x.ExperimentName.Contains(value)||x.ExperimentNo.Contains(value)||x.TopicName!.Contains(value));}
        if(!string.IsNullOrWhiteSpace(status))q=q.Where(x=>x.Status==status);
        if(startTime.HasValue)q=q.Where(x=>x.CreateTime>=startTime.Value);
        if(endTime.HasValue)q=q.Where(x=>x.CreateTime<endTime.Value.Date.AddDays(1));
        return await MapAsync(await q.OrderByDescending(x=>x.CreateTime).ToListAsync());
    }

    public async Task<ExperimentDto> GetByIdAsync(Guid id){var row=await FindAsync(id);await EnsureCanViewAsync(row);return (await MapAsync([row]))[0];}
    public async Task<ExperimentDto> CreateAsync(ExperimentRequest request)
    {
        Validate(request);var now=DateTime.Now;var row=new Domain.Entities.LabExperiment{Id=NewId(),ExperimentNo=No(),ExperimentName=request.ExperimentName.Trim(),GroupId=RawGuidConverter.ToNullableRaw(request.GroupId),OwnerId=CurrentRaw(),TopicName=Clean(request.TopicName),Purpose=request.Purpose.Trim(),Status="draft",CreateTime=now,UpdateTime=now};
        await ValidateGroupAsync(row.GroupId);await _db.Ado.BeginTranAsync();try{await _db.Insertable(row).ExecuteCommandAsync();await ReplaceRelationsAsync(row,request);await _db.Ado.CommitTranAsync();}catch{await _db.Ado.RollbackTranAsync();throw;}return (await MapAsync([row]))[0];
    }
    public async Task UpdateAsync(Guid id,ExperimentRequest request)
    {
        Validate(request);var row=await FindAsync(id);await EnsureCanEditAsync(row);if(row.Status!="draft")throw new InvalidOperationException("只有草稿实验可以编辑基本信息和关联数据。");row.ExperimentName=request.ExperimentName.Trim();row.GroupId=RawGuidConverter.ToNullableRaw(request.GroupId);row.TopicName=Clean(request.TopicName);row.Purpose=request.Purpose.Trim();row.UpdateTime=DateTime.Now;await ValidateGroupAsync(row.GroupId);await _db.Ado.BeginTranAsync();try{await _db.Updateable(row).ExecuteCommandAsync();await ReplaceRelationsAsync(row,request);await _db.Ado.CommitTranAsync();}catch{await _db.Ado.RollbackTranAsync();throw;}
    }
    public async Task StartAsync(Guid id)=>await ChangeStatusAsync(id,"draft","in_progress",x=>x.StartTime=DateTime.Now);
    public async Task CompleteAsync(Guid id)=>await ChangeStatusAsync(id,"in_progress","completed",x=>x.EndTime=DateTime.Now);
    public async Task ReopenAsync(Guid id,ExperimentReasonRequest request){Required(request.Reason,"退回原因");await ChangeStatusAsync(id,"completed","in_progress",x=>x.EndTime=null);await AddSystemRecordAsync(id,"reopen",request.Reason);}
    public async Task ArchiveAsync(Guid id){await ChangeStatusAsync(id,"completed","archived",x=>{x.ArchiveUserId=CurrentRaw();x.ArchiveTime=DateTime.Now;});}
    public async Task UnarchiveAsync(Guid id,ExperimentReasonRequest request){Required(request.Reason,"解档原因");var row=await FindAsync(id);if(row.Status!="archived")throw new InvalidOperationException("只有已归档实验可以解档。");row.Status="completed";row.ArchiveUserId=null;row.ArchiveTime=null;row.UpdateTime=DateTime.Now;await _db.Updateable(row).ExecuteCommandAsync();await AddSystemRecordAsync(id,"unarchive",request.Reason);}
    public async Task<ExperimentRecordDto> AddRecordAsync(Guid id,ExperimentRecordRequest request)
    {
        Required(request.Content,"记录内容");if(request.RecordType is not("process" or "result" or "raw_data"))throw new InvalidOperationException("记录类型无效。");var row=await FindAsync(id);await EnsureCanEditAsync(row);if(row.Status!="in_progress")throw new InvalidOperationException("只有进行中的实验可以增加过程记录。");var record=new LabExperimentRecord{Id=NewId(),ExperimentId=row.Id,RecordType=request.RecordType,Content=request.Content.Trim(),RecordTime=request.RecordTime??DateTime.Now,CreatorId=CurrentRaw()};await _db.Insertable(record).ExecuteCommandAsync();return new ExperimentRecordDto(RawGuidConverter.ToGuid(record.Id),record.RecordType,record.Content,record.RecordTime,CurrentId(),null);
    }

    private async Task ChangeStatusAsync(Guid id,string from,string to,Action<Domain.Entities.LabExperiment> change)
    {
        await _db.Ado.BeginTranAsync();try{await LockAsync(id);var row=await FindAsync(id);await EnsureCanEditAsync(row);if(row.Status!=from)throw new InvalidOperationException($"当前状态不能执行该操作，要求状态为 {from}。");row.Status=to;change(row);row.UpdateTime=DateTime.Now;await _db.Updateable(row).ExecuteCommandAsync();await _db.Ado.CommitTranAsync();}catch{await _db.Ado.RollbackTranAsync();throw;}
    }
    private async Task ReplaceRelationsAsync(Domain.Entities.LabExperiment experiment,ExperimentRequest request)
    {
        var instruments=request.Instruments.GroupBy(x=>x.InstrumentId).Select(x=>x.First()).ToList();var materials=request.Materials.GroupBy(x=>x.MaterialId).Select(x=>x.First()).ToList();
        foreach(var item in instruments)
        {
            var instrument=await FindEntityAsync<HxsAiSystem.Domain.Entities.LabInstrument>(item.InstrumentId,"仪器不存在。");if(instrument.IsActive!=1)throw new InvalidOperationException("关联仪器已停用。");
            if(item.BookingId.HasValue){var booking=await FindEntityAsync<LabBooking>(item.BookingId.Value,"预约不存在。");if(booking.Status!="approved"||!booking.InstrumentId.SequenceEqual(instrument.Id)||!booking.ApplicantId.SequenceEqual(experiment.OwnerId))throw new InvalidOperationException("只能关联负责人本人已通过且仪器一致的预约。");}
        }
        foreach(var item in materials)
        {
            if(item.Quantity<=0)throw new InvalidOperationException("关联物资数量必须大于0。");var material=await FindEntityAsync<LabMaterial>(item.MaterialId,"物资不存在。");if(material.IsActive!=1)throw new InvalidOperationException("关联物资已停用。");
            if(item.RequisitionId.HasValue){var req=await FindEntityAsync<LabRequisition>(item.RequisitionId.Value,"领用单不存在。");if(req.Status!="approved"||!req.ApplicantId.SequenceEqual(experiment.OwnerId))throw new InvalidOperationException("只能关联负责人本人已通过的领用单。");var detail=await _db.Queryable<LabRequisitionItem>().FirstAsync(x=>x.RequisitionId==req.Id&&x.MaterialId==material.Id);if(detail is null||detail.ApprovedQuantity.GetValueOrDefault()<item.Quantity)throw new InvalidOperationException("关联数量不能超过领用单批准数量。");}
        }
        await _db.Deleteable<LabExperimentInstrument>().Where(x=>x.ExperimentId==experiment.Id).ExecuteCommandAsync();await _db.Deleteable<LabExperimentMaterial>().Where(x=>x.ExperimentId==experiment.Id).ExecuteCommandAsync();
        if(instruments.Count>0)await _db.Insertable(instruments.Select(x=>new LabExperimentInstrument{Id=NewId(),ExperimentId=experiment.Id,InstrumentId=RawGuidConverter.ToRaw(x.InstrumentId),BookingId=RawGuidConverter.ToNullableRaw(x.BookingId)}).ToList()).ExecuteCommandAsync();
        if(materials.Count>0)await _db.Insertable(materials.Select(x=>new LabExperimentMaterial{Id=NewId(),ExperimentId=experiment.Id,MaterialId=RawGuidConverter.ToRaw(x.MaterialId),RequisitionId=RawGuidConverter.ToNullableRaw(x.RequisitionId),Quantity=x.Quantity}).ToList()).ExecuteCommandAsync();
    }
    private async Task EnsureCanViewAsync(Domain.Entities.LabExperiment row){if(row.OwnerId.SequenceEqual(CurrentRaw()))return;if(await _dataScope.GetCurrentScopeAsync()==DataScope.Self)throw new UnauthorizedAccessException("无权查看该实验。");}
    private async Task EnsureCanEditAsync(Domain.Entities.LabExperiment row){if(row.Status=="archived")throw new InvalidOperationException("已归档实验只读。");if(row.OwnerId.SequenceEqual(CurrentRaw()))return;if(await _dataScope.GetCurrentScopeAsync()!=DataScope.All)throw new UnauthorizedAccessException("只能编辑本人实验。");}
    private async Task ValidateGroupAsync(byte[]? groupId){if(groupId is not null&&!await _db.Queryable<LabGroup>().AnyAsync(x=>x.Id==groupId&&x.IsActive==1))throw new InvalidOperationException("课题组不存在或已停用。");}
    private async Task AddSystemRecordAsync(Guid id,string type,string content)=>await _db.Insertable(new LabExperimentRecord{Id=NewId(),ExperimentId=RawGuidConverter.ToRaw(id),RecordType=type,Content=content.Trim(),RecordTime=DateTime.Now,CreatorId=CurrentRaw()}).ExecuteCommandAsync();
    private async Task LockAsync(Guid id)=>await _db.Ado.GetDataTableAsync($"SELECT ID FROM HXS_LAB_EXPERIMENT WHERE ID=HEXTORAW('{Convert.ToHexString(id.ToByteArray())}') FOR UPDATE");
    private async Task<Domain.Entities.LabExperiment> FindAsync(Guid id)=>await FindEntityAsync<Domain.Entities.LabExperiment>(id,"实验任务不存在。");
    private async Task<T> FindEntityAsync<T>(Guid id,string message)where T:class,new()=>await _db.Queryable<T>().Where($"ID=HEXTORAW('{Convert.ToHexString(id.ToByteArray())}')").FirstAsync()??throw new KeyNotFoundException(message);

    private async Task<List<ExperimentDto>> MapAsync(List<Domain.Entities.LabExperiment> rows)
    {
        var users=(await _db.Queryable<AppUser>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var groups=(await _db.Queryable<LabGroup>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var instruments=(await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var bookings=(await _db.Queryable<LabBooking>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var materials=(await _db.Queryable<LabMaterial>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var requisitions=(await _db.Queryable<LabRequisition>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var dict=(await _db.Queryable<SysDictItem>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));
        var expIns=await _db.Queryable<LabExperimentInstrument>().ToListAsync();var expMats=await _db.Queryable<LabExperimentMaterial>().ToListAsync();var records=await _db.Queryable<LabExperimentRecord>().ToListAsync();var files=await _db.Queryable<SysFileRecord>().Where(x=>x.BusinessType=="experiment").ToListAsync();
        return rows.Select(x=>{var id=RawGuidConverter.ToGuid(x.Id);var owner=RawGuidConverter.ToGuid(x.OwnerId);Guid? group=x.GroupId is null?null:RawGuidConverter.ToGuid(x.GroupId);var ins=expIns.Where(y=>y.ExperimentId.SequenceEqual(x.Id)).Select(y=>{var iid=RawGuidConverter.ToGuid(y.InstrumentId);Guid? bid=y.BookingId is null?null:RawGuidConverter.ToGuid(y.BookingId);return new ExperimentInstrumentDto(RawGuidConverter.ToGuid(y.Id),iid,instruments.GetValueOrDefault(iid)?.InstrumentName,bid,bid.HasValue?bookings.GetValueOrDefault(bid.Value)?.BookingNo:null);}).ToList();var mats=expMats.Where(y=>y.ExperimentId.SequenceEqual(x.Id)).Select(y=>{var mid=RawGuidConverter.ToGuid(y.MaterialId);Guid? rid=y.RequisitionId is null?null:RawGuidConverter.ToGuid(y.RequisitionId);var material=materials.GetValueOrDefault(mid);return new ExperimentMaterialDto(RawGuidConverter.ToGuid(y.Id),mid,material?.MaterialName,rid,rid.HasValue?requisitions.GetValueOrDefault(rid.Value)?.RequisitionNo:null,y.Quantity,material is null?null:dict.GetValueOrDefault(RawGuidConverter.ToGuid(material.UnitId))?.ItemLabel);}).ToList();var recs=records.Where(y=>y.ExperimentId.SequenceEqual(x.Id)).OrderByDescending(y=>y.RecordTime).Select(y=>{var creator=RawGuidConverter.ToGuid(y.CreatorId);return new ExperimentRecordDto(RawGuidConverter.ToGuid(y.Id),y.RecordType,y.Content,y.RecordTime,creator,users.GetValueOrDefault(creator)?.DisplayName??users.GetValueOrDefault(creator)?.UserName);}).ToList();var fs=files.Where(y=>string.Equals(y.BusinessId,id.ToString(),StringComparison.OrdinalIgnoreCase)).Select(y=>new ExperimentFileDto(RawGuidConverter.ToGuid(y.Id),y.OriginalName,y.ContentType,y.FileSize,RawGuidConverter.ToGuid(y.UploaderId),y.CreateTime)).ToList();return new ExperimentDto(id,x.ExperimentNo,x.ExperimentName,group,group.HasValue?groups.GetValueOrDefault(group.Value)?.GroupName:null,owner,users.GetValueOrDefault(owner)?.DisplayName??users.GetValueOrDefault(owner)?.UserName,x.TopicName,x.Purpose,x.Status,x.StartTime,x.EndTime,x.ArchiveUserId is null?null:RawGuidConverter.ToGuid(x.ArchiveUserId),x.ArchiveTime,x.CreateTime,ins,mats,recs,fs);}).ToList();
    }
    private Guid CurrentId()=>_currentUser.GetUserId()??throw new UnauthorizedAccessException("用户未登录。");private byte[] CurrentRaw()=>RawGuidConverter.ToRaw(CurrentId());private static byte[] NewId()=>Guid.NewGuid().ToByteArray();private static string No()=>($"EX{DateTime.Now:yyyyMMddHHmmssfff}{Guid.NewGuid():N}")[..25];private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();private static void Required(string? value,string name){if(string.IsNullOrWhiteSpace(value))throw new InvalidOperationException($"{name}不能为空。");}private static void Validate(ExperimentRequest request){Required(request.ExperimentName,"实验名称");Required(request.Purpose,"实验目的");}
}
