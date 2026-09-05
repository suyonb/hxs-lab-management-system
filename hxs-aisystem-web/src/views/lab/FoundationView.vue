<template>
  <div class="page-surface management-page lab-foundation-page">
    <PageToolbar eyebrow="Laboratory" :title="activeLabel">
      <a-button v-if="canManage" type="primary" class="action-create" @click="openCreate"><PlusOutlined />新增{{ activeLabel }}</a-button>
    </PageToolbar>
    <a-table v-if="activeTab === 'labs'" row-key="id" class="management-table" bordered :loading="loading" :data-source="labs" :columns="labColumns" :scroll="tableScroll">
      <template #bodyCell="ctx"><StatusAndActions v-bind="ctx" type="lab" @edit="openEdit" @remove="remove" /></template>
    </a-table>
    <a-table v-else-if="activeTab === 'locations'" row-key="id" class="management-table" bordered :loading="loading" :data-source="locations" :columns="locationColumns" :pagination="false" :scroll="tableScroll">
      <template #bodyCell="ctx"><StatusAndActions v-bind="ctx" type="location" @edit="openEdit" @remove="remove" /></template>
    </a-table>
    <a-table v-else-if="activeTab === 'groups'" row-key="id" class="management-table" bordered :loading="loading" :data-source="groups" :columns="groupColumns" :scroll="tableScroll">
      <template #bodyCell="ctx">
        <template v-if="ctx.column.key === 'labName'">{{ labName(ctx.record.labId) }}</template>
        <template v-else-if="ctx.column.key === 'actions'">
          <a-space><a-button size="small" class="action-secondary" @click="openMembers(ctx.record)">成员</a-button><a-button v-if="canManage" size="small" class="action-edit" @click="openEdit(ctx.record)">编辑</a-button><a-popconfirm v-if="canManage" title="确认删除？" @confirm="remove(ctx.record)"><a-button size="small" danger class="action-delete">删除</a-button></a-popconfirm></a-space>
        </template>
        <a-tag v-else-if="ctx.column.key === 'isActive'" :color="ctx.record.isActive ? 'green' : 'default'">{{ ctx.record.isActive ? '启用' : '停用' }}</a-tag>
        <template v-else>{{ cellValue(ctx.record, ctx.column.dataIndex) }}</template>
      </template>
    </a-table>
    <a-table v-else-if="activeTab === 'suppliers'" row-key="id" class="management-table" bordered :loading="loading" :data-source="suppliers" :columns="supplierColumns" :scroll="tableScroll">
      <template #bodyCell="ctx"><StatusAndActions v-bind="ctx" type="supplier" @edit="openEdit" @remove="remove" /></template>
    </a-table>
    <div v-else class="dict-workbench">
      <a-table row-key="id" bordered :loading="loading" :data-source="dictTypes" :columns="dictTypeColumns" :pagination="false" :custom-row="dictRow">
        <template #bodyCell="ctx"><StatusAndActions v-bind="ctx" type="dictType" @edit="openEdit" @remove="remove" /></template>
      </a-table>
      <section class="dict-items-panel">
        <div class="dict-items-panel__head"><strong>{{ selectedDict?.dictName || '选择字典类型' }}</strong><a-button v-if="canManage && selectedDict" size="small" type="primary" @click="openDictItem()"><PlusOutlined />字典项</a-button></div>
        <a-table row-key="id" bordered size="small" :data-source="dictItems" :columns="dictItemColumns" :pagination="false">
          <template #bodyCell="{ column, record }">
            <a-tag v-if="column.key === 'isActive'" :color="record.isActive ? 'green' : 'default'">{{ record.isActive ? '启用' : '停用' }}</a-tag>
            <a-space v-if="column.key === 'actions'"><a-button v-if="canManage" size="small" class="action-edit" @click="openDictItem(record)">编辑</a-button><a-popconfirm v-if="canManage" title="确认删除？" @confirm="removeDictItem(record.id)"><a-button size="small" danger class="action-delete">删除</a-button></a-popconfirm></a-space>
            <template v-if="column.key !== 'isActive' && column.key !== 'actions'">{{ cellValue(record, column.dataIndex) }}</template>
          </template>
        </a-table>
      </section>
    </div>

    <a-modal v-model:open="editorOpen" :title="`${editing ? '编辑' : '新增'}${activeLabel}`" width="680px" @ok="save">
      <a-form layout="vertical" :model="form">
        <template v-if="activeTab === 'labs'">
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="实验室编码"><a-input v-model:value="form.code" :disabled="!!editing" /></a-form-item></a-col><a-col :span="12"><a-form-item label="实验室名称"><a-input v-model:value="form.name" /></a-form-item></a-col></a-row>
          <a-form-item label="负责人"><a-select v-model:value="form.userId" allow-clear show-search :options="userOptions" /></a-form-item><a-form-item label="说明"><a-textarea v-model:value="form.description" /></a-form-item>
        </template>
        <template v-else-if="activeTab === 'locations'">
          <a-form-item label="所属实验室"><a-select v-model:value="form.labId" :options="labOptions" /></a-form-item><a-form-item label="上级位置"><a-tree-select v-model:value="form.parentId" allow-clear :tree-data="locationOptions" /></a-form-item>
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="位置编码"><a-input v-model:value="form.code" :disabled="!!editing" /></a-form-item></a-col><a-col :span="12"><a-form-item label="位置名称"><a-input v-model:value="form.name" /></a-form-item></a-col></a-row>
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="位置类型"><a-select v-model:value="form.type" :options="locationTypes" /></a-form-item></a-col><a-col :span="12"><a-form-item label="排序"><a-input-number v-model:value="form.sortNo" :min="0" /></a-form-item></a-col></a-row>
        </template>
        <template v-else-if="activeTab === 'groups'">
          <a-form-item label="所属实验室"><a-select v-model:value="form.labId" :options="labOptions" /></a-form-item><a-row :gutter="16"><a-col :span="12"><a-form-item label="课题组编码"><a-input v-model:value="form.code" :disabled="!!editing" /></a-form-item></a-col><a-col :span="12"><a-form-item label="课题组名称"><a-input v-model:value="form.name" /></a-form-item></a-col></a-row><a-form-item label="负责人"><a-select v-model:value="form.userId" allow-clear :options="userOptions" /></a-form-item><a-form-item label="说明"><a-textarea v-model:value="form.description" /></a-form-item>
        </template>
        <template v-else-if="activeTab === 'suppliers'">
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="供应商编码"><a-input v-model:value="form.code" :disabled="!!editing" /></a-form-item></a-col><a-col :span="12"><a-form-item label="供应商名称"><a-input v-model:value="form.name" /></a-form-item></a-col></a-row><a-row :gutter="16"><a-col :span="12"><a-form-item label="联系人"><a-input v-model:value="form.contactName" /></a-form-item></a-col><a-col :span="12"><a-form-item label="电话"><a-input v-model:value="form.phone" /></a-form-item></a-col></a-row><a-form-item label="邮箱"><a-input v-model:value="form.email" /></a-form-item><a-form-item label="地址"><a-input v-model:value="form.address" /></a-form-item>
        </template>
        <template v-else><a-row :gutter="16"><a-col :span="12"><a-form-item label="字典编码"><a-input v-model:value="form.code" :disabled="!!editing" /></a-form-item></a-col><a-col :span="12"><a-form-item label="字典名称"><a-input v-model:value="form.name" /></a-form-item></a-col></a-row><a-form-item label="说明"><a-textarea v-model:value="form.description" /></a-form-item></template>
        <a-checkbox v-model:checked="form.isActive">启用</a-checkbox>
      </a-form>
    </a-modal>

    <a-modal v-model:open="membersOpen" title="课题组成员" width="620px" :footer="null">
      <div v-if="canManage" class="inline-create"><a-select v-model:value="memberForm.userId" placeholder="选择用户" :options="userOptions" /><a-select v-model:value="memberForm.memberRole" :options="memberRoles" /><a-button type="primary" @click="addMember">添加</a-button></div>
      <a-list bordered :data-source="members"><template #renderItem="{ item }"><a-list-item><a-list-item-meta :title="item.displayName || item.userName" :description="item.memberRole" /><a-button v-if="canManage" type="text" danger @click="removeMember(item.id)"><DeleteOutlined /></a-button></a-list-item></template></a-list>
    </a-modal>
    <a-modal v-model:open="dictItemOpen" :title="editingItem ? '编辑字典项' : '新增字典项'" @ok="saveDictItem"><a-form layout="vertical"><a-form-item label="字典值"><a-input v-model:value="itemForm.itemValue" :disabled="!!editingItem" /></a-form-item><a-form-item label="显示名称"><a-input v-model:value="itemForm.itemLabel" /></a-form-item><a-form-item label="排序"><a-input-number v-model:value="itemForm.sortNo" :min="0" /></a-form-item><a-checkbox v-model:checked="itemForm.isActive">启用</a-checkbox></a-form></a-modal>
  </div>
