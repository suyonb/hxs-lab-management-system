<template>
  <a-layout class="shell">
    <a-layout-sider v-model:collapsed="collapsed" class="shell__sider" :width="224" :collapsed-width="66">
      <div class="brand" :class="{ 'brand--collapsed': collapsed }">
        <a-dropdown :trigger="['click']">
          <button class="brand__user" type="button" title="用户菜单">
            <span class="brand__mark">{{ userInitial }}</span>
            <span v-if="!collapsed" class="brand__text">
              <strong>{{ currentUser?.displayName || currentUser?.userName }}</strong>
              <span>{{ currentUser?.userName || '当前登录用户' }}</span>
            </span>
          </button>
          <template #overlay>
            <a-menu>
              <a-menu-item key="logout" @click="logout">退出登录</a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
        <a-button type="text" class="sider-collapse-btn" @click="collapsed = !collapsed">
          <MenuFoldOutlined v-if="!collapsed" />
          <MenuUnfoldOutlined v-else />
        </a-button>
      </div>
      <a-menu v-model:selectedKeys="selectedKeys" v-model:openKeys="openKeys" mode="inline" class="shell__menu" @click="go">
        <MenuTreeNode v-for="item in visibleMenus" :key="item.id" :item="item" />
      </a-menu>
    </a-layout-sider>
    <a-layout>
      <a-layout-header class="shell__header">
        <div class="header-title header-card">
          <div class="header-system-title">
            <span class="header-system-title__icon"><ExperimentOutlined /></span>
            <strong><b>HXS</b> 实验室管理系统</strong>
          </div>
        </div>
        <div class="header-actions header-card">
          <span v-if="isDemoMode" class="demo-status"><ExperimentOutlined />在线演示</span>
          <a-popconfirm v-if="isDemoMode" title="恢复初始演示数据？" ok-text="恢复" cancel-text="取消" @confirm="resetDemo">
            <a-button class="demo-reset-button" title="恢复演示数据"><ReloadOutlined /></a-button>
          </a-popconfirm>
          <a-button class="theme-entry-button" @click="themeDrawerOpen = true">
            <BgColorsOutlined />
            <span>{{ activeThemeName }}</span>
          </a-button>
        </div>
      </a-layout-header>
      <a-layout-content class="shell__content">
        <router-view />
      </a-layout-content>
    </a-layout>
    <a-drawer v-model:open="themeDrawerOpen" placement="right" width="400" class="theme-drawer">
      <template #title><AppModalTitle title="主题细化" subtitle="调整系统主题、菜单形态与界面字体" icon="theme" /></template>
      <div class="theme-editor">
        <section class="theme-editor__section theme-editor__section--first">
          <h3>主题风格</h3>
          <a-segmented v-model:value="activeThemeKey" class="theme-style-segmented" :options="themeOptions" />
          <h3 class="theme-editor__subheading">菜单模式</h3>
          <a-segmented v-model:value="menuMode" class="theme-menu-segmented" :options="menuModeOptions" />
        </section>

        <div class="theme-editor__preview" :style="{ background: currentTheme.headerBg, color: currentTheme.headerText }">
          <span class="theme-editor__preview-mark" :style="{ background: currentTheme.primary }">H</span>
          <span><strong>{{ activeThemeName }}</strong><small>{{ activeThemeDescription }}</small></span>
          <i :style="{ background: currentTheme.accent }"></i>
        </div>

        <section class="theme-editor__section">
          <h3>颜色</h3>
          <div class="theme-color-grid">
            <label v-for="item in colorOptions" :key="item.key" class="theme-color-field">
              <span>{{ item.label }}</span>
              <span class="theme-color-control">
                <input :value="currentTheme[item.key]" type="color" @input="updateColor(item.key, $event)" />
                <code>{{ currentTheme[item.key] }}</code>
              </span>
            </label>
          </div>
        </section>

        <section class="theme-editor__section">
          <h3>形态与透明度</h3>
          <label class="theme-slider-field">
            <span>头部透明度 <b>{{ Math.round(currentTheme.headerOpacity * 100) }}%</b></span>
            <a-slider :value="currentTheme.headerOpacity" :min="0.7" :max="1" :step="0.01" @change="updateNumber('headerOpacity', $event)" />
          </label>
          <label class="theme-slider-field">
            <span>卡片透明度 <b>{{ Math.round(currentTheme.cardOpacity * 100) }}%</b></span>
            <a-slider :value="currentTheme.cardOpacity" :min="0.78" :max="1" :step="0.01" @change="updateNumber('cardOpacity', $event)" />
          </label>
          <div class="theme-slider-grid">
            <label class="theme-slider-field">
              <span>整体圆角 <b>{{ currentTheme.radius }}px</b></span>
              <a-slider :value="currentTheme.radius" :min="8" :max="28" @change="updateNumber('radius', $event)" />
            </label>
            <label class="theme-slider-field">
              <span>按钮圆角 <b>{{ currentTheme.buttonRadius }}px</b></span>
              <a-slider :value="currentTheme.buttonRadius" :min="6" :max="32" @change="updateNumber('buttonRadius', $event)" />
            </label>
          </div>
        </section>

        <section class="theme-editor__section">
          <h3>字体与背景</h3>
          <label class="theme-select-field">
            <span>界面字体</span>
            <a-select :value="currentTheme.fontFamily" @change="updateFont">
              <a-select-option v-for="item in fontOptions" :key="item.value" :value="item.value">{{ item.label }}</a-select-option>
            </a-select>
          </label>
          <div class="theme-background-actions">
            <label class="theme-upload-button">
              <PictureOutlined />
              <span>{{ currentTheme.backgroundImage ? '更换背景图' : '上传背景图' }}</span>
              <input type="file" accept="image/*" @change="uploadBackground" />
            </label>
            <a-button v-if="currentTheme.backgroundImage" danger title="清除背景图" @click="clearBackground"><DeleteOutlined /></a-button>
          </div>
        </section>

        <a-button class="theme-reset-button" block @click="themeStore.resetCurrentTheme"><ReloadOutlined />恢复当前主题默认</a-button>
      </div>
    </a-drawer>
  </a-layout>
