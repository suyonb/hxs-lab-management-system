<template>
  <div class="page-surface management-page">
    <PageToolbar eyebrow="Users" title="用户管理">
      <a-input-search v-model:value="keyword" class="search-input" placeholder="搜索用户名或显示名" @search="load" />
      <a-button v-if="authStore.hasPermission('sys:user:create')" type="primary" class="action-create" @click="openCreate">新增用户</a-button>
    </PageToolbar>
    <a-table row-key="id" class="management-table" bordered :columns="columns" :data-source="items" :loading="loading" :scroll="tableScroll">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isActive'">
          <a-tag :color="record.isActive ? 'green' : 'default'">{{ record.isActive ? '启用' : '停用' }}</a-tag>
        </template>
        <template v-if="column.key === 'lastLoginTime'">{{ record.lastLoginTime ? dayjs(record.lastLoginTime).format('YYYY-MM-DD HH:mm') : '-' }}</template>
        <template v-if="column.key === 'actions'">
          <a-space>
            <a-button v-if="authStore.hasPermission('sys:user:assign-role')" size="small" class="action-secondary" @click="openRoles(record)">角色</a-button>
            <a-button v-if="authStore.hasPermission('sys:user:edit')" size="small" class="action-edit" @click="openEdit(record)">编辑</a-button>
            <a-popconfirm v-if="authStore.hasPermission('sys:user:delete')" title="确认删除该用户？" @confirm="remove(record.id)">
              <a-button size="small" danger class="action-delete">删除</a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </template>
    </a-table>

    <a-modal v-model:open="modalOpen" ok-text="保存用户" @ok="save">
      <template #title><AppModalTitle :title="editing ? '编辑用户' : '新增用户'" subtitle="维护登录身份、组织归属与基础联系方式" icon="user" /></template>
      <a-form layout="vertical" :model="form">
        <a-form-item label="所属组织"><a-tree-select v-model:value="form.orgId" allow-clear :tree-data="orgOptions" /></a-form-item>
        <a-form-item v-if="!editing" label="用户名"><a-input v-model:value="form.userName" /></a-form-item>
        <a-form-item label="显示名称"><a-input v-model:value="form.displayName" /></a-form-item>
        <a-form-item :label="editing ? '新密码' : '密码'"><a-input-password v-model:value="form.password" :placeholder="editing ? '留空则不修改密码' : ''" /></a-form-item>
        <a-form-item label="手机号"><a-input v-model:value="form.phone" /></a-form-item>
        <a-form-item label="邮箱"><a-input v-model:value="form.email" /></a-form-item>
        <a-form-item><a-checkbox v-model:checked="form.isActive">启用</a-checkbox></a-form-item>
      </a-form>
    </a-modal>

    <a-modal v-model:open="roleOpen" ok-text="保存分配" @ok="saveRoles">
      <template #title><AppModalTitle title="分配角色" subtitle="角色将决定用户可访问的菜单与操作范围" icon="role" /></template>
      <a-checkbox-group v-model:value="selectedRoleIds" class="check-list">
        <a-checkbox v-for="role in roles" :key="role.id" :value="role.id">{{ role.roleName }}（{{ role.roleCode }}）</a-checkbox>
      </a-checkbox-group>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { message } from 'ant-design-vue';
import dayjs from 'dayjs';
import { computed, onMounted, reactive, ref } from 'vue';
import { orgApi, roleApi, userApi } from '../../api/system';
import PageToolbar from '../../components/PageToolbar.vue';
import type { OrgDto, RoleDto, UserDto } from '../../types/system';
import { useAuthStore } from '../../stores/auth';

const loading = ref(false);
const authStore = useAuthStore();
const keyword = ref('');
const items = ref<UserDto[]>([]);
const orgs = ref<OrgDto[]>([]);
const roles = ref<RoleDto[]>([]);
const modalOpen = ref(false);
const roleOpen = ref(false);
const editing = ref<UserDto | null>(null);
const roleTarget = ref<UserDto | null>(null);
const selectedRoleIds = ref<string[]>([]);
const form = reactive({ orgId: undefined as string | undefined, userName: '', displayName: '', password: '', phone: '', email: '', isActive: true });
const columns = [
  { title: '用户名', dataIndex: 'userName' },
  { title: '显示名称', dataIndex: 'displayName' },
  { title: '手机号', dataIndex: 'phone' },
  { title: '邮箱', dataIndex: 'email' },
  { title: '状态', key: 'isActive', width: 90 },
  { title: '最后登录', key: 'lastLoginTime', width: 160 },
  { title: '操作', key: 'actions', width: 210 }
];
const tableScroll = { x: 980, y: 'calc(100vh - 350px)' };
const orgOptions = computed(() => toTreeOptions(orgs.value));

onMounted(async () => {
  await Promise.all([load(), loadMeta()]);
});

async function load() {
  loading.value = true;
  try {
    items.value = await userApi.list(keyword.value);
  } finally {
    loading.value = false;
  }
}

async function loadMeta() {
  [orgs.value, roles.value] = await Promise.all([orgApi.tree(), roleApi.list()]);
}

function openCreate() {
  editing.value = null;
  Object.assign(form, { orgId: undefined, userName: '', displayName: '', password: '', phone: '', email: '', isActive: true });
  modalOpen.value = true;
}

function openEdit(record: UserDto | Record<string, any>) {
  const item = record as UserDto;
  editing.value = item;
  Object.assign(form, { orgId: item.orgId ?? undefined, userName: item.userName, displayName: item.displayName ?? '', password: '', phone: item.phone ?? '', email: item.email ?? '', isActive: item.isActive });
  modalOpen.value = true;
}

async function save() {
  const payload = { ...form, orgId: form.orgId || null };
  if (editing.value) await userApi.update(editing.value.id, payload);
  else await userApi.create({ ...payload, password: form.password });
  message.success('保存成功');
  modalOpen.value = false;
  await load();
}

async function remove(id: string) {
  await userApi.remove(id);
  message.success('删除成功');
  await load();
}

async function openRoles(record: UserDto | Record<string, any>) {
  const item = record as UserDto;
  roleTarget.value = item;
  const owned = await userApi.roles(item.id);
  selectedRoleIds.value = owned.map((item) => item.id);
  roleOpen.value = true;
}

async function saveRoles() {
  if (!roleTarget.value) return;
  await userApi.assignRoles(roleTarget.value.id, { roleIds: selectedRoleIds.value });
  message.success('角色已更新');
  roleOpen.value = false;
}

function toTreeOptions(nodes: OrgDto[]): Array<{ label: string; value: string; children?: Array<{ label: string; value: string }> }> {
  return nodes.map((item) => ({ label: item.orgName, value: item.id, children: item.children ? toTreeOptions(item.children) : undefined }));
}
</script>
