<template>
  <div class="page-surface management-page menu-management-page">
    <PageToolbar eyebrow="Menus" title="菜单管理">
      <a-button v-if="authStore.hasPermission('sys:menu:create')" type="primary" class="action-create" @click="openCreate">新增菜单</a-button>
    </PageToolbar>
    <a-table row-key="id" class="management-table" bordered :columns="columns" :data-source="items" :loading="loading" :pagination="false" :scroll="tableScroll" :row-class-name="rowClassName">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'menuName'">
          <span class="resource-name" :class="{ 'resource-name--permission': record.menuType === 'button' }">
            <ControlOutlined v-if="record.menuType === 'button'" />
            <span>{{ record.menuName }}</span>
          </span>
        </template>
        <template v-if="column.key === 'menuType'">
          <a-tag :color="typeColor[record.menuType]">{{ typeText[record.menuType] || record.menuType }}</a-tag>
        </template>
        <template v-if="column.key === 'flags'">
          <a-space>
            <a-tag :color="record.isVisible ? 'blue' : 'default'">{{ record.isVisible ? '可见' : '隐藏' }}</a-tag>
            <a-tag :color="record.isActive ? 'green' : 'default'">{{ record.isActive ? '启用' : '停用' }}</a-tag>
          </a-space>
        </template>
        <template v-if="column.key === 'actions'">
          <a-space>
            <a-button v-if="authStore.hasPermission('sys:menu:edit')" size="small" class="action-edit" @click="openEdit(record)">编辑</a-button>
            <a-popconfirm v-if="authStore.hasPermission('sys:menu:delete')" title="确认删除该菜单？" @confirm="remove(record.id)">
              <a-button size="small" danger class="action-delete">删除</a-button>
            </a-popconfirm>
          </a-space>
        </template>
      </template>
    </a-table>

    <a-modal v-model:open="modalOpen" :title="editing ? '编辑菜单' : '新增菜单'" width="720px" class="menu-editor-modal" @ok="save">
      <a-form layout="vertical" :model="form" class="menu-editor-form">
        <a-form-item label="上级菜单"><a-tree-select v-model:value="form.parentId" allow-clear :tree-data="menuOptions" placeholder="选择上级菜单" /></a-form-item>
        <a-row :gutter="16">
          <a-col :xs="24" :sm="12"><a-form-item label="菜单名称"><a-input v-model:value="form.menuName" placeholder="输入菜单名称" /></a-form-item></a-col>
          <a-col :xs="24" :sm="12"><a-form-item label="菜单编码"><a-input v-model:value="form.menuCode" placeholder="例如 lab:example" /></a-form-item></a-col>
        </a-row>
        <a-row :gutter="16">
          <a-col :xs="24" :sm="8"><a-form-item label="类型"><a-select v-model:value="form.menuType" :options="menuTypes" /></a-form-item></a-col>
          <a-col :xs="24" :sm="8"><a-form-item label="排序"><a-input-number v-model:value="form.sortNo" :min="0" class="menu-editor-sort" /></a-form-item></a-col>
          <a-col :xs="24" :sm="8">
            <a-form-item label="图标">
              <div class="menu-icon-picker">
                <a-popover v-model:open="iconPickerOpen" trigger="click" placement="bottomLeft" overlay-class-name="menu-icon-picker__overlay">
                  <a-button class="menu-icon-picker__trigger" title="选择菜单图标">
                    <component v-if="form.icon" :is="resolveMenuIcon(form.icon)" />
                    <PlusOutlined v-else />
                    <DownOutlined class="menu-icon-picker__arrow" />
                  </a-button>
                  <template #content>
                    <div class="menu-icon-grid">
                      <a-tooltip v-for="item in menuIconOptions" :key="item.value" :title="item.label">
                        <button type="button" :class="{ active: form.icon === item.value }" @click="selectIcon(item.value)">
                          <component :is="resolveMenuIcon(item.value)" />
                        </button>
                      </a-tooltip>
                    </div>
                  </template>
                </a-popover>
                <a-button v-if="form.icon" class="menu-icon-picker__clear" type="text" title="清除图标" @click="form.icon = ''"><CloseOutlined /></a-button>
              </div>
            </a-form-item>
          </a-col>
        </a-row>
        <template v-if="form.menuType === 'page'">
          <a-form-item label="路由路径"><a-input v-model:value="form.routePath" placeholder="/lab/example" /></a-form-item>
          <a-form-item label="组件文件"><a-input v-model:value="form.component" placeholder="views/lab/ExampleView.vue" /></a-form-item>
        </template>
        <a-form-item label="权限标识"><a-input v-model:value="form.permissionCode" placeholder="例如 lab:example:view" /></a-form-item>
        <div class="menu-editor-status">
          <a-checkbox v-model:checked="form.isVisible">菜单可见</a-checkbox>
          <a-checkbox v-model:checked="form.isActive">状态启用</a-checkbox>
        </div>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { message } from 'ant-design-vue';
