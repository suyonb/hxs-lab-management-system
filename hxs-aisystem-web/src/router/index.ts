import { createRouter, createWebHashHistory, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { getMyMenus } from '../api/system';
import { isDemoMode } from '../demo/mode';
import { useAuthStore } from '../stores/auth';
import type { MenuDto } from '../types/system';

const viewModules = import.meta.glob('../views/**/*.vue');
const dynamicRouteRemovers = new Map<string, () => void>();
let loadedToken = '';
let loadingPromise: Promise<void> | null = null;

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/auth/LoginView.vue'),
    meta: { public: true }
  },
  {
    path: '/',
    name: 'app-shell',
    component: () => import('../layouts/AppLayout.vue'),
    children: []
  }
];

if (import.meta.env.DEV) {
  routes.push({
    path: '/__lab3d-preview',
    name: 'lab3d-preview',
    component: () => import('../views/lab/Lab3dView.vue'),
    meta: { public: true }
  });
}

const router = createRouter({ history: isDemoMode ? createWebHashHistory() : createWebHistory(), routes });

export function syncDynamicRoutes(menus: MenuDto[]) {
  const pages = flattenMenus(menus).filter((item) => item.menuType === 'page' && item.routePath && item.component);
  const activeNames = new Set(pages.map(routeName));

  for (const [name, remove] of dynamicRouteRemovers) {
    if (!activeNames.has(name)) {
      remove();
      dynamicRouteRemovers.delete(name);
    }
  }

  for (const menu of pages) {
    const name = routeName(menu);
    if (dynamicRouteRemovers.has(name)) continue;
    const component = resolveView(menu.component!);
    if (!component) {
      console.error(`[dynamic-router] 组件文件不存在：${menu.component}`);
      continue;
    }

    const path = menu.routePath === '/' ? '' : menu.routePath!.replace(/^\/+/, '');
    const remove = router.addRoute('app-shell', {
      path,
      name,
      component,
      meta: { menuCode: menu.menuCode, permissionCode: menu.permissionCode }
    });
    dynamicRouteRemovers.set(name, remove);
  }
}

export async function ensureDynamicRoutes(force = false) {
  const token = useAuthStore().accessToken;
  if (!token) return;
  if (!force && loadedToken === token) return;
  if (loadingPromise) return loadingPromise;

  loadingPromise = getMyMenus()
    .then((menus) => {
      syncDynamicRoutes(menus);
      loadedToken = token;
    })
    .finally(() => {
      loadingPromise = null;
    });
  return loadingPromise;
}

export function resetDynamicRoutes() {
  for (const remove of dynamicRouteRemovers.values()) remove();
  dynamicRouteRemovers.clear();
  loadedToken = '';
}

router.beforeEach(async (to) => {
  const authStore = useAuthStore();
  if (!to.meta.public && !authStore.accessToken) return '/login';
  if (to.path === '/login' && authStore.accessToken) return '/';
  if (authStore.accessToken && to.path !== '/login') {
    const matchedBeforeLoad = to.matched.length;
    await ensureDynamicRoutes();
    if (matchedBeforeLoad === 0 || (to.path === '/' && to.matched.length === 1)) return to.fullPath;
  }
  return true;
});

function resolveView(componentPath: string) {
  const normalized = componentPath.trim().replace(/\\/g, '/').replace(/^\/+/, '').replace(/^src\//, '');
  return viewModules[`../${normalized}`];
}

function flattenMenus(items: MenuDto[]): MenuDto[] {
  return items.flatMap((item) => [item, ...flattenMenus(item.children ?? [])]);
}

function routeName(menu: MenuDto) {
  return `dynamic-${menu.id}`;
}

export default router;
