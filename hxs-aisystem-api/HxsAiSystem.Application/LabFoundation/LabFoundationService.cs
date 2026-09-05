using HxsAiSystem.Application.Common;
using HxsAiSystem.Domain.Entities;
using SqlSugar;

namespace HxsAiSystem.Application.LabFoundation;

public sealed class LabFoundationService : ILabFoundationService
{
    private readonly ISqlSugarClient _db;
    public LabFoundationService(ISqlSugarClient db) => _db = db;

    public async Task<List<LabDto>> GetLabsAsync(bool enabledOnly = false)
    {
        var query = _db.Queryable<Lab>(); if (enabledOnly) query = query.Where(x => x.IsActive == 1);
        var rows = await query.OrderBy(x => x.LabName).ToListAsync(); var users = await UserNamesAsync();
        return rows.Select(x => ToLab(x, users)).ToList();
    }
    public async Task<LabDto> CreateLabAsync(LabRequest r)
    {
        Required(r.LabCode, "实验室编码"); Required(r.LabName, "实验室名称");
        if (await _db.Queryable<Lab>().AnyAsync(x => x.LabCode == r.LabCode.Trim())) throw new InvalidOperationException("实验室编码已存在。");
        var x = new Lab { Id = NewId(), LabCode = r.LabCode.Trim(), LabName = r.LabName.Trim(), ManagerId = RawGuidConverter.ToNullableRaw(r.ManagerId), Description = Clean(r.Description), IsActive = Flag(r.IsActive), CreateTime = Now, UpdateTime = Now };
        await _db.Insertable(x).ExecuteCommandAsync(); return ToLab(x, await UserNamesAsync());
    }
    public async Task UpdateLabAsync(Guid id, LabRequest r)
    {
        Required(r.LabName, "实验室名称"); var x = await FindAsync<Lab>(id);
        x.LabName = r.LabName.Trim(); x.ManagerId = RawGuidConverter.ToNullableRaw(r.ManagerId); x.Description = Clean(r.Description); x.IsActive = Flag(r.IsActive); x.UpdateTime = Now;
        await _db.Updateable(x).UpdateColumns(y => new { y.LabName, y.ManagerId, y.Description, y.IsActive, y.UpdateTime }).ExecuteCommandAsync();
    }
    public async Task DeleteLabAsync(Guid id)
    {
        var raw = RawGuidConverter.ToRaw(id);
        if (await _db.Queryable<LabLocation>().AnyAsync(x => x.LabId == raw) || await _db.Queryable<LabGroup>().AnyAsync(x => x.LabId == raw)) throw new InvalidOperationException("实验室已有关联位置或课题组，请停用而不是删除。");
        await DeleteAsync<Lab>(id);
    }

    public async Task<List<LocationDto>> GetLocationsAsync(Guid? labId = null, bool enabledOnly = false)
    {
        var query = _db.Queryable<LabLocation>(); if (labId.HasValue) { var raw = RawGuidConverter.ToRaw(labId.Value); query = query.Where(x => x.LabId == raw); } if (enabledOnly) query = query.Where(x => x.IsActive == 1);
        return BuildLocationTree((await query.OrderBy(x => x.SortNo).ToListAsync()).Select(ToLocation).ToList());
    }
    public async Task<LocationDto> CreateLocationAsync(LocationRequest r)
    {
        Required(r.LocationCode, "位置编码"); Required(r.LocationName, "位置名称"); await EnsureLocationCodeAsync(r.LocationCode, null); await ValidateLocationParentAsync(null, r.LabId, r.ParentId);
        var x = new LabLocation { Id = NewId(), LabId = RawGuidConverter.ToRaw(r.LabId), ParentId = RawGuidConverter.ToNullableRaw(r.ParentId), LocationCode = r.LocationCode.Trim(), LocationName = r.LocationName.Trim(), LocationType = r.LocationType, SortNo = r.SortNo, IsActive = Flag(r.IsActive), CreateTime = Now, UpdateTime = Now };
        await _db.Insertable(x).ExecuteCommandAsync(); return ToLocation(x);
    }
    public async Task UpdateLocationAsync(Guid id, LocationRequest r)
    {
        Required(r.LocationName, "位置名称"); await ValidateLocationParentAsync(id, r.LabId, r.ParentId); var x = await FindAsync<LabLocation>(id);
        x.LabId = RawGuidConverter.ToRaw(r.LabId); x.ParentId = RawGuidConverter.ToNullableRaw(r.ParentId); x.LocationName = r.LocationName.Trim(); x.LocationType = r.LocationType; x.SortNo = r.SortNo; x.IsActive = Flag(r.IsActive); x.UpdateTime = Now;
        await _db.Updateable(x).UpdateColumns(y => new { y.LabId, y.ParentId, y.LocationName, y.LocationType, y.SortNo, y.IsActive, y.UpdateTime }).ExecuteCommandAsync();
    }
    public async Task DeleteLocationAsync(Guid id)
    {
        var raw = RawGuidConverter.ToRaw(id); if (await _db.Queryable<LabLocation>().AnyAsync(x => x.ParentId == raw)) throw new InvalidOperationException("位置下存在子节点，不能删除。"); await DeleteAsync<LabLocation>(id);
    }