import { CloseOutlined, ControlOutlined, DownOutlined, PlusOutlined } from '@ant-design/icons-vue';
import { computed, onMounted, reactive, ref } from 'vue';
import { menuApi } from '../../api/system';
import { ensureDynamicRoutes } from '../../router';
import { menuIconOptions, resolveMenuIcon } from '../../config/menuIcons';
import PageToolbar from '../../components/PageToolbar.vue';
import type { MenuDto } from '../../types/system';
import { useAuthStore } from '../../stores/auth';

const loading = ref(false);
const authStore = useAuthStore();
const items = ref<MenuDto[]>([]);
const modalOpen = ref(false);
const iconPickerOpen = ref(false);
const editing = ref<MenuDto | null>(null);
const form = reactive({
  parentId: undefined as string | undefined,
  menuCode: '',
  menuName: '',
  menuType: 'page',
  routePath: '',
  component: '',
  icon: '',
  permissionCode: '',
  sortNo: 0,
  isVisible: true,
  isActive: true
});
const typeText: Record<string, string> = { directory: '目录', page: '页面', button: '操作权限' };
const typeColor: Record<string, string> = { directory: 'blue', page: 'cyan', button: 'gold' };
const tableScroll = { x: 1050, y: 'calc(100vh - 280px)' };
const menuTypes = [
  { label: '目录', value: 'directory' },
  { label: '页面', value: 'page' },
  { label: '按钮', value: 'button' }
];
const columns = [
  { title: '资源名称', dataIndex: 'menuName', key: 'menuName' },
  { title: '编码', dataIndex: 'menuCode' },
  { title: '类型', key: 'menuType', width: 100 },
  { title: '路由', dataIndex: 'routePath' },
  { title: '组件文件', dataIndex: 'component' },
  { title: '权限标识', dataIndex: 'permissionCode' },
  { title: '排序', dataIndex: 'sortNo', width: 80 },
  { title: '状态', key: 'flags', width: 150 },
  { title: '操作', key: 'actions', width: 150 }
];
const menuOptions = computed(() => toTreeOptions(items.value));

function rowClassName(record: MenuDto) {
  return record.menuType === 'button' ? 'permission-row' : '';
}

onMounted(load);

async function load() {
  loading.value = true;
  try {
    items.value = buildMenuTree(await menuApi.list());
  } finally {
    loading.value = false;
  }
}

function buildMenuTree(rows: MenuDto[]): MenuDto[] {
  const nodes = new Map(rows.map((row) => [row.id, { ...row, children: [] as MenuDto[] }]));
  const roots: MenuDto[] = [];

  for (const node of nodes.values()) {
    const parent = node.parentId ? nodes.get(node.parentId) : undefined;
    if (parent) parent.children!.push(node);
    else roots.push(node);
  }

  return roots;
}

function openCreate() {
  editing.value = null;
  Object.assign(form, { parentId: undefined, menuCode: '', menuName: '', menuType: 'page', routePath: '', component: '', icon: '', permissionCode: '', sortNo: 0, isVisible: true, isActive: true });
  modalOpen.value = true;
}

function openEdit(record: MenuDto | Record<string, any>) {
  const item = record as MenuDto;
  editing.value = item;
  Object.assign(form, {
    parentId: item.parentId ?? undefined,
    menuCode: item.menuCode,
    menuName: item.menuName,
    menuType: item.menuType,
    routePath: item.routePath ?? '',
    component: item.component ?? '',
    icon: item.icon ?? '',
    permissionCode: item.permissionCode ?? '',
    sortNo: item.sortNo,
    isVisible: item.isVisible,
    isActive: item.isActive
  });
  modalOpen.value = true;
}

function selectIcon(value: string) {
  form.icon = value;
  iconPickerOpen.value = false;
}

async function save() {
  const payload = { ...form, parentId: form.parentId || null };
  if (editing.value) await menuApi.update(editing.value.id, payload);
  else await menuApi.create(payload);
  message.success('保存成功');
  modalOpen.value = false;
  await load();
  await ensureDynamicRoutes(true);
}

async function remove(id: string) {
  await menuApi.remove(id);
  message.success('删除成功');
  await load();
  await ensureDynamicRoutes(true);
}

function toTreeOptions(nodes: MenuDto[]): Array<{ label: string; value: string; children?: Array<{ label: string; value: string }> }> {
  return nodes.map((item) => ({ label: item.menuName, value: item.id, children: item.children ? toTreeOptions(item.children) : undefined }));
}
</script>