</template>

<script setup lang="ts">
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { computed, defineComponent, h, onMounted, reactive, ref, watch } from 'vue';
import { labFoundationApi as api } from '../../api/lab';
import { userApi } from '../../api/system';
import PageToolbar from '../../components/PageToolbar.vue';
import { useAuthStore } from '../../stores/auth';
import type { DictItemDto, DictTypeDto, GroupMemberDto, LabDto, LabGroupDto, LocationDto, SupplierDto } from '../../types/lab';
import type { UserDto } from '../../types/system';

const props = defineProps<{ initialTab: string }>();
const auth = useAuthStore(); const canManage = computed(() => auth.hasPermission('lab:base:manage'));
const activeTab = ref(props.initialTab); const loading = ref(false); const labs = ref<LabDto[]>([]); const locations = ref<LocationDto[]>([]); const groups = ref<LabGroupDto[]>([]); const suppliers = ref<SupplierDto[]>([]); const dictTypes = ref<DictTypeDto[]>([]); const dictItems = ref<DictItemDto[]>([]); const users = ref<UserDto[]>([]);
const editorOpen = ref(false); const editing = ref<any>(null); const selectedDict = ref<DictTypeDto>(); const membersOpen = ref(false); const memberGroup = ref<LabGroupDto>(); const members = ref<GroupMemberDto[]>([]); const dictItemOpen = ref(false); const editingItem = ref<DictItemDto>();
const form = reactive<any>({}); const memberForm = reactive({ userId: undefined as string | undefined, memberRole: 'member' }); const itemForm = reactive({ itemValue: '', itemLabel: '', sortNo: 0, isActive: true });
const activeLabel = computed(() => ({ labs: '实验室', locations: '位置', groups: '课题组', suppliers: '供应商', dicts: '字典类型' }[activeTab.value] ?? '基础数据'));
const tableScroll = { x: 900, y: 'calc(100vh - 360px)' }; const labOptions = computed(() => labs.value.map(x => ({ label: x.labName, value: x.id }))); const userOptions = computed(() => users.value.map(x => ({ label: x.displayName || x.userName, value: x.id })));
const locationOptions = computed(() => mapTree(locations.value)); const locationTypes = [{ label: '楼栋', value: 'building' }, { label: '房间', value: 'room' }, { label: '区域', value: 'area' }, { label: '柜体', value: 'cabinet' }]; const memberRoles = [{ label: '负责人', value: 'leader' }, { label: '成员', value: 'member' }];
const labColumns = [{ title: '编码', dataIndex: 'labCode' }, { title: '名称', dataIndex: 'labName' }, { title: '负责人', dataIndex: 'managerName' }, { title: '说明', dataIndex: 'description' }, { title: '状态', key: 'isActive', width: 90 }, { title: '操作', key: 'actions', width: 150 }];
const locationColumns = [{ title: '位置名称', dataIndex: 'locationName' }, { title: '编码', dataIndex: 'locationCode' }, { title: '类型', dataIndex: 'locationType' }, { title: '排序', dataIndex: 'sortNo', width: 80 }, { title: '状态', key: 'isActive', width: 90 }, { title: '操作', key: 'actions', width: 150 }];
const groupColumns = [{ title: '编码', dataIndex: 'groupCode' }, { title: '名称', dataIndex: 'groupName' }, { title: '实验室', key: 'labName' }, { title: '负责人', dataIndex: 'leaderName' }, { title: '状态', key: 'isActive', width: 90 }, { title: '操作', key: 'actions', width: 210 }];
const supplierColumns = [{ title: '编码', dataIndex: 'supplierCode' }, { title: '名称', dataIndex: 'supplierName' }, { title: '联系人', dataIndex: 'contactName' }, { title: '电话', dataIndex: 'phone' }, { title: '邮箱', dataIndex: 'email' }, { title: '状态', key: 'isActive', width: 90 }, { title: '操作', key: 'actions', width: 150 }];
const dictTypeColumns = [{ title: '编码', dataIndex: 'dictCode' }, { title: '名称', dataIndex: 'dictName' }, { title: '状态', key: 'isActive', width: 80 }, { title: '操作', key: 'actions', width: 140 }]; const dictItemColumns = [{ title: '值', dataIndex: 'itemValue' }, { title: '名称', dataIndex: 'itemLabel' }, { title: '排序', dataIndex: 'sortNo', width: 70 }, { title: '状态', key: 'isActive', width: 80 }, { title: '操作', key: 'actions', width: 130 }];