    public async Task<List<LabGroupDto>> GetGroupsAsync(Guid? labId = null, bool enabledOnly = false)
    {
        var query = _db.Queryable<LabGroup>(); if (labId.HasValue) { var raw = RawGuidConverter.ToRaw(labId.Value); query = query.Where(x => x.LabId == raw); } if (enabledOnly) query = query.Where(x => x.IsActive == 1);
        var rows = await query.OrderBy(x => x.GroupName).ToListAsync(); var users = await UserNamesAsync(); return rows.Select(x => ToGroup(x, users)).ToList();
    }
    public async Task<LabGroupDto> CreateGroupAsync(LabGroupRequest r)
    {
        Required(r.GroupCode, "课题组编码"); Required(r.GroupName, "课题组名称"); if (await _db.Queryable<LabGroup>().AnyAsync(x => x.GroupCode == r.GroupCode.Trim())) throw new InvalidOperationException("课题组编码已存在。");
        var x = new LabGroup { Id = NewId(), LabId = RawGuidConverter.ToRaw(r.LabId), GroupCode = r.GroupCode.Trim(), GroupName = r.GroupName.Trim(), LeaderId = RawGuidConverter.ToNullableRaw(r.LeaderId), Description = Clean(r.Description), IsActive = Flag(r.IsActive), CreateTime = Now, UpdateTime = Now };
        await _db.Insertable(x).ExecuteCommandAsync(); return ToGroup(x, await UserNamesAsync());
    }
    public async Task UpdateGroupAsync(Guid id, LabGroupRequest r)
    {
        Required(r.GroupName, "课题组名称"); var x = await FindAsync<LabGroup>(id); x.LabId = RawGuidConverter.ToRaw(r.LabId); x.GroupName = r.GroupName.Trim(); x.LeaderId = RawGuidConverter.ToNullableRaw(r.LeaderId); x.Description = Clean(r.Description); x.IsActive = Flag(r.IsActive); x.UpdateTime = Now;
        await _db.Updateable(x).UpdateColumns(y => new { y.LabId, y.GroupName, y.LeaderId, y.Description, y.IsActive, y.UpdateTime }).ExecuteCommandAsync();
    }
    public async Task DeleteGroupAsync(Guid id) { var raw = RawGuidConverter.ToRaw(id); if (await _db.Queryable<LabGroupMember>().AnyAsync(x => x.GroupId == raw)) throw new InvalidOperationException("课题组存在成员，请先移除成员或停用课题组。"); await DeleteAsync<LabGroup>(id); }
    public async Task<List<GroupMemberDto>> GetMembersAsync(Guid groupId)
    {
        var raw = RawGuidConverter.ToRaw(groupId); var rows = await _db.Queryable<LabGroupMember>().Where(x => x.GroupId == raw).ToListAsync(); var users = await UserNamesAsync(); return rows.Select(x => ToMember(x, users)).ToList();
    }
    public async Task<GroupMemberDto> AddMemberAsync(Guid groupId, GroupMemberRequest r)
    {
        var group = RawGuidConverter.ToRaw(groupId); var user = RawGuidConverter.ToRaw(r.UserId); if (await _db.Queryable<LabGroupMember>().AnyAsync(x => x.GroupId == group && x.UserId == user)) throw new InvalidOperationException("该用户已在课题组中。");
        var x = new LabGroupMember { Id = NewId(), GroupId = group, UserId = user, MemberRole = r.MemberRole, CreateTime = Now }; await _db.Insertable(x).ExecuteCommandAsync(); return ToMember(x, await UserNamesAsync());
    }
    public Task DeleteMemberAsync(Guid groupId, Guid memberId) => DeleteAsync<LabGroupMember>(memberId);