</template>

<script setup lang="ts">
import {
  BgColorsOutlined,
  DeleteOutlined,
  ExperimentOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  PictureOutlined,
  ReloadOutlined
} from '@ant-design/icons-vue';
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getMyMenus, getMyPermissions } from '../api/system';
import { resetDemoState } from '../demo/adapter';
import { isDemoMode } from '../demo/mode';
import { resetDynamicRoutes, syncDynamicRoutes } from '../router';
import { useAuthStore } from '../stores/auth';
import { themes, useThemeStore, type AppTheme, type MenuMode, type ThemeKey } from '../stores/theme';
import type { MenuDto } from '../types/system';
import MenuTreeNode from '../components/MenuTreeNode.vue';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const themeStore = useThemeStore();
const compactMenuQuery = window.matchMedia('(max-width: 720px)');
const collapsed = ref(compactMenuQuery.matches);
const selectedKeys = ref([route.path]);
const openKeys = ref<string[]>([]);
const menuTree = ref<MenuDto[]>([]);
const themeDrawerOpen = ref(false);

const fontOptions = [
  { label: '系统苹果风', value: "'SF Pro Display', 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif" },
  { label: 'Inter 专业风', value: "'Inter', 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif" },
  { label: '中文稳重风', value: "'PingFang SC', 'Microsoft YaHei', Arial, sans-serif" },
  { label: '现代系统风', value: "system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Microsoft YaHei', sans-serif" },
  { label: '宋体阅读风', value: "'Songti SC', 'STSong', 'SimSun', serif" }
];

