<template>
  <div class="page-surface management-page">
    <PageToolbar eyebrow="Organization" title="组织架构">
      <a-button v-if="authStore.hasPermission('sys:org:create')" type="primary" class="action-create" @click="openCreate">新增组织</a-button>
    </PageToolbar>
    <a-table row-key="id" class="management-table" bordered :columns="columns" :data-source="items" :loading="loading" :pagination="false" :scroll="tableScroll">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isActive'">
          <a-tag :color="record.isActive ? 'green' : 'default'">{{ record.isActive ? '启用' : '停用' }}</a-tag>
        </template>
        <template v-if="column.key === 'actions'">
          <a-space>
            <a-button v-if="authStore.hasPermission('sys:org:edit')" size="small" class="action-edit" @click="openEdit(record)">编辑</a-button>
            <a-popconfirm v-if="authStore.hasPermission('sys:org:delete')" title="确认删除该组织？" @confirm="remove(record.id)">
              <a-button size="small" danger class="action-delete">删除</a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </template>
    </a-table>
    <a-modal v-model:open="modalOpen" ok-text="保存组织" @ok="save">
      <template #title><AppModalTitle :title="editing ? '编辑组织' : '新增组织'" subtitle="维护部门层级、编码与启用状态" icon="organization" /></template>
      <a-form layout="vertical" :model="form">
        <a-form-item label="上级组织"><a-tree-select v-model:value="form.parentId" allow-clear :tree-data="treeOptions" /></a-form-item>
        <a-form-item label="组织名称"><a-input v-model:value="form.orgName" /></a-form-item>
        <a-form-item label="组织编码"><a-input v-model:value="form.orgCode" /></a-form-item>
        <a-form-item label="组织类型"><a-select v-model:value="form.orgType" :options="orgTypes" /></a-form-item>
        <a-form-item label="排序"><a-input-number v-model:value="form.sortNo" :min="0" /></a-form-item>
        <a-form-item><a-checkbox v-model:checked="form.isActive">启用</a-checkbox></a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { message } from 'ant-design-vue';
import { computed, onMounted, reactive, ref } from 'vue';
import { orgApi } from '../../api/system';
import PageToolbar from '../../components/PageToolbar.vue';
import type { OrgDto } from '../../types/system';
import { useAuthStore } from '../../stores/auth';

const loading = ref(false);
const authStore = useAuthStore();
const items = ref<OrgDto[]>([]);
const modalOpen = ref(false);
const editing = ref<OrgDto | null>(null);
const form = reactive({ parentId: undefined as string | undefined, orgName: '', orgCode: '', orgType: 'department', sortNo: 0, isActive: true });
const orgTypes = [
  { label: '公司', value: 'company' },
  { label: '部门', value: 'department' },
  { label: '小组', value: 'team' }
];
const columns = [
  { title: '组织名称', dataIndex: 'orgName' },
  { title: '编码', dataIndex: 'orgCode' },
  { title: '类型', dataIndex: 'orgType' },
  { title: '排序', dataIndex: 'sortNo', width: 90 },
  { title: '状态', key: 'isActive', width: 90 },
  { title: '操作', key: 'actions', width: 150 }
];
const tableScroll = { x: 760, y: 'calc(100vh - 280px)' };
const treeOptions = computed(() => toTreeOptions(items.value));

onMounted(load);

async function load() {
  loading.value = true;
  try {
    items.value = await orgApi.tree();
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  editing.value = null;
  Object.assign(form, { parentId: undefined, orgName: '', orgCode: '', orgType: 'department', sortNo: 0, isActive: true });
  modalOpen.value = true;
}

function openEdit(record: OrgDto | Record<string, any>) {
  const item = record as OrgDto;
  editing.value = item;
  Object.assign(form, { parentId: item.parentId ?? undefined, orgName: item.orgName, orgCode: item.orgCode, orgType: item.orgType, sortNo: item.sortNo, isActive: item.isActive });
  modalOpen.value = true;
}

async function save() {
  const payload = { ...form, parentId: form.parentId || null };
  if (editing.value) await orgApi.update(editing.value.id, payload);
  else await orgApi.create(payload);
  message.success('保存成功');
  modalOpen.value = false;
  await load();
}

async function remove(id: string) {
  await orgApi.remove(id);
  message.success('删除成功');
  await load();
}

function toTreeOptions(nodes: OrgDto[]): Array<{ label: string; value: string; children?: Array<{ label: string; value: string }> }> {
  return nodes.map((item) => ({ label: item.orgName, value: item.id, children: item.children ? toTreeOptions(item.children) : undefined }));
}
</script>
