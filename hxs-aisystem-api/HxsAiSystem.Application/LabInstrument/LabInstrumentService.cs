using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabInstrument;

public sealed class LabInstrumentService : ILabInstrumentService
{
    private readonly ISqlSugarClient _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDataScopeService _dataScope;
    public LabInstrumentService(ISqlSugarClient db, ICurrentUserService currentUser, IDataScopeService dataScope) { _db = db; _currentUser = currentUser; _dataScope = dataScope; }

    public async Task<List<InstrumentDto>> GetInstrumentsAsync(bool availableOnly = false)
    {
        var query = _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>();
        if (availableOnly) query = query.Where(x => x.IsActive == 1 && x.Status == "normal");
        return await MapInstrumentsAsync(await query.OrderBy(x => x.InstrumentCode).ToListAsync());
    }

    public async Task<InstrumentDto> CreateInstrumentAsync(InstrumentRequest r)
    {
        Required(r.InstrumentCode, "仪器编号"); Required(r.InstrumentName, "仪器名称"); ValidateInstrumentStatus(r.Status);
        if (await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().AnyAsync(x => x.InstrumentCode == r.InstrumentCode.Trim())) throw new InvalidOperationException("仪器编号已存在。");
        await ValidateReferencesAsync(r);
        var now = DateTime.Now;
        var row = new HxsAiSystem.Domain.Entities.LabInstrument { Id = NewId(), InstrumentCode = r.InstrumentCode.Trim(), InstrumentName = r.InstrumentName.Trim(), CategoryId = RawGuidConverter.ToNullableRaw(r.CategoryId), Model = Clean(r.Model), Manufacturer = Clean(r.Manufacturer), SupplierId = RawGuidConverter.ToNullableRaw(r.SupplierId), LabId = RawGuidConverter.ToRaw(r.LabId), LocationId = RawGuidConverter.ToRaw(r.LocationId), Status = r.Status, Description = Clean(r.Description), IsActive = r.IsActive ? 1 : 0, CreateTime = now, UpdateTime = now };
        await _db.Insertable(row).ExecuteCommandAsync(); return (await MapInstrumentsAsync([row]))[0];
    }

    public async Task UpdateInstrumentAsync(Guid id, InstrumentRequest r)
    {
        Required(r.InstrumentName, "仪器名称"); ValidateInstrumentStatus(r.Status); await ValidateReferencesAsync(r);
        var row = await FindAsync<HxsAiSystem.Domain.Entities.LabInstrument>(id);
        row.InstrumentName = r.InstrumentName.Trim(); row.CategoryId = RawGuidConverter.ToNullableRaw(r.CategoryId); row.Model = Clean(r.Model); row.Manufacturer = Clean(r.Manufacturer); row.SupplierId = RawGuidConverter.ToNullableRaw(r.SupplierId); row.LabId = RawGuidConverter.ToRaw(r.LabId); row.LocationId = RawGuidConverter.ToRaw(r.LocationId); row.Status = r.Status; row.Description = Clean(r.Description); row.IsActive = r.IsActive ? 1 : 0; row.UpdateTime = DateTime.Now;
        await _db.Updateable(row).ExecuteCommandAsync();
    }

    public async Task<List<BookingDto>> GetBookingsAsync(bool mine = false, string? status = null)
    {
        var query = _db.Queryable<LabBooking>();
        if (mine || await IsSelfScopeAsync()) { var user = CurrentRaw(); query = query.Where(x => x.ApplicantId == user); }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        return await MapBookingsAsync(await query.OrderByDescending(x => x.CreateTime).ToListAsync());
    }