const visibleMenus = computed(() => menuTree.value.filter((item) => item.menuType !== 'button'));
const currentUser = computed(() => authStore.currentUser);
const userInitial = computed(() => (currentUser.value?.displayName || currentUser.value?.userName || 'H').trim().slice(0, 1).toUpperCase());
const activeThemeKey = computed({
  get: () => themeStore.activeThemeKey,
  set: (value) => {
    themeStore.setTheme(value);
  }
});
const menuMode = computed({
  get: () => themeStore.menuMode,
  set: (value: MenuMode) => themeStore.setMenuMode(value)
});
const currentTheme = computed(() => themeStore.currentTheme);
const activeThemeName = computed(() => themes.find((item) => item.key === activeThemeKey.value)?.name ?? '当前主题');
const activeThemeDescription = computed(() => themes.find((item) => item.key === activeThemeKey.value)?.description ?? '');
const colorOptions: Array<{ label: string; key: 'primary' | 'accent' | 'buttonColor' | 'headerBg' | 'headerText' }> = [
  { label: '主色', key: 'primary' },
  { label: '强调色', key: 'accent' },
  { label: '按钮色', key: 'buttonColor' },
  { label: '头部背景', key: 'headerBg' },
  { label: '头部文字', key: 'headerText' }
];
const themeOptions = themes.map((item) => ({ label: item.name, value: item.key }));
const menuModeOptions = [
  { label: '主题同步', value: 'theme' },
  { label: '明亮菜单', value: 'light' }
];

onMounted(()=>{loadMenus();compactMenuQuery.addEventListener('change',syncCompactMenu);});
onBeforeUnmount(()=>compactMenuQuery.removeEventListener('change',syncCompactMenu));

watch(
  () => route.path,
  (path) => {
    selectedKeys.value = [path];
    openKeys.value = collapsed.value ? [] : findOpenKeys(menuTree.value, path);
  },
  { immediate: true }
);

watch(collapsed, (value) => {
  openKeys.value = value ? [] : findOpenKeys(menuTree.value, route.path);
});

function go(info: { key: string | number }) {
  const key = String(info.key);
  if (!key.startsWith('/')) return;
  router.push(key);
}

function syncCompactMenu(event:MediaQueryListEvent){collapsed.value=event.matches;}

async function loadMenus() {
  const [menus, permissions] = await Promise.all([getMyMenus(), getMyPermissions()]);
  menuTree.value = menus;
  syncDynamicRoutes(menus);
  authStore.setPermissions(permissions);
  openKeys.value = collapsed.value ? [] : findOpenKeys(menuTree.value, route.path);
}

function updateColor(key: 'primary' | 'accent' | 'buttonColor' | 'headerBg' | 'headerText', event: Event) {
  themeStore.updateOverrides({ [key]: (event.target as HTMLInputElement).value });
}

function updateNumber(key: 'radius' | 'buttonRadius' | 'cardOpacity' | 'headerOpacity', value: number | [number, number]) {
  themeStore.updateOverrides({ [key]: Array.isArray(value) ? value[0] : value });
}

function updateFont(value: unknown) {
  themeStore.updateOverrides({ fontFamily: value as AppTheme['fontFamily'] });
}

function uploadBackground(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;
  const reader = new FileReader();
  reader.onload = () => {
    themeStore.updateOverrides({ backgroundImage: String(reader.result) });
  };
  reader.readAsDataURL(file);
}

function clearBackground() {
  themeStore.updateOverrides({ backgroundImage: '' });
}


function findOpenKeys(items: MenuDto[], path: string) {
  return findMenuPath(items, path) ?? [];
}

function findMenuPath(items: MenuDto[], path: string, parents: string[] = []): string[] | null {
  for (const item of items) {
    const key = item.routePath || item.menuCode;
    if (key === path) return parents;
    const result = findMenuPath(item.children ?? [], path, [...parents, key]);
    if (result) return result;
  }
  return null;
}

function logout() {
  resetDynamicRoutes();
  authStore.clearSession();
  router.replace('/login');
}

function resetDemo() {
  resetDemoState();
  window.location.reload();
}
</script>
