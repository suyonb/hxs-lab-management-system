namespace HxsAiSystem.Application.LabFoundation;

public sealed record LabDto(Guid Id, string LabCode, string LabName, Guid? ManagerId, string? ManagerName, string? Description, bool IsActive);
public sealed record LocationDto(Guid Id, Guid LabId, Guid? ParentId, string LocationCode, string LocationName, string LocationType, int SortNo, bool IsActive, List<LocationDto>? Children = null);
public sealed record LabGroupDto(Guid Id, Guid LabId, string GroupCode, string GroupName, Guid? LeaderId, string? LeaderName, string? Description, bool IsActive);
public sealed record GroupMemberDto(Guid Id, Guid GroupId, Guid UserId, string? UserName, string? DisplayName, string MemberRole);
public sealed record SupplierDto(Guid Id, string SupplierCode, string SupplierName, string? ContactName, string? Phone, string? Email, string? Address, bool IsActive);
public sealed record DictTypeDto(Guid Id, string DictCode, string DictName, string? Description, bool IsActive);
public sealed record DictItemDto(Guid Id, Guid DictTypeId, string ItemValue, string ItemLabel, int SortNo, bool IsActive);

public class LabRequest { public string LabCode { get; set; } = ""; public string LabName { get; set; } = ""; public Guid? ManagerId { get; set; } public string? Description { get; set; } public bool IsActive { get; set; } = true; }
public class LocationRequest { public Guid LabId { get; set; } public Guid? ParentId { get; set; } public string LocationCode { get; set; } = ""; public string LocationName { get; set; } = ""; public string LocationType { get; set; } = "room"; public int SortNo { get; set; } public bool IsActive { get; set; } = true; }
public class LabGroupRequest { public Guid LabId { get; set; } public string GroupCode { get; set; } = ""; public string GroupName { get; set; } = ""; public Guid? LeaderId { get; set; } public string? Description { get; set; } public bool IsActive { get; set; } = true; }
public class GroupMemberRequest { public Guid UserId { get; set; } public string MemberRole { get; set; } = "member"; }
public class SupplierRequest { public string SupplierCode { get; set; } = ""; public string SupplierName { get; set; } = ""; public string? ContactName { get; set; } public string? Phone { get; set; } public string? Email { get; set; } public string? Address { get; set; } public bool IsActive { get; set; } = true; }
public class DictTypeRequest { public string DictCode { get; set; } = ""; public string DictName { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } = true; }
public class DictItemRequest { public string ItemValue { get; set; } = ""; public string ItemLabel { get; set; } = ""; public int SortNo { get; set; } public bool IsActive { get; set; } = true; }
