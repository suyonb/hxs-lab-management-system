namespace HxsAiSystem.Application.LabFoundation;

public interface ILabFoundationService
{
    Task<List<LabDto>> GetLabsAsync(bool enabledOnly = false); Task<LabDto> CreateLabAsync(LabRequest request); Task UpdateLabAsync(Guid id, LabRequest request); Task DeleteLabAsync(Guid id);
    Task<List<LocationDto>> GetLocationsAsync(Guid? labId = null, bool enabledOnly = false); Task<LocationDto> CreateLocationAsync(LocationRequest request); Task UpdateLocationAsync(Guid id, LocationRequest request); Task DeleteLocationAsync(Guid id);
    Task<List<LabGroupDto>> GetGroupsAsync(Guid? labId = null, bool enabledOnly = false); Task<LabGroupDto> CreateGroupAsync(LabGroupRequest request); Task UpdateGroupAsync(Guid id, LabGroupRequest request); Task DeleteGroupAsync(Guid id);
    Task<List<GroupMemberDto>> GetMembersAsync(Guid groupId); Task<GroupMemberDto> AddMemberAsync(Guid groupId, GroupMemberRequest request); Task DeleteMemberAsync(Guid groupId, Guid memberId);
    Task<List<SupplierDto>> GetSuppliersAsync(bool enabledOnly = false); Task<SupplierDto> CreateSupplierAsync(SupplierRequest request); Task UpdateSupplierAsync(Guid id, SupplierRequest request); Task DeleteSupplierAsync(Guid id);
    Task<List<DictTypeDto>> GetDictTypesAsync(bool enabledOnly = false); Task<DictTypeDto> CreateDictTypeAsync(DictTypeRequest request); Task UpdateDictTypeAsync(Guid id, DictTypeRequest request); Task DeleteDictTypeAsync(Guid id);
    Task<List<DictItemDto>> GetDictItemsAsync(Guid typeId, bool enabledOnly = false); Task<DictItemDto> CreateDictItemAsync(Guid typeId, DictItemRequest request); Task UpdateDictItemAsync(Guid id, DictItemRequest request); Task DeleteDictItemAsync(Guid id);
}
