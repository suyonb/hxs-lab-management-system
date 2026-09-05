export interface LabDto { id: string; labCode: string; labName: string; managerId?: string | null; managerName?: string | null; description?: string | null; isActive: boolean }
export interface LocationDto { id: string; labId: string; parentId?: string | null; locationCode: string; locationName: string; locationType: string; sortNo: number; isActive: boolean; children?: LocationDto[] }
export interface LabGroupDto { id: string; labId: string; groupCode: string; groupName: string; leaderId?: string | null; leaderName?: string | null; description?: string | null; isActive: boolean }
export interface GroupMemberDto { id: string; groupId: string; userId: string; userName?: string | null; displayName?: string | null; memberRole: string }
export interface SupplierDto { id: string; supplierCode: string; supplierName: string; contactName?: string | null; phone?: string | null; email?: string | null; address?: string | null; isActive: boolean }
export interface DictTypeDto { id: string; dictCode: string; dictName: string; description?: string | null; isActive: boolean }
export interface DictItemDto { id: string; dictTypeId: string; itemValue: string; itemLabel: string; sortNo: number; isActive: boolean }