    public async Task<List<SupplierDto>> GetSuppliersAsync(bool enabledOnly = false) { var q = _db.Queryable<LabSupplier>(); if (enabledOnly) q = q.Where(x => x.IsActive == 1); return (await q.OrderBy(x => x.SupplierName).ToListAsync()).Select(ToSupplier).ToList(); }
    public async Task<SupplierDto> CreateSupplierAsync(SupplierRequest r) { Required(r.SupplierCode, "供应商编码"); Required(r.SupplierName, "供应商名称"); if (await _db.Queryable<LabSupplier>().AnyAsync(x => x.SupplierCode == r.SupplierCode.Trim())) throw new InvalidOperationException("供应商编码已存在。"); var x = MapSupplier(new LabSupplier { Id = NewId(), SupplierCode = r.SupplierCode.Trim(), CreateTime = Now }, r); await _db.Insertable(x).ExecuteCommandAsync(); return ToSupplier(x); }
    public async Task UpdateSupplierAsync(Guid id, SupplierRequest r) { Required(r.SupplierName, "供应商名称"); var x = MapSupplier(await FindAsync<LabSupplier>(id), r); await _db.Updateable(x).ExecuteCommandAsync(); }
    public Task DeleteSupplierAsync(Guid id) => DeleteAsync<LabSupplier>(id);

    public async Task<List<DictTypeDto>> GetDictTypesAsync(bool enabledOnly = false) { var q = _db.Queryable<SysDictType>(); if (enabledOnly) q = q.Where(x => x.IsActive == 1); return (await q.OrderBy(x => x.DictName).ToListAsync()).Select(ToDictType).ToList(); }
    public async Task<DictTypeDto> CreateDictTypeAsync(DictTypeRequest r) { Required(r.DictCode, "字典编码"); Required(r.DictName, "字典名称"); if (await _db.Queryable<SysDictType>().AnyAsync(x => x.DictCode == r.DictCode.Trim())) throw new InvalidOperationException("字典编码已存在。"); var x = new SysDictType { Id = NewId(), DictCode = r.DictCode.Trim(), DictName = r.DictName.Trim(), Description = Clean(r.Description), IsActive = Flag(r.IsActive), CreateTime = Now, UpdateTime = Now }; await _db.Insertable(x).ExecuteCommandAsync(); return ToDictType(x); }
    public async Task UpdateDictTypeAsync(Guid id, DictTypeRequest r) { Required(r.DictName, "字典名称"); var x = await FindAsync<SysDictType>(id); x.DictName = r.DictName.Trim(); x.Description = Clean(r.Description); x.IsActive = Flag(r.IsActive); x.UpdateTime = Now; await _db.Updateable(x).UpdateColumns(y => new { y.DictName, y.Description, y.IsActive, y.UpdateTime }).ExecuteCommandAsync(); }
    public async Task DeleteDictTypeAsync(Guid id) { var raw = RawGuidConverter.ToRaw(id); if (await _db.Queryable<SysDictItem>().AnyAsync(x => x.DictTypeId == raw)) throw new InvalidOperationException("字典类型下存在字典项，不能删除。"); await DeleteAsync<SysDictType>(id); }
    public async Task<List<DictItemDto>> GetDictItemsAsync(Guid typeId, bool enabledOnly = false) { var raw = RawGuidConverter.ToRaw(typeId); var q = _db.Queryable<SysDictItem>().Where(x => x.DictTypeId == raw); if (enabledOnly) q = q.Where(x => x.IsActive == 1); return (await q.OrderBy(x => x.SortNo).ToListAsync()).Select(ToDictItem).ToList(); }
    public async Task<DictItemDto> CreateDictItemAsync(Guid typeId, DictItemRequest r) { Required(r.ItemValue, "字典值"); Required(r.ItemLabel, "字典标签"); var type = RawGuidConverter.ToRaw(typeId); if (await _db.Queryable<SysDictItem>().AnyAsync(x => x.DictTypeId == type && x.ItemValue == r.ItemValue.Trim())) throw new InvalidOperationException("该字典值已存在。"); var x = new SysDictItem { Id = NewId(), DictTypeId = type, ItemValue = r.ItemValue.Trim(), ItemLabel = r.ItemLabel.Trim(), SortNo = r.SortNo, IsActive = Flag(r.IsActive), CreateTime = Now, UpdateTime = Now }; await _db.Insertable(x).ExecuteCommandAsync(); return ToDictItem(x); }
    public async Task UpdateDictItemAsync(Guid id, DictItemRequest r) { Required(r.ItemLabel, "字典标签"); var x = await FindAsync<SysDictItem>(id); x.ItemLabel = r.ItemLabel.Trim(); x.SortNo = r.SortNo; x.IsActive = Flag(r.IsActive); x.UpdateTime = Now; await _db.Updateable(x).UpdateColumns(y => new { y.ItemLabel, y.SortNo, y.IsActive, y.UpdateTime }).ExecuteCommandAsync(); }
    public Task DeleteDictItemAsync(Guid id) => DeleteAsync<SysDictItem>(id);

