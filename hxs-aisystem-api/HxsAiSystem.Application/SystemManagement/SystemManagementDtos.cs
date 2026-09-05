namespace HxsAiSystem.Application.SystemManagement;

public sealed record OrgDto(Guid Id, Guid? ParentId, string OrgName, string OrgCode, string OrgType, int SortNo, bool IsActive);
public sealed record UserDto(Guid Id, Guid? OrgId, string UserName, string? DisplayName, string? Phone, string? Email, bool IsActive, DateTime? LastLoginTime);
public sealed record RoleDto(Guid Id, string RoleCode, string RoleName, string? Description, bool IsActive);
public sealed record MenuDto(Guid Id, Guid? ParentId, string MenuCode, string MenuName, string MenuType, string? RoutePath, string? Component, string? Icon, string? PermissionCode, int SortNo, bool IsVisible, bool IsActive);

public sealed class MenuTreeDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string MenuCode { get; init; } = string.Empty;
    public string MenuName { get; init; } = string.Empty;
    public string MenuType { get; init; } = string.Empty;
    public string? RoutePath { get; init; }
    public string? Component { get; init; }
    public string? Icon { get; init; }
    public string? PermissionCode { get; init; }
    public int SortNo { get; init; }
    public bool IsVisible { get; init; }
    public bool IsActive { get; init; }
    public List<MenuTreeDto> Children { get; init; } = [];
}

public sealed class OrgTreeDto
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string OrgName { get; init; } = string.Empty;
    public string OrgCode { get; init; } = string.Empty;
    public string OrgType { get; init; } = string.Empty;
    public int SortNo { get; init; }
    public bool IsActive { get; init; }
    public List<OrgTreeDto> Children { get; init; } = [];
}

public class CreateOrgRequest
{
    public Guid? ParentId { get; set; }
    public string OrgName { get; set; } = string.Empty;
    public string OrgCode { get; set; } = string.Empty;
    public string OrgType { get; set; } = "department";
    public int SortNo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateOrgRequest : CreateOrgRequest
{
}

public sealed class CreateUserRequest
{
    public Guid? OrgId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserRequest
{
    public Guid? OrgId { get; set; }
    public string? DisplayName { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateRoleRequest
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRoleRequest : CreateRoleRequest
{
}

public class CreateMenuRequest
{
    public Guid? ParentId { get; set; }
    public string MenuCode { get; set; } = string.Empty;
    public string MenuName { get; set; } = string.Empty;
    public string MenuType { get; set; } = "page";
    public string? RoutePath { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public string? PermissionCode { get; set; }
    public int SortNo { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateMenuRequest : CreateMenuRequest
{
}

public sealed class AssignRolesRequest
{
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class AssignMenusRequest
{
    public List<Guid> MenuIds { get; set; } = [];
}
