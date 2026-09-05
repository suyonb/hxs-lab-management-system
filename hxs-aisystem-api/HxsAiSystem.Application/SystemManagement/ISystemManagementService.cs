namespace HxsAiSystem.Application.SystemManagement;

public interface ISystemManagementService
{
    Task<List<OrgDto>> GetOrgsAsync();
    Task<List<OrgTreeDto>> GetOrgTreeAsync();
    Task<OrgDto> CreateOrgAsync(CreateOrgRequest request);
    Task<bool> UpdateOrgAsync(Guid id, UpdateOrgRequest request);
    Task<bool> DeleteOrgAsync(Guid id);

    Task<List<UserDto>> GetUsersAsync(string? keyword = null);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<List<RoleDto>> GetUserRolesAsync(Guid userId);
    Task AssignUserRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds);

    Task<List<RoleDto>> GetRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
    Task<bool> UpdateRoleAsync(Guid id, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<List<MenuDto>> GetRoleMenusAsync(Guid roleId);
    Task AssignRoleMenusAsync(Guid roleId, IReadOnlyCollection<Guid> menuIds);

    Task<List<MenuDto>> GetMenusAsync();
    Task<List<MenuTreeDto>> GetMenuTreeAsync();
    Task<List<MenuTreeDto>> GetUserMenuTreeAsync(Guid userId);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<MenuDto> CreateMenuAsync(CreateMenuRequest request);
    Task<bool> UpdateMenuAsync(Guid id, UpdateMenuRequest request);
    Task<bool> DeleteMenuAsync(Guid id);
}
