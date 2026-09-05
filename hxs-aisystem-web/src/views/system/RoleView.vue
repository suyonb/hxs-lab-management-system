<template>
  <div class="page-surface management-page">
    <PageToolbar eyebrow="Roles" title="角色管理">
      <a-button v-if="authStore.hasPermission('sys:role:create')" type="primary" class="action-create" @click="openCreate">新增角色</a-button>
    </PageToolbar>
    <a-table row-key="id" class="management-table" bordered :columns="columns" :data-source="items" :loading="loading" :scroll="tableScroll">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isActive'">
          <a-tag :color="record.isActive ? 'green' : 'default'">{{ record.isActive ? '启用' : '停用' }}</a-tag>
        </template>
        <template v-if="column.key === 'actions'">
          <a-space>
            <a-button v-if="authStore.hasPermission('sys:role:assign-menu')" size="small" class="action-secondary" @click="openMenus(record)">授权</a-button>
            <a-button v-if="authStore.hasPermission('sys:role:edit')" size="small" class="action-edit" @click="openEdit(record)">编辑</a-button>
            <a-popconfirm v-if="authStore.hasPermission('sys:role:delete')" title="确认删除该角色？" @confirm="remove(record.id)">
              <a-button size="small" danger class="action-delete">删除</a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </template>
    </a-table>

    <a-modal v-model:open="modalOpen" :title="editing ? '编辑角色' : '新增角色'" @ok="save">
      <a-form layout="vertical" :model="form">
        <a-form-item label="角色编码"><a-input v-model:value="form.roleCode" /></a-form-item>
        <a-form-item label="角色名称"><a-input v-model:value="form.roleName" /></a-form-item>
        <a-form-item label="描述"><a-textarea v-model:value="form.description" /></a-form-item>
        <a-form-item><a-checkbox v-model:checked="form.isActive">启用</a-checkbox></a-form-item>
      </a-form>
    </a-modal>

    <a-modal v-model:open="menuOpen" title="菜单与操作权限" width="620px" @ok="saveMenus">
      <div class="permission-legend">
        <span><FolderOutlined /> 目录</span><span><FileOutlined /> 页面</span><span class="permission-legend__action"><ControlOutlined /> 操作权限</span>
      </div>
      <a-tree v-model:checkedKeys="checkedMenuIds" class="permission-tree" checkable check-strictly default-expand-all :tree-data="menuOptions">
        <template #title="node">
          <span class="permission-tree__title" :class="{ 'permission-tree__title--action': node.menuType === 'button' }">
            <component :is="resourceIcon(node.menuType)" />
            <span>{{ node.title }}</span>
            <a-tag v-if="node.menuType === 'button'" color="gold">操作权限</a-tag>
          </span>
        </template>
      </a-tree>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { message } from 'ant-design-vue';
import { ControlOutlined, FileOutlined, FolderOutlined } from '@ant-design/icons-vue';
import { computed, onMounted, reactive, ref } from 'vue';
import { menuApi, roleApi } from '../../api/system';
import PageToolbar from '../../components/PageToolbar.vue';
import type { MenuDto, RoleDto } from '../../types/system';
import { useAuthStore } from '../../stores/auth';

const loading = ref(false);
const authStore = useAuthStore();
const items = ref<RoleDto[]>([]);
const menus = ref<MenuDto[]>([]);
const modalOpen = ref(false);
const menuOpen = ref(false);
const editing = ref<RoleDto | null>(null);
const menuTarget = ref<RoleDto | null>(null);
const checkedMenuIds = ref<string[] | { checked: string[]; halfChecked: string[] }>([]);
const form = reactive({ roleCode: '', roleName: '', description: '', isActive: true });
const columns = [
  { title: '角色编码', dataIndex: 'roleCode' },
  { title: '角色名称', dataIndex: 'roleName' },
  { title: '描述', dataIndex: 'description' },
  { title: '状态', key: 'isActive', width: 90 },
  { title: '操作', key: 'actions', width: 190 }
];
const tableScroll = { x: 760, y: 'calc(100vh - 350px)' };
const menuOptions = computed(() => toTreeOptions(menus.value));

onMounted(async () => {
  await Promise.all([load(), loadMenus()]);
});

async function load() {
  loading.value = true;
  try {
    items.value = await roleApi.list();
  } finally {
    loading.value = false;
  }
}

async function loadMenus() {
  menus.value = await menuApi.tree();
}

function openCreate() {
  editing.value = null;
  Object.assign(form, { roleCode: '', roleName: '', description: '', isActive: true });
  modalOpen.value = true;
}

function openEdit(record: RoleDto | Record<string, any>) {
  const item = record as RoleDto;
  editing.value = item;
  Object.assign(form, { roleCode: item.roleCode, roleName: item.roleName, description: item.description ?? '', isActive: item.isActive });
  modalOpen.value = true;
}

async function save() {
  if (editing.value) await roleApi.update(editing.value.id, form);
  else await roleApi.create(form);
  message.success('保存成功');
  modalOpen.value = false;
  await load();
}

async function remove(id: string) {
  await roleApi.remove(id);
  message.success('删除成功');
  await load();
}

async function openMenus(record: RoleDto | Record<string, any>) {
  const item = record as RoleDto;
  menuTarget.value = item;
  const owned = await roleApi.menus(item.id);
  checkedMenuIds.value = owned.map((item) => item.id);
  menuOpen.value = true;
}

async function saveMenus() {
  if (!menuTarget.value) return;
  const menuIds = Array.isArray(checkedMenuIds.value) ? checkedMenuIds.value : checkedMenuIds.value.checked;
  await roleApi.assignMenus(menuTarget.value.id, { menuIds });
  message.success('菜单授权已更新');
  menuOpen.value = false;
}

function toTreeOptions(nodes: MenuDto[]): Array<{ title: string; key: string; menuType: string; children?: ReturnType<typeof toTreeOptions> }> {
  return nodes.map((item) => ({ title: item.menuName, key: item.id, menuType: item.menuType, children: item.children ? toTreeOptions(item.children) : undefined }));
}

function resourceIcon(menuType: string) {
  return menuType === 'button' ? ControlOutlined : menuType === 'directory' ? FolderOutlined : FileOutlined;
}
</script>
