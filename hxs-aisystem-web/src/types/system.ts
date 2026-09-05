export interface UserProfile {
  id: string;
  userName: string;
  displayName?: string | null;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  user: UserProfile;
}

export interface OrgDto {
  id: string;
  parentId?: string | null;
  orgName: string;
  orgCode: string;
  orgType: string;
  sortNo: number;
  isActive: boolean;
  children?: OrgDto[];
}

export interface UserDto {
  id: string;
  orgId?: string | null;
  userName: string;
  displayName?: string | null;
  phone?: string | null;
  email?: string | null;
  isActive: boolean;
  lastLoginTime?: string | null;
}

export interface RoleDto {
  id: string;
  roleCode: string;
  roleName: string;
  description?: string | null;
  isActive: boolean;
}

export interface MenuDto {
  id: string;
  parentId?: string | null;
  menuCode: string;
  menuName: string;
  menuType: string;
  routePath?: string | null;
  component?: string | null;
  icon?: string | null;
  permissionCode?: string | null;
  sortNo: number;
  isVisible: boolean;
  isActive: boolean;
  children?: MenuDto[];
}

export interface AssignRolesRequest {
  roleIds: string[];
}

export interface AssignMenusRequest {
  menuIds: string[];
}

export interface AuditLogDto {
  id: string;
  userId?: string | null;
  userName?: string | null;
  moduleCode: string;
  actionCode: string;
  businessId?: string | null;
  requestPath: string;
  httpMethod: string;
  result: string;
  ipAddress?: string | null;
  createTime: string;
}

export interface PageResult<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  total: number;
}
