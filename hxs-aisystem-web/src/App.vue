<template>
  <a-config-provider :theme="antTheme" :locale="zhCN">
    <router-view />
  </a-config-provider>
</template>

<script setup lang="ts">
import zhCN from 'ant-design-vue/es/locale/zh_CN';
import { theme as antDesignTheme } from 'ant-design-vue';
import { computed } from 'vue';
import { useThemeStore } from './stores/theme';

const themeStore = useThemeStore();
themeStore.applyThemeVars();

const antTheme = computed(() => ({
  algorithm: themeStore.activeThemeKey === 'personal' ? antDesignTheme.darkAlgorithm : antDesignTheme.defaultAlgorithm,
  token: {
    colorPrimary: themeStore.currentTheme.primary,
    colorInfo: themeStore.currentTheme.primary,
    borderRadius: themeStore.currentTheme.radius,
    borderRadiusLG: themeStore.currentTheme.radius,
    borderRadiusSM: Math.max(4, themeStore.currentTheme.radius - 2),
    controlHeight: 34,
    controlHeightLG: 38,
    fontSize: 13,
    fontFamily: themeStore.currentTheme.fontFamily,
    colorText: themeStore.activeThemeKey === 'personal' ? '#f8f3ea' : '#1a2723',
    colorTextSecondary: themeStore.activeThemeKey === 'personal' ? '#c9bca9' : '#66756f',
    colorBgContainer: themeStore.activeThemeKey === 'personal' ? '#1e1c18' : '#ffffff',
    colorBorder: themeStore.activeThemeKey === 'personal' ? '#4b4338' : '#d9e2de'
  },
  components: {
    Button: {
      colorPrimary: themeStore.currentTheme.buttonColor,
      borderRadius: themeStore.currentTheme.buttonRadius,
      borderRadiusLG: themeStore.currentTheme.buttonRadius,
      controlHeight: 34,
      paddingInline: 13,
      fontWeight: 600
    },
    Table: {
      headerBg: themeStore.activeThemeKey === 'personal' ? '#29251f' : '#f5f8f7',
      headerColor: themeStore.activeThemeKey === 'personal' ? '#d7cab9' : '#596862',
      borderColor: themeStore.activeThemeKey === 'personal' ? '#4b4338' : '#d9e2de',
      cellPaddingBlock: 10,
      cellPaddingInline: 12
    },
    Modal: {
      titleFontSize: 16,
      titleLineHeight: 1.4
    }
  }
}));
</script>
