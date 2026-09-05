import { http } from './http';
import type { AssignMenusRequest, AssignRolesRequest, AuditLogDto, LoginResponse, MenuDto, OrgDto, PageResult, RoleDto, UserDto } from '../types/system';

export function login(data: { userName: string; password: string }) {
  return http.post<LoginResponse>('/api/auth/login', data).then((res) => res.data);
}

export function getMyMenus() {
  return http.get<MenuDto[]>('/api/auth/menus').then((res) => res.data);
}

export function getMyPermissions() {
  return http.get<string[]>('/api/auth/permissions').then((res) => res.data);
}

export function getHealth(silentError = false) {
  return http.get('/health', { silentError }).then((res) => res.data);
}

export const orgApi = {
  list: () => http.get<OrgDto[]>('/api/system/orgs').then((res) => res.data),
  tree: () => http.get<OrgDto[]>('/api/system/orgs/tree').then((res) => res.data),
  create: (data: Partial<OrgDto>) => http.post<OrgDto>('/api/system/orgs', data).then((res) => res.data),
  update: (id: string, data: Partial<OrgDto>) => http.put(`/api/system/orgs/${id}`, data),
  remove: (id: string) => http.delete(`/api/system/orgs/${id}`)
};

export const userApi = {
  list: (keyword?: string) => http.get<UserDto[]>('/api/system/users', { params: { keyword } }).then((res) => res.data),
  create: (data: Partial<UserDto> & { password: string }) => http.post<UserDto>('/api/system/users', data).then((res) => res.data),
  update: (id: string, data: Partial<UserDto> & { password?: string }) => http.put(`/api/system/users/${id}`, data),
  remove: (id: string) => http.delete(`/api/system/users/${id}`),
  roles: (id: string) => http.get<RoleDto[]>(`/api/system/users/${id}/roles`).then((res) => res.data),
  assignRoles: (id: string, data: AssignRolesRequest) => http.put(`/api/system/users/${id}/roles`, data)
};

export const roleApi = {
  list: () => http.get<RoleDto[]>('/api/system/roles').then((res) => res.data),
  create: (data: Partial<RoleDto>) => http.post<RoleDto>('/api/system/roles', data).then((res) => res.data),
  update: (id: string, data: Partial<RoleDto>) => http.put(`/api/system/roles/${id}`, data),
  remove: (id: string) => http.delete(`/api/system/roles/${id}`),
  menus: (id: string) => http.get<MenuDto[]>(`/api/system/roles/${id}/menus`).then((res) => res.data),
  assignMenus: (id: string, data: AssignMenusRequest) => http.put(`/api/system/roles/${id}/menus`, data)
};

export const menuApi = {
  list: () => http.get<MenuDto[]>('/api/system/menus').then((res) => res.data),
  tree: () => http.get<MenuDto[]>('/api/system/menus/tree').then((res) => res.data),
  create: (data: Partial<MenuDto>) => http.post<MenuDto>('/api/system/menus', data).then((res) => res.data),
  update: (id: string, data: Partial<MenuDto>) => http.put(`/api/system/menus/${id}`, data),
  remove: (id: string) => http.delete(`/api/system/menus/${id}`)
};

export const auditApi = {
  page: (params: { pageIndex: number; pageSize: number; keyword?: string; startTime?: string; endTime?: string }) =>
    http.get<PageResult<AuditLogDto>>('/api/system/audit-logs', { params }).then((res) => res.data)
};
