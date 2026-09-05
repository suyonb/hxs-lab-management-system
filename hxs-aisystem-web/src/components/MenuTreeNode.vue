<template>
  <a-sub-menu v-if="visibleChildren.length" :key="item.routePath || item.menuCode">
    <template #icon><component :is="resolveMenuIcon(item.icon)" /></template>
    <template #title>{{ item.menuName }}</template>
    <MenuTreeNode v-for="child in visibleChildren" :key="child.id" :item="child" />
  </a-sub-menu>
  <a-menu-item v-else :key="item.routePath || item.menuCode">
    <template #icon><component :is="resolveMenuIcon(item.icon)" /></template>
    <span>{{ item.menuName }}</span>
  </a-menu-item>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { resolveMenuIcon } from '../config/menuIcons';
import type { MenuDto } from '../types/system';

defineOptions({ name: 'MenuTreeNode' });
const props = defineProps<{ item: MenuDto }>();
const visibleChildren = computed(() => (props.item.children ?? []).filter((child) => child.menuType !== 'button'));
</script>