    public async Task<BookingDto> CreateBookingAsync(BookingRequest r)
    {
        var startTime = NormalizeMinute(r.StartTime); var endTime = NormalizeMinute(r.EndTime);
        ValidatePeriod(startTime, endTime); Required(r.Purpose, "预约用途");
        await _db.Ado.BeginTranAsync();
        try
        {
            var instrument = await FindAsync<HxsAiSystem.Domain.Entities.LabInstrument>(r.InstrumentId);
            if (instrument.IsActive != 1 || instrument.Status != "normal") throw new InvalidOperationException("当前仪器不可预约。");
            await LockInstrumentAsync(instrument.Id);
            await EnsureNoConflictAsync(instrument.Id, startTime, endTime, null);
            var now = DateTime.Now;
            var row = new LabBooking { Id = NewId(), BookingNo = No("BK"), InstrumentId = instrument.Id, ApplicantId = CurrentRaw(), GroupId = RawGuidConverter.ToNullableRaw(r.GroupId), StartTime = startTime, EndTime = endTime, Purpose = r.Purpose.Trim(), Status = "pending", CreateTime = now, UpdateTime = now };
            await _db.Insertable(row).ExecuteCommandAsync(); await _db.Ado.CommitTranAsync(); return (await MapBookingsAsync([row]))[0];
        }
        catch { await _db.Ado.RollbackTranAsync(); throw; }
    }

    public async Task CancelBookingAsync(Guid id)
    {
        var row = await FindAsync<LabBooking>(id); await EnsureOwnerOrManagerAsync(row.ApplicantId);
        if (row.Status is not ("pending" or "approved")) throw new InvalidOperationException("当前预约状态不允许取消。");
        row.Status = "cancelled"; row.CancelTime = DateTime.Now; row.UpdateTime = DateTime.Now; await _db.Updateable(row).ExecuteCommandAsync();
    }

    public Task ApproveBookingAsync(Guid id, ApprovalRequest r) => DecideBookingAsync(id, "approved", r.Remark);
    public Task RejectBookingAsync(Guid id, ApprovalRequest r) => DecideBookingAsync(id, "rejected", r.Remark);
    private async Task DecideBookingAsync(Guid id, string target, string? remark)
    {
        await _db.Ado.BeginTranAsync();
        try
        {
            var row = await FindAsync<LabBooking>(id); if (row.Status != "pending") throw new InvalidOperationException("预约已处理，不能重复审批。");
            if (target == "approved") { await LockInstrumentAsync(row.InstrumentId); await EnsureNoConflictAsync(row.InstrumentId, row.StartTime, row.EndTime, row.Id); }
            row.Status = target; row.ApproverId = CurrentRaw(); row.ApproveTime = DateTime.Now; row.ApproveRemark = Clean(remark); row.UpdateTime = DateTime.Now;
            await _db.Updateable(row).ExecuteCommandAsync(); await _db.Ado.CommitTranAsync();
        }
        catch { await _db.Ado.RollbackTranAsync(); throw; }
    }

    public async Task CompleteBookingAsync(Guid id)
    {
        var row = await FindAsync<LabBooking>(id); if (row.Status != "approved") throw new InvalidOperationException("只有已通过预约可以完成。");
        row.Status = "completed"; row.UpdateTime = DateTime.Now; await _db.Updateable(row).ExecuteCommandAsync();
    }

    public async Task<List<UsageDto>> GetUsagesAsync(bool mine = false)
    {
        var query = _db.Queryable<LabUsage>(); if (mine || await IsSelfScopeAsync()) { var user = CurrentRaw(); query = query.Where(x => x.UserId == user); }
        return await MapUsagesAsync(await query.OrderByDescending(x => x.StartTime).ToListAsync());
    }

    public async Task<UsageDto> CreateUsageAsync(UsageRequest r)
    {
        var startTime = NormalizeMinute(r.StartTime); var endTime = NormalizeMinute(r.EndTime);
        ValidatePeriod(startTime, endTime); Required(r.ExperimentContent, "实验内容"); var instrument = await FindAsync<HxsAiSystem.Domain.Entities.LabInstrument>(r.InstrumentId); if (instrument.Status == "stopped") throw new InvalidOperationException("停用仪器不能登记使用。");
        LabBooking? booking = null;
        if (r.BookingId.HasValue) { booking = await FindAsync<LabBooking>(r.BookingId.Value); if (!booking.InstrumentId.SequenceEqual(instrument.Id) || booking.Status != "approved") throw new InvalidOperationException("关联预约无效。"); await EnsureOwnerOrManagerAsync(booking.ApplicantId); }
        var now = DateTime.Now; var row = new LabUsage { Id = NewId(), InstrumentId = instrument.Id, BookingId = RawGuidConverter.ToNullableRaw(r.BookingId), UserId = CurrentRaw(), StartTime = startTime, EndTime = endTime, ExperimentContent = r.ExperimentContent.Trim(), Remark = Clean(r.Remark), CreateTime = now, UpdateTime = now };
        await _db.Insertable(row).ExecuteCommandAsync(); if (booking is not null) { booking.Status = "completed"; booking.UpdateTime = now; await _db.Updateable(booking).ExecuteCommandAsync(); } return (await MapUsagesAsync([row]))[0];
    }

