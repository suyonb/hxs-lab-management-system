using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.LabFoundation;
using Microsoft.AspNetCore.Mvc;

namespace HxsAiSystem.WebApiHost.Controllers;

[ApiController]
[Route("api/lab/foundation")]
[PermissionAuthorize("lab:base:view")]
public sealed class LabFoundationController : ControllerBase
{
    private readonly ILabFoundationService _service;
    public LabFoundationController(ILabFoundationService service) => _service = service;

    /// <summary>查询实验室列表，可仅返回启用项供下拉选择。</summary>
    [HttpGet("labs")] public Task<List<LabDto>> GetLabs([FromQuery] bool enabledOnly = false) => _service.GetLabsAsync(enabledOnly);
    /// <summary>创建实验室基础档案。</summary>
    [HttpPost("labs"), PermissionAuthorize("lab:base:manage")] public Task<LabDto> CreateLab(LabRequest request) => _service.CreateLabAsync(request);
    /// <summary>修改实验室资料或启停状态，编码保持不变。</summary>
    [HttpPut("labs/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> UpdateLab(Guid id, LabRequest request) { await _service.UpdateLabAsync(id, request); return NoContent(); }
    /// <summary>删除没有位置及课题组引用的实验室。</summary>
    [HttpDelete("labs/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteLab(Guid id) { await _service.DeleteLabAsync(id); return NoContent(); }

    /// <summary>查询实验室楼栋、房间、区域和柜体位置树。</summary>
    [HttpGet("locations")] public Task<List<LocationDto>> GetLocations([FromQuery] Guid? labId = null, [FromQuery] bool enabledOnly = false) => _service.GetLocationsAsync(labId, enabledOnly);
    /// <summary>创建实验室位置节点。</summary>
    [HttpPost("locations"), PermissionAuthorize("lab:base:manage")] public Task<LocationDto> CreateLocation(LocationRequest request) => _service.CreateLocationAsync(request);
    /// <summary>修改位置节点并校验位置树循环引用。</summary>
    [HttpPut("locations/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> UpdateLocation(Guid id, LocationRequest request) { await _service.UpdateLocationAsync(id, request); return NoContent(); }
    /// <summary>删除没有子节点引用的位置。</summary>
    [HttpDelete("locations/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteLocation(Guid id) { await _service.DeleteLocationAsync(id); return NoContent(); }

    /// <summary>查询实验室课题组列表。</summary>
    [HttpGet("groups")] public Task<List<LabGroupDto>> GetGroups([FromQuery] Guid? labId = null, [FromQuery] bool enabledOnly = false) => _service.GetGroupsAsync(labId, enabledOnly);
    /// <summary>创建实验室课题组。</summary>
    [HttpPost("groups"), PermissionAuthorize("lab:base:manage")] public Task<LabGroupDto> CreateGroup(LabGroupRequest request) => _service.CreateGroupAsync(request);
    /// <summary>修改课题组资料、负责人或启停状态。</summary>
    [HttpPut("groups/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> UpdateGroup(Guid id, LabGroupRequest request) { await _service.UpdateGroupAsync(id, request); return NoContent(); }
    /// <summary>删除没有成员引用的课题组。</summary>
    [HttpDelete("groups/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteGroup(Guid id) { await _service.DeleteGroupAsync(id); return NoContent(); }
    /// <summary>查询指定课题组成员。</summary>
    [HttpGet("groups/{groupId:guid}/members")] public Task<List<GroupMemberDto>> GetMembers(Guid groupId) => _service.GetMembersAsync(groupId);
    /// <summary>向指定课题组添加系统用户。</summary>
    [HttpPost("groups/{groupId:guid}/members"), PermissionAuthorize("lab:base:manage")] public Task<GroupMemberDto> AddMember(Guid groupId, GroupMemberRequest request) => _service.AddMemberAsync(groupId, request);
    /// <summary>从指定课题组移除成员。</summary>
    [HttpDelete("groups/{groupId:guid}/members/{memberId:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteMember(Guid groupId, Guid memberId) { await _service.DeleteMemberAsync(groupId, memberId); return NoContent(); }

    /// <summary>查询供应商列表，可仅返回启用项供下拉选择。</summary>
    [HttpGet("suppliers")] public Task<List<SupplierDto>> GetSuppliers([FromQuery] bool enabledOnly = false) => _service.GetSuppliersAsync(enabledOnly);
    /// <summary>创建试剂或耗材供应商。</summary>
    [HttpPost("suppliers"), PermissionAuthorize("lab:base:manage")] public Task<SupplierDto> CreateSupplier(SupplierRequest request) => _service.CreateSupplierAsync(request);
    /// <summary>修改供应商资料或启停状态。</summary>
    [HttpPut("suppliers/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> UpdateSupplier(Guid id, SupplierRequest request) { await _service.UpdateSupplierAsync(id, request); return NoContent(); }
    /// <summary>删除未被业务引用的供应商。</summary>
    [HttpDelete("suppliers/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteSupplier(Guid id) { await _service.DeleteSupplierAsync(id); return NoContent(); }

    /// <summary>查询字典类型，可仅返回启用项。</summary>
    [HttpGet("dict-types")] public Task<List<DictTypeDto>> GetDictTypes([FromQuery] bool enabledOnly = false) => _service.GetDictTypesAsync(enabledOnly);
    /// <summary>创建业务字典类型。</summary>
    [HttpPost("dict-types"), PermissionAuthorize("lab:base:manage")] public Task<DictTypeDto> CreateDictType(DictTypeRequest request) => _service.CreateDictTypeAsync(request);
    /// <summary>修改字典类型名称、说明或启停状态。</summary>
    [HttpPut("dict-types/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> UpdateDictType(Guid id, DictTypeRequest request) { await _service.UpdateDictTypeAsync(id, request); return NoContent(); }
    /// <summary>删除没有字典项的字典类型。</summary>
    [HttpDelete("dict-types/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteDictType(Guid id) { await _service.DeleteDictTypeAsync(id); return NoContent(); }
    /// <summary>查询指定字典类型的字典项，可仅返回启用项供下拉选择。</summary>
    [HttpGet("dict-types/{typeId:guid}/items")] public Task<List<DictItemDto>> GetDictItems(Guid typeId, [FromQuery] bool enabledOnly = false) => _service.GetDictItemsAsync(typeId, enabledOnly);
    /// <summary>向指定字典类型添加字典项。</summary>
    [HttpPost("dict-types/{typeId:guid}/items"), PermissionAuthorize("lab:base:manage")] public Task<DictItemDto> CreateDictItem(Guid typeId, DictItemRequest request) => _service.CreateDictItemAsync(typeId, request);
    /// <summary>修改字典项标签、排序或启停状态。</summary>
    [HttpPut("dict-items/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> UpdateDictItem(Guid id, DictItemRequest request) { await _service.UpdateDictItemAsync(id, request); return NoContent(); }
    /// <summary>删除未被业务引用的字典项。</summary>
    [HttpDelete("dict-items/{id:guid}"), PermissionAuthorize("lab:base:manage")] public async Task<IActionResult> DeleteDictItem(Guid id) { await _service.DeleteDictItemAsync(id); return NoContent(); }
}
