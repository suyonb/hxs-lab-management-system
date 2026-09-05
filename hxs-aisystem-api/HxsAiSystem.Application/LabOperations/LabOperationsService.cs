using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Application.LabInventory;
using HxsAiSystem.Domain.Entities;
using SqlSugar;
using System.Security;
using System.Text;
using LabInstrumentEntity = HxsAiSystem.Domain.Entities.LabInstrument;
using LabExperimentEntity = HxsAiSystem.Domain.Entities.LabExperiment;

namespace HxsAiSystem.Application.LabOperations;

public sealed class LabOperationsService : ILabOperationsService
{
    private readonly ISqlSugarClient _db;private readonly ICurrentUserService _currentUser;private readonly IDataScopeService _dataScope;private readonly ILabInventoryService _inventory;
    public LabOperationsService(ISqlSugarClient db,ICurrentUserService currentUser,IDataScopeService dataScope,ILabInventoryService inventory){_db=db;_currentUser=currentUser;_dataScope=dataScope;_inventory=inventory;}

    public async Task<List<UnifiedApprovalDto>> GetApprovalsAsync(string view,string? businessType=null,string? status=null,string? keyword=null,DateTime? startTime=null,DateTime? endTime=null)
    {
        if(view is not("mine" or "pending" or "processed" or "history"))throw new InvalidOperationException("审批视图无效。");var user=CurrentRaw();var scope=await _dataScope.GetCurrentScopeAsync();var users=(await _db.Queryable<AppUser>().ToListAsync()).ToDictionary(x=>RawGuidConverter.ToGuid(x.Id));var result=new List<UnifiedApprovalDto>();
        if(string.IsNullOrWhiteSpace(businessType)||businessType=="booking")
        {
            var rows=await _db.Queryable<LabBooking>().ToListAsync();foreach(var x in rows.Where(x=>Match(view,x.ApplicantId,x.ApproverId,x.Status,user,scope))){var a=RawGuidConverter.ToGuid(x.ApplicantId);Guid? p=x.ApproverId is null?null:RawGuidConverter.ToGuid(x.ApproverId);result.Add(new("booking",RawGuidConverter.ToGuid(x.Id),x.BookingNo,a,Name(users,a),x.CreateTime,x.Purpose,x.Status,p,p.HasValue?Name(users,p.Value):null,x.ApproveTime,"/lab/booking-approvals"));}
        }
        if(string.IsNullOrWhiteSpace(businessType)||businessType=="requisition")
        {
            var rows=await _db.Queryable<LabRequisition>().ToListAsync();foreach(var x in rows.Where(x=>Match(view,x.ApplicantId,x.ApproverId,x.Status,user,scope))){var a=RawGuidConverter.ToGuid(x.ApplicantId);Guid? p=x.ApproverId is null?null:RawGuidConverter.ToGuid(x.ApproverId);result.Add(new("requisition",RawGuidConverter.ToGuid(x.Id),x.RequisitionNo,a,Name(users,a),x.CreateTime,x.Purpose,x.Status,p,p.HasValue?Name(users,p.Value):null,x.ApproveTime,"/lab/requisition-approvals"));}
        }
        if(string.IsNullOrWhiteSpace(businessType)||businessType=="repair")
        {
            var rows=await _db.Queryable<LabRepair>().ToListAsync();foreach(var x in rows.Where(x=>Match(view,x.ReporterId,x.ApproverId,x.Status,user,scope))){var a=RawGuidConverter.ToGuid(x.ReporterId);Guid? p=x.ApproverId is null?null:RawGuidConverter.ToGuid(x.ApproverId);result.Add(new("repair",RawGuidConverter.ToGuid(x.Id),x.RepairNo,a,Name(users,a),x.CreateTime,x.FaultDescription,x.Status,p,p.HasValue?Name(users,p.Value):null,x.ApproveTime,"/lab/repairs"));}
        }
        if(!string.IsNullOrWhiteSpace(status))result=result.Where(x=>x.Status==status).ToList();if(!string.IsNullOrWhiteSpace(keyword)){var k=keyword.Trim();result=result.Where(x=>x.BusinessNo.Contains(k,StringComparison.OrdinalIgnoreCase)||(x.ApplicantName?.Contains(k,StringComparison.OrdinalIgnoreCase)??false)||x.Summary.Contains(k,StringComparison.OrdinalIgnoreCase)).ToList();}if(startTime.HasValue)result=result.Where(x=>x.ApplyTime>=startTime.Value).ToList();if(endTime.HasValue)result=result.Where(x=>x.ApplyTime<endTime.Value.Date.AddDays(1)).ToList();return result.OrderByDescending(x=>x.ApplyTime).ToList();
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync(int days=7)
    {
        days=Math.Clamp(days,7,30);var scope=await _dataScope.GetCurrentScopeAsync();var user=CurrentRaw();var now=DateTime.Now;var today=now.Date;var trendStart=today.AddDays(-(days-1));var bookings=await _db.Queryable<LabBooking>().ToListAsync();var repairs=await _db.Queryable<LabRepair>().ToListAsync();var instruments=await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().ToListAsync();var usages=await _db.Queryable<LabUsage>().Where(x=>x.StartTime>=trendStart).ToListAsync();var flows=await _db.Queryable<LabStockFlow>().Where(x=>x.CreateTime>=trendStart&&x.FlowType=="out").ToListAsync();var experiments=await _db.Queryable<HxsAiSystem.Domain.Entities.LabExperiment>().ToListAsync();var warnings=await _inventory.GetWarningsAsync();
        var pending=scope==DataScope.Self?bookings.Count(x=>x.ApplicantId.SequenceEqual(user)&&x.Status=="pending")+(await _db.Queryable<LabRequisition>().ToListAsync()).Count(x=>x.ApplicantId.SequenceEqual(user)&&x.Status=="pending")+repairs.Count(x=>x.ReporterId.SequenceEqual(user)&&x.Status=="pending"):bookings.Count(x=>x.Status=="pending")+(await _db.Queryable<LabRequisition>().ToListAsync()).Count(x=>x.Status=="pending")+repairs.Count(x=>x.Status=="pending");
        var usageTrend=Enumerable.Range(0,days).Select(i=>today.AddDays(i-days+1)).Select(d=>new TrendPointDto(d.ToString("MM-dd"),usages.Count(x=>x.StartTime.Date==d))).ToList();var materialTrend=Enumerable.Range(0,days).Select(i=>today.AddDays(i-days+1)).Select(d=>new TrendPointDto(d.ToString("MM-dd"),Math.Abs(flows.Where(x=>x.CreateTime.Date==d).Sum(x=>x.Quantity)))).ToList();
        return new DashboardSummaryDto(pending,bookings.Count(x=>x.StartTime.Date==today),instruments.Count(x=>x.IsActive==1&&x.Status=="repair"),warnings.Count(x=>x.WarningStatus=="low"),warnings.Count(x=>x.WarningStatus=="expiring"),warnings.Count(x=>x.WarningStatus=="expired"),experiments.Count(x=>x.CreateTime>=today.AddDays(-7)),experiments.Count(x=>x.Status=="archived"),usageTrend,materialTrend);
    }
    public async Task<(byte[] Content,string FileName)> ExportAsync(string type,string? keyword=null,string? status=null,DateTime? startTime=null,DateTime? endTime=null)
    {
        var scope=await _dataScope.GetCurrentScopeAsync();var user=CurrentRaw();var rows=new List<string[]>();string[] headers;
        bool MatchText(params string?[] values)=>string.IsNullOrWhiteSpace(keyword)||values.Any(x=>x?.Contains(keyword.Trim(),StringComparison.OrdinalIgnoreCase)==true);
        bool MatchDate(DateTime date)=>(!startTime.HasValue||date>=startTime.Value)&&(!endTime.HasValue||date<endTime.Value.Date.AddDays(1));
        switch(type)
        {
            case "instruments":{headers=["仪器编号","仪器名称","型号","制造商","运行状态","启用状态","创建时间"];var list=await _db.Queryable<LabInstrumentEntity>().ToListAsync();rows=list.Where(x=>(string.IsNullOrWhiteSpace(status)||x.Status==status)&&MatchText(x.InstrumentCode,x.InstrumentName,x.Model,x.Manufacturer)&&MatchDate(x.CreateTime)).Select(x=>new[]{x.InstrumentCode,x.InstrumentName,x.Model??"",x.Manufacturer??"",x.Status,x.IsActive==1?"启用":"停用",x.CreateTime.ToString("yyyy-MM-dd HH:mm")}).ToList();break;}
            case "materials":{headers=["物资编号","物资名称","类型","规格","CAS号","最低库存","创建时间"];var list=await _db.Queryable<LabMaterial>().ToListAsync();rows=list.Where(x=>MatchText(x.MaterialCode,x.MaterialName,x.Specification,x.CasNo)&&MatchDate(x.CreateTime)).Select(x=>new[]{x.MaterialCode,x.MaterialName,x.MaterialType,x.Specification??"",x.CasNo??"",x.MinStock.ToString(),x.CreateTime.ToString("yyyy-MM-dd HH:mm")}).ToList();break;}
            case "stock-flows":{headers=["流水号","类型","数量","变更前","变更后","来源","发生时间"];var list=await _db.Queryable<LabStockFlow>().ToListAsync();rows=list.Where(x=>(string.IsNullOrWhiteSpace(status)||x.FlowType==status)&&MatchText(x.FlowNo,x.SourceType,x.Remark)&&MatchDate(x.CreateTime)).Select(x=>new[]{x.FlowNo,x.FlowType,x.Quantity.ToString(),x.BeforeQuantity.ToString(),x.AfterQuantity.ToString(),x.SourceType,x.CreateTime.ToString("yyyy-MM-dd HH:mm")}).ToList();break;}
            case "bookings":{headers=["预约单号","开始时间","结束时间","用途","状态","申请时间"];var list=await _db.Queryable<LabBooking>().ToListAsync();rows=list.Where(x=>(scope!=DataScope.Self||x.ApplicantId.SequenceEqual(user))&&(string.IsNullOrWhiteSpace(status)||x.Status==status)&&MatchText(x.BookingNo,x.Purpose)&&MatchDate(x.CreateTime)).Select(x=>new[]{x.BookingNo,x.StartTime.ToString("yyyy-MM-dd HH:mm"),x.EndTime.ToString("yyyy-MM-dd HH:mm"),x.Purpose,x.Status,x.CreateTime.ToString("yyyy-MM-dd HH:mm")}).ToList();break;}
            case "requisitions":{headers=["领用单号","用途","状态","审批备注","申请时间"];var list=await _db.Queryable<LabRequisition>().ToListAsync();rows=list.Where(x=>(scope!=DataScope.Self||x.ApplicantId.SequenceEqual(user))&&(string.IsNullOrWhiteSpace(status)||x.Status==status)&&MatchText(x.RequisitionNo,x.Purpose,x.ApproveRemark)&&MatchDate(x.CreateTime)).Select(x=>new[]{x.RequisitionNo,x.Purpose,x.Status,x.ApproveRemark??"",x.CreateTime.ToString("yyyy-MM-dd HH:mm")}).ToList();break;}
            case "experiments":{headers=["实验编号","实验名称","课题","目的","状态","开始时间","结束时间","创建时间"];var list=await _db.Queryable<LabExperimentEntity>().ToListAsync();rows=list.Where(x=>(scope!=DataScope.Self||x.OwnerId.SequenceEqual(user))&&(string.IsNullOrWhiteSpace(status)||x.Status==status)&&MatchText(x.ExperimentNo,x.ExperimentName,x.TopicName,x.Purpose)&&MatchDate(x.CreateTime)).Select(x=>new[]{x.ExperimentNo,x.ExperimentName,x.TopicName??"",x.Purpose,x.Status,x.StartTime?.ToString("yyyy-MM-dd HH:mm")??"",x.EndTime?.ToString("yyyy-MM-dd HH:mm")??"",x.CreateTime.ToString("yyyy-MM-dd HH:mm")}).ToList();break;}
            default:throw new InvalidOperationException("不支持的导出类型。");
        }
        rows=rows.Take(5000).ToList();var xml=new StringBuilder("<?xml version=\"1.0\"?><Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"><Worksheet ss:Name=\"数据\"><Table>");void Row(IEnumerable<string> cells){xml.Append("<Row>");foreach(var cell in cells)xml.Append("<Cell><Data ss:Type=\"String\">").Append(SecurityElement.Escape(cell)).Append("</Data></Cell>");xml.Append("</Row>");}Row(headers);foreach(var row in rows)Row(row);xml.Append("</Table></Worksheet></Workbook>");return(Encoding.UTF8.GetBytes(xml.ToString()),$"lab-{type}-{DateTime.Now:yyyyMMddHHmmss}.xls");
    }
    private static bool Match(string view,byte[] applicant,byte[]? approver,string status,byte[] user,DataScope scope)=>view switch{"mine"=>applicant.SequenceEqual(user),"pending"=>scope!=DataScope.Self&&status=="pending","processed"=>approver is not null&&approver.SequenceEqual(user)&&status!="pending","history"=>(status=="cancelled"||status=="rejected")&&(scope!=DataScope.Self||applicant.SequenceEqual(user)),_=>false};private static string? Name(Dictionary<Guid,AppUser> users,Guid id)=>users.GetValueOrDefault(id)?.DisplayName??users.GetValueOrDefault(id)?.UserName;private byte[] CurrentRaw()=>RawGuidConverter.ToRaw(_currentUser.GetUserId()??throw new UnauthorizedAccessException("用户未登录。"));
}