    public async Task<List<RepairDto>> GetRepairsAsync(bool mine = false, string? status = null)
    {
        var query = _db.Queryable<LabRepair>(); if (mine || await IsSelfScopeAsync()) { var user = CurrentRaw(); query = query.Where(x => x.ReporterId == user); } if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        return await MapRepairsAsync(await query.OrderByDescending(x => x.CreateTime).ToListAsync());
    }

    public async Task<RepairDto> CreateRepairAsync(RepairRequest r)
    {
        Required(r.FaultDescription, "故障描述"); var instrument = await FindAsync<HxsAiSystem.Domain.Entities.LabInstrument>(r.InstrumentId); var now = DateTime.Now;
        var row = new LabRepair { Id = NewId(), RepairNo = No("RP"), InstrumentId = instrument.Id, ReporterId = CurrentRaw(), FaultDescription = r.FaultDescription.Trim(), Status = "pending", CreateTime = now, UpdateTime = now };
        await _db.Insertable(row).ExecuteCommandAsync(); return (await MapRepairsAsync([row]))[0];
    }
    public Task ApproveRepairAsync(Guid id, ApprovalRequest r) => DecideRepairAsync(id, "approved", r.Remark);
    public Task RejectRepairAsync(Guid id, ApprovalRequest r) => DecideRepairAsync(id, "rejected", r.Remark);
    private async Task DecideRepairAsync(Guid id, string target, string? remark)
    {
        await _db.Ado.BeginTranAsync(); try { var row = await FindAsync<LabRepair>(id); if (row.Status != "pending") throw new InvalidOperationException("报修已处理，不能重复审批。"); row.Status = target; row.ApproverId = CurrentRaw(); row.ApproveTime = DateTime.Now; row.Remark = Clean(remark); row.UpdateTime = DateTime.Now; await _db.Updateable(row).ExecuteCommandAsync(); if (target == "approved") await SetInstrumentStatusAsync(row.InstrumentId, "repair"); await _db.Ado.CommitTranAsync(); } catch { await _db.Ado.RollbackTranAsync(); throw; }
    }
    public async Task StartRepairAsync(Guid id, RepairWorkRequest r) { Required(r.Repairer, "维修人员"); var row = await FindAsync<LabRepair>(id); if (row.Status != "approved") throw new InvalidOperationException("只有已通过报修可以开始维修。"); row.Status = "repairing"; row.Repairer = r.Repairer.Trim(); row.RepairContent = Clean(r.RepairContent); row.RepairStartTime = DateTime.Now; row.Remark = Clean(r.Remark); row.UpdateTime = DateTime.Now; await _db.Updateable(row).ExecuteCommandAsync(); }
    public async Task CompleteRepairAsync(Guid id, RepairWorkRequest r)
    {
        await _db.Ado.BeginTranAsync(); try { var row = await FindAsync<LabRepair>(id); if (row.Status != "repairing") throw new InvalidOperationException("只有维修中的记录可以完成。"); Required(r.RepairContent, "维修内容"); row.Status = "completed"; row.Repairer = string.IsNullOrWhiteSpace(r.Repairer) ? row.Repairer : r.Repairer.Trim(); row.RepairContent = r.RepairContent.Trim(); row.RepairEndTime = DateTime.Now; row.Remark = Clean(r.Remark); row.UpdateTime = DateTime.Now; await _db.Updateable(row).ExecuteCommandAsync(); await SetInstrumentStatusAsync(row.InstrumentId, "normal"); await _db.Ado.CommitTranAsync(); } catch { await _db.Ado.RollbackTranAsync(); throw; }
    }