const StatusAndActions = defineComponent({ props: ['column', 'record', 'type'], emits: ['edit', 'remove'], setup(p, { emit }) { return () => p.column.key === 'isActive' ? h('span', { class: ['ant-tag', p.record.isActive ? 'ant-tag-green' : ''] }, p.record.isActive ? '启用' : '停用') : p.column.key === 'actions' ? h('div', { class: 'table-actions' }, canManage.value ? [h('button', { class: 'ant-btn ant-btn-sm action-edit', onClick: () => emit('edit', p.record) }, '编辑'), h('button', { class: 'ant-btn ant-btn-sm ant-btn-dangerous action-delete', onClick: () => emit('remove', p.record) }, '删除')] : []) : h('span', String(p.record[p.column.dataIndex] ?? '-')); } });

onMounted(async () => { users.value = await userApi.list(); labs.value = await api.labs(); await loadActive(); });
watch(() => props.initialTab, async (value) => { activeTab.value = value; selectedDict.value = undefined; dictItems.value = []; await loadActive(); });
async function loadActive() { loading.value = true; try { if (activeTab.value === 'labs') labs.value = await api.labs(); if (activeTab.value === 'locations') locations.value = await api.locations(); if (activeTab.value === 'groups') groups.value = await api.groups(); if (activeTab.value === 'suppliers') suppliers.value = await api.suppliers(); if (activeTab.value === 'dicts') { dictTypes.value = await api.dictTypes(); if (!selectedDict.value && dictTypes.value[0]) await selectDict(dictTypes.value[0]); } } finally { loading.value = false; } }
function resetForm() { Object.assign(form, { code: '', name: '', labId: labs.value[0]?.id, parentId: undefined, userId: undefined, type: 'room', sortNo: 0, description: '', contactName: '', phone: '', email: '', address: '', isActive: true }); }
function openCreate() { editing.value = null; resetForm(); editorOpen.value = true; }
function openEdit(row: any) { editing.value = row; resetForm(); Object.assign(form, { code: row.labCode || row.locationCode || row.groupCode || row.supplierCode || row.dictCode, name: row.labName || row.locationName || row.groupName || row.supplierName || row.dictName, labId: row.labId, parentId: row.parentId || undefined, userId: row.managerId || row.leaderId || undefined, type: row.locationType, sortNo: row.sortNo || 0, description: row.description || '', contactName: row.contactName || '', phone: row.phone || '', email: row.email || '', address: row.address || '', isActive: row.isActive }); editorOpen.value = true; }
async function save() { const payloads: any = { labs: { labCode: form.code, labName: form.name, managerId: form.userId, description: form.description, isActive: form.isActive }, locations: { locationCode: form.code, locationName: form.name, labId: form.labId, parentId: form.parentId, locationType: form.type, sortNo: form.sortNo, isActive: form.isActive }, groups: { groupCode: form.code, groupName: form.name, labId: form.labId, leaderId: form.userId, description: form.description, isActive: form.isActive }, suppliers: { supplierCode: form.code, supplierName: form.name, contactName: form.contactName, phone: form.phone, email: form.email, address: form.address, isActive: form.isActive }, dicts: { dictCode: form.code, dictName: form.name, description: form.description, isActive: form.isActive } }; const names: any = { labs: ['createLab', 'updateLab'], locations: ['createLocation', 'updateLocation'], groups: ['createGroup', 'updateGroup'], suppliers: ['createSupplier', 'updateSupplier'], dicts: ['createDictType', 'updateDictType'] }; const [create, update] = names[activeTab.value]; if (editing.value) await (api as any)[update](editing.value.id, payloads[activeTab.value]); else await (api as any)[create](payloads[activeTab.value]); message.success('保存成功'); editorOpen.value = false; await loadActive(); }
async function remove(row: any) { const names: any = { labs: 'removeLab', locations: 'removeLocation', groups: 'removeGroup', suppliers: 'removeSupplier', dicts: 'removeDictType' }; await (api as any)[names[activeTab.value]](row.id); message.success('删除成功'); await loadActive(); }
function labName(id: string) { return labs.value.find(x => x.id === id)?.labName || '-'; } function mapTree(rows: LocationDto[]): any[] { return rows.map(x => ({ label: x.locationName, value: x.id, children: mapTree(x.children || []) })); }
function cellValue(record: Record<string, any>, dataIndex?: string | number | readonly (string | number)[]) { if (Array.isArray(dataIndex)) return dataIndex.reduce<any>((value, key) => value?.[key], record) ?? '-'; return dataIndex === undefined ? '-' : record[String(dataIndex)] ?? '-'; }
function dictRow(row: DictTypeDto) { return { onClick: () => selectDict(row), class: selectedDict.value?.id === row.id ? 'dict-row--selected' : '' }; } async function selectDict(row: DictTypeDto) { selectedDict.value = row; dictItems.value = await api.dictItems(row.id); }
async function openMembers(row: LabGroupDto | Record<string, any>) { const group = row as LabGroupDto; memberGroup.value = group; members.value = await api.members(group.id); membersOpen.value = true; } async function addMember() { if (!memberGroup.value || !memberForm.userId) return; await api.addMember(memberGroup.value.id, { userId: memberForm.userId, memberRole: memberForm.memberRole }); members.value = await api.members(memberGroup.value.id); memberForm.userId = undefined; } async function removeMember(id: string) { if (!memberGroup.value) return; await api.removeMember(memberGroup.value.id, id); members.value = await api.members(memberGroup.value.id); }
function openDictItem(row?: DictItemDto | Record<string, any>) { const item = row as DictItemDto | undefined; editingItem.value = item; Object.assign(itemForm, item || { itemValue: '', itemLabel: '', sortNo: 0, isActive: true }); dictItemOpen.value = true; } async function saveDictItem() { if (!selectedDict.value) return; if (editingItem.value) await api.updateDictItem(editingItem.value.id, itemForm); else await api.createDictItem(selectedDict.value.id, itemForm); dictItemOpen.value = false; await selectDict(selectedDict.value); } async function removeDictItem(id: string) { await api.removeDictItem(id); if (selectedDict.value) await selectDict(selectedDict.value); }
</script>