    private async Task ValidateLocationParentAsync(Guid? id, Guid labId, Guid? parentId)
    {
        if (!parentId.HasValue) return; if (id == parentId) throw new InvalidOperationException("位置不能选择自身作为上级。");
        var parent = await FindAsync<LabLocation>(parentId.Value); if (RawGuidConverter.ToGuid(parent.LabId) != labId) throw new InvalidOperationException("上级位置必须属于同一实验室。");
        var cursor = parent; while (cursor.ParentId is not null) { var ancestorId = RawGuidConverter.ToGuid(cursor.ParentId); if (ancestorId == id) throw new InvalidOperationException("位置树不能形成循环引用。"); cursor = await FindAsync<LabLocation>(ancestorId); }
    }
    private async Task EnsureLocationCodeAsync(string code, Guid? _) { if (await _db.Queryable<LabLocation>().AnyAsync(x => x.LocationCode == code.Trim())) throw new InvalidOperationException("位置编码已存在。"); }
    private async Task<Dictionary<Guid, (string UserName, string? DisplayName)>> UserNamesAsync() => (await _db.Queryable<AppUser>().ToListAsync()).ToDictionary(x => RawGuidConverter.ToGuid(x.Id), x => (x.UserName, x.DisplayName));
    private async Task<T> FindAsync<T>(Guid id) where T : class, new() => await _db.Queryable<T>().Where($"ID = HEXTORAW('{RawHex(id)}')").FirstAsync() ?? throw new KeyNotFoundException("数据不存在。");
    private async Task DeleteAsync<T>(Guid id) where T : class, new()
    {
        var table = typeof(T) == typeof(Lab) ? "HXS_LAB" : typeof(T) == typeof(LabLocation) ? "HXS_LAB_LOCATION" : typeof(T) == typeof(LabGroup) ? "HXS_LAB_GROUP" : typeof(T) == typeof(LabGroupMember) ? "HXS_LAB_GROUP_MEMBER" : typeof(T) == typeof(LabSupplier) ? "HXS_LAB_SUPPLIER" : typeof(T) == typeof(SysDictType) ? "HXS_SYS_DICT_TYPE" : typeof(T) == typeof(SysDictItem) ? "HXS_SYS_DICT_ITEM" : throw new InvalidOperationException("不支持的数据类型。");
        if (await _db.Ado.ExecuteCommandAsync($"DELETE FROM {table} WHERE ID = HEXTORAW('{RawHex(id)}')") == 0) throw new KeyNotFoundException("数据不存在。");
    }
    private static string RawHex(Guid id) => Convert.ToHexString(id.ToByteArray());
    private static List<LocationDto> BuildLocationTree(List<LocationDto> rows) { var map = rows.ToDictionary(x => x.Id, x => x with { Children = [] }); foreach (var x in map.Values.OrderBy(x => x.SortNo)) if (x.ParentId.HasValue && map.TryGetValue(x.ParentId.Value, out var parent)) parent.Children!.Add(x); return map.Values.Where(x => !x.ParentId.HasValue || !map.ContainsKey(x.ParentId.Value)).OrderBy(x => x.SortNo).ToList(); }
    private static LabSupplier MapSupplier(LabSupplier x, SupplierRequest r) { x.SupplierName = r.SupplierName.Trim(); x.ContactName = Clean(r.ContactName); x.Phone = Clean(r.Phone); x.Email = Clean(r.Email); x.Address = Clean(r.Address); x.IsActive = Flag(r.IsActive); x.UpdateTime = Now; return x; }
    private static LabDto ToLab(Lab x, Dictionary<Guid, (string UserName, string? DisplayName)> users) { var manager = x.ManagerId is null ? (Guid?)null : RawGuidConverter.ToGuid(x.ManagerId); return new(RawGuidConverter.ToGuid(x.Id), x.LabCode, x.LabName, manager, manager.HasValue && users.TryGetValue(manager.Value, out var u) ? u.DisplayName ?? u.UserName : null, x.Description, x.IsActive == 1); }
    private static LocationDto ToLocation(LabLocation x) => new(RawGuidConverter.ToGuid(x.Id), RawGuidConverter.ToGuid(x.LabId), x.ParentId is null ? null : RawGuidConverter.ToGuid(x.ParentId), x.LocationCode, x.LocationName, x.LocationType, x.SortNo, x.IsActive == 1, []);
    private static LabGroupDto ToGroup(LabGroup x, Dictionary<Guid, (string UserName, string? DisplayName)> users) { var leader = x.LeaderId is null ? (Guid?)null : RawGuidConverter.ToGuid(x.LeaderId); return new(RawGuidConverter.ToGuid(x.Id), RawGuidConverter.ToGuid(x.LabId), x.GroupCode, x.GroupName, leader, leader.HasValue && users.TryGetValue(leader.Value, out var u) ? u.DisplayName ?? u.UserName : null, x.Description, x.IsActive == 1); }
    private static GroupMemberDto ToMember(LabGroupMember x, Dictionary<Guid, (string UserName, string? DisplayName)> users) { var uid = RawGuidConverter.ToGuid(x.UserId); users.TryGetValue(uid, out var u); return new(RawGuidConverter.ToGuid(x.Id), RawGuidConverter.ToGuid(x.GroupId), uid, u.UserName, u.DisplayName, x.MemberRole); }
    private static SupplierDto ToSupplier(LabSupplier x) => new(RawGuidConverter.ToGuid(x.Id), x.SupplierCode, x.SupplierName, x.ContactName, x.Phone, x.Email, x.Address, x.IsActive == 1);
    private static DictTypeDto ToDictType(SysDictType x) => new(RawGuidConverter.ToGuid(x.Id), x.DictCode, x.DictName, x.Description, x.IsActive == 1);
    private static DictItemDto ToDictItem(SysDictItem x) => new(RawGuidConverter.ToGuid(x.Id), RawGuidConverter.ToGuid(x.DictTypeId), x.ItemValue, x.ItemLabel, x.SortNo, x.IsActive == 1);
    private static byte[] NewId() => Guid.NewGuid().ToByteArray(); private static DateTime Now => DateTime.Now; private static int Flag(bool value) => value ? 1 : 0; private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Required(string? value, string name) { if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"{name}不能为空。"); }
}