    private async Task EnsureNoConflictAsync(byte[] instrumentId, DateTime start, DateTime end, byte[]? excludedId)
    {
        var instrumentHex = Convert.ToHexString(instrumentId);
        var excludeSql = excludedId is null ? string.Empty : $" AND ID <> HEXTORAW('{Convert.ToHexString(excludedId)}')";
        var sql = $"SELECT COUNT(*) FROM HXS_LAB_BOOKING WHERE INSTRUMENT_ID = HEXTORAW('{instrumentHex}') AND START_TIME < :endTime AND END_TIME > :startTime AND STATUS IN ('pending', 'approved'){excludeSql}";
        var count = await _db.Ado.GetIntAsync(sql, new SugarParameter(":endTime", end), new SugarParameter(":startTime", start));
        if (count > 0) throw new InvalidOperationException("该仪器所选时间段已被预约。");
    }
    private async Task LockInstrumentAsync(byte[] instrumentId)
    {
        var rawHex = Convert.ToHexString(instrumentId);
        await _db.Ado.GetDataTableAsync($"SELECT ID FROM HXS_LAB_INSTRUMENT WHERE ID = HEXTORAW('{rawHex}') FOR UPDATE");
    }
    private async Task SetInstrumentStatusAsync(byte[] id, string status) { var row = await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().Where($"ID = HEXTORAW('{Convert.ToHexString(id)}')").FirstAsync() ?? throw new KeyNotFoundException("仪器不存在。"); row.Status = status; row.UpdateTime = DateTime.Now; await _db.Updateable(row).ExecuteCommandAsync(); }
    private async Task ValidateReferencesAsync(InstrumentRequest r) { var lab = RawGuidConverter.ToRaw(r.LabId); var loc = RawGuidConverter.ToRaw(r.LocationId); if (!await _db.Queryable<Lab>().AnyAsync(x => x.Id == lab && x.IsActive == 1)) throw new InvalidOperationException("实验室不存在或已停用。"); if (!await _db.Queryable<LabLocation>().AnyAsync(x => x.Id == loc && x.LabId == lab && x.IsActive == 1)) throw new InvalidOperationException("位置不存在、已停用或不属于所选实验室。"); }
    private async Task<bool> IsSelfScopeAsync() => await _dataScope.GetCurrentScopeAsync() == DataScope.Self;
    private async Task EnsureOwnerOrManagerAsync(byte[] owner) { if (owner.SequenceEqual(CurrentRaw())) return; if (await IsSelfScopeAsync()) throw new UnauthorizedAccessException("只能操作本人数据。"); }
    private byte[] CurrentRaw() => RawGuidConverter.ToRaw(_currentUser.GetUserId() ?? throw new UnauthorizedAccessException("用户未登录。"));
    private async Task<T> FindAsync<T>(Guid id) where T : class, new() => await _db.Queryable<T>().Where($"ID = HEXTORAW('{Convert.ToHexString(id.ToByteArray())}')").FirstAsync() ?? throw new KeyNotFoundException("记录不存在。");
    private static byte[] NewId() => Guid.NewGuid().ToByteArray(); private static string No(string p) => $"{p}{DateTime.Now:yyyyMMddHHmmssfff}"; private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim(); private static DateTime NormalizeMinute(DateTime value) => value.AddTicks(-(value.Ticks % TimeSpan.TicksPerMinute)); private static void Required(string? s, string name) { if (string.IsNullOrWhiteSpace(s)) throw new InvalidOperationException($"{name}不能为空。"); } private static void ValidatePeriod(DateTime s, DateTime e) { if (s >= e) throw new InvalidOperationException("开始时间必须早于结束时间。"); if (s.Minute % 30 != 0 || e.Minute % 30 != 0) throw new InvalidOperationException("预约和使用时间必须按30分钟对齐。"); } private static void ValidateInstrumentStatus(string s) { if (s is not ("normal" or "repair" or "stopped")) throw new InvalidOperationException("仪器状态无效。"); }

