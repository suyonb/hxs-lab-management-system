import { http } from './http';
import type { DictItemDto, DictTypeDto, GroupMemberDto, LabDto, LabGroupDto, LocationDto, SupplierDto } from '../types/lab';

const base = '/api/lab/foundation';
export const labFoundationApi = {
  labs: (enabledOnly = false, silentError = false) => http.get<LabDto[]>(`${base}/labs`, { params: { enabledOnly }, silentError }).then(r => r.data),
  createLab: (data: Partial<LabDto>) => http.post<LabDto>(`${base}/labs`, data).then(r => r.data),
  updateLab: (id: string, data: Partial<LabDto>) => http.put(`${base}/labs/${id}`, data), removeLab: (id: string) => http.delete(`${base}/labs/${id}`),
  locations: (labId?: string, enabledOnly = false, silentError = false) => http.get<LocationDto[]>(`${base}/locations`, { params: { labId, enabledOnly }, silentError }).then(r => r.data),
  createLocation: (data: Partial<LocationDto>) => http.post<LocationDto>(`${base}/locations`, data).then(r => r.data),
  updateLocation: (id: string, data: Partial<LocationDto>) => http.put(`${base}/locations/${id}`, data), removeLocation: (id: string) => http.delete(`${base}/locations/${id}`),
  groups: (labId?: string, enabledOnly = false, silentError = false) => http.get<LabGroupDto[]>(`${base}/groups`, { params: { labId, enabledOnly }, silentError }).then(r => r.data),
  createGroup: (data: Partial<LabGroupDto>) => http.post<LabGroupDto>(`${base}/groups`, data).then(r => r.data),
  updateGroup: (id: string, data: Partial<LabGroupDto>) => http.put(`${base}/groups/${id}`, data), removeGroup: (id: string) => http.delete(`${base}/groups/${id}`),
  members: (groupId: string) => http.get<GroupMemberDto[]>(`${base}/groups/${groupId}/members`).then(r => r.data),
  addMember: (groupId: string, data: { userId: string; memberRole: string }) => http.post(`${base}/groups/${groupId}/members`, data), removeMember: (groupId: string, id: string) => http.delete(`${base}/groups/${groupId}/members/${id}`),
  suppliers: (enabledOnly = false) => http.get<SupplierDto[]>(`${base}/suppliers`, { params: { enabledOnly } }).then(r => r.data),
  createSupplier: (data: Partial<SupplierDto>) => http.post<SupplierDto>(`${base}/suppliers`, data).then(r => r.data),
  updateSupplier: (id: string, data: Partial<SupplierDto>) => http.put(`${base}/suppliers/${id}`, data), removeSupplier: (id: string) => http.delete(`${base}/suppliers/${id}`),
  dictTypes: (enabledOnly = false) => http.get<DictTypeDto[]>(`${base}/dict-types`, { params: { enabledOnly } }).then(r => r.data),
  createDictType: (data: Partial<DictTypeDto>) => http.post<DictTypeDto>(`${base}/dict-types`, data).then(r => r.data),
  updateDictType: (id: string, data: Partial<DictTypeDto>) => http.put(`${base}/dict-types/${id}`, data), removeDictType: (id: string) => http.delete(`${base}/dict-types/${id}`),
  dictItems: (typeId: string, enabledOnly = false) => http.get<DictItemDto[]>(`${base}/dict-types/${typeId}/items`, { params: { enabledOnly } }).then(r => r.data),
  createDictItem: (typeId: string, data: Partial<DictItemDto>) => http.post(`${base}/dict-types/${typeId}/items`, data),
  updateDictItem: (id: string, data: Partial<DictItemDto>) => http.put(`${base}/dict-items/${id}`, data), removeDictItem: (id: string) => http.delete(`${base}/dict-items/${id}`)
};