    private async Task<List<InstrumentDto>> MapInstrumentsAsync(List<HxsAiSystem.Domain.Entities.LabInstrument> rows) { var labs = (await _db.Queryable<Lab>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id)); var locs = (await _db.Queryable<LabLocation>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id)); var suppliers = (await _db.Queryable<LabSupplier>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id)); var items = (await _db.Queryable<SysDictItem>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id)); return rows.Select(x => { var id = RawGuidConverter.ToGuid(x.Id); var lab = RawGuidConverter.ToGuid(x.LabId); var loc = RawGuidConverter.ToGuid(x.LocationId); Guid? cat = x.CategoryId is null ? null : RawGuidConverter.ToGuid(x.CategoryId); Guid? sup = x.SupplierId is null ? null : RawGuidConverter.ToGuid(x.SupplierId); return new InstrumentDto(id, x.InstrumentCode, x.InstrumentName, cat, cat.HasValue ? items.GetValueOrDefault(cat.Value)?.ItemLabel : null, x.Model, x.Manufacturer, sup, sup.HasValue ? suppliers.GetValueOrDefault(sup.Value)?.SupplierName : null, lab, labs.TryGetValue(lab, out var l) ? l.LabName : null, loc, locs.TryGetValue(loc, out var o) ? o.LocationName : null, x.Status, x.Description, x.IsActive == 1); }).ToList(); }
    private async Task<List<BookingDto>> MapBookingsAsync(List<LabBooking> rows) { var ins = (await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id), x => x.InstrumentName); var users = await UserNamesAsync(); var groups = (await _db.Queryable<LabGroup>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id), x => x.GroupName); return rows.Select(x => { var i = RawGuidConverter.ToGuid(x.InstrumentId); var u = RawGuidConverter.ToGuid(x.ApplicantId); Guid? g = x.GroupId is null ? null : RawGuidConverter.ToGuid(x.GroupId); return new BookingDto(RawGuidConverter.ToGuid(x.Id), x.BookingNo, i, ins.GetValueOrDefault(i), u, users.GetValueOrDefault(u), g, g.HasValue ? groups.GetValueOrDefault(g.Value) : null, x.StartTime, x.EndTime, x.Purpose, x.Status, x.ApproverId is null ? (Guid?)null : RawGuidConverter.ToGuid(x.ApproverId), x.ApproveTime, x.ApproveRemark, x.CancelTime, x.CreateTime); }).ToList(); }
    private async Task<List<UsageDto>> MapUsagesAsync(List<LabUsage> rows) { var ins = (await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id), x => x.InstrumentName); var users = await UserNamesAsync(); return rows.Select(x => { var i = RawGuidConverter.ToGuid(x.InstrumentId); var u = RawGuidConverter.ToGuid(x.UserId); return new UsageDto(RawGuidConverter.ToGuid(x.Id), i, ins.GetValueOrDefault(i), x.BookingId is null ? (Guid?)null : RawGuidConverter.ToGuid(x.BookingId), u, users.GetValueOrDefault(u), x.StartTime, x.EndTime, x.ExperimentContent, x.Remark, x.CreateTime); }).ToList(); }
    private async Task<List<RepairDto>> MapRepairsAsync(List<LabRepair> rows) { var ins = (await _db.Queryable<HxsAiSystem.Domain.Entities.LabInstrument>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id), x => x.InstrumentName); var users = await UserNamesAsync(); return rows.Select(x => { var i = RawGuidConverter.ToGuid(x.InstrumentId); var u = RawGuidConverter.ToGuid(x.ReporterId); return new RepairDto(RawGuidConverter.ToGuid(x.Id), x.RepairNo, i, ins.GetValueOrDefault(i), u, users.GetValueOrDefault(u), x.FaultDescription, x.Status, x.ApproverId is null ? (Guid?)null : RawGuidConverter.ToGuid(x.ApproverId), x.ApproveTime, x.Repairer, x.RepairContent, x.RepairStartTime, x.RepairEndTime, x.Remark, x.CreateTime); }).ToList(); }
    private async Task<Dictionary<Guid, string>> UserNamesAsync() => (await _db.Queryable<AppUser>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id), x => x.DisplayName ?? x.UserName);
}
