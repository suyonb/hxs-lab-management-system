import { defineStore } from 'pinia';

export type ThemeKey = 'business' | 'personal' | 'neo' | 'classic';
export type MenuMode = 'theme' | 'light';

export interface AppTheme {
  key: ThemeKey;
  name: string;
  description: string;
  primary: string;
  accent: string;
  buttonColor: string;
  radius: number;
  buttonRadius: number;
  fontFamily: string;
  headerBg: string;
  headerText: string;
  headerOpacity: number;
  cardOpacity: number;
  backgroundImage: string;
}

export const themes: AppTheme[] = [
  {
    key: 'business',
    name: '商务简洁',
    description: '实验室绿、紧凑卡片、适合后台高频操作。',
    primary: '#137a63',
    accent: '#3168b7',
    buttonColor: '#137a63',
    radius: 8,
    buttonRadius: 6,
    fontFamily: "'Inter', 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif",
    headerBg: '#ffffff',
    headerText: '#111827',
    headerOpacity: 0.9,
    cardOpacity: 0.92,
    backgroundImage: ''
  },
  {
    key: 'personal',
    name: '个人高级',
    description: '深色质感、琥珀点缀、适合个人工作台和决策视图。',
    primary: '#b7791f',
    accent: '#7c3aed',
    buttonColor: '#b7791f',
    radius: 10,
    buttonRadius: 7,
    fontFamily: "'SF Pro Display', 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif",
    headerBg: '#1f1b16',
    headerText: '#f8f3ea',
    headerOpacity: 0.9,
    cardOpacity: 0.9,
    backgroundImage: ''
  },
  {
    key: 'neo',
    name: '科技专业',
    description: '冷静黑白、青色强调、适合 AI 和数据产品。',
    primary: '#0891b2',
    accent: '#4f46e5',
    buttonColor: '#0891b2',
    radius: 8,
    buttonRadius: 6,
    fontFamily: "'Inter', 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif",
    headerBg: '#f8fbfc',
    headerText: '#0f172a',
    headerOpacity: 0.92,
    cardOpacity: 0.94,
    backgroundImage: ''
  },
  {
    key: 'classic',
    name: '经典明亮',
    description: '稳重灰阶、红色强调、适合传统管理系统。',
    primary: '#b91c1c',
    accent: '#475569',
    buttonColor: '#b91c1c',
    radius: 8,
    buttonRadius: 6,
    fontFamily: "'PingFang SC', 'Microsoft YaHei', Arial, sans-serif",
    headerBg: '#ffffff',
    headerText: '#1f2937',
    headerOpacity: 0.92,
    cardOpacity: 0.94,
    backgroundImage: ''
  }
];

type ThemeOverrides = Partial<
  Pick<
    AppTheme,
    'primary' | 'accent' | 'buttonColor' | 'radius' | 'buttonRadius' | 'fontFamily' | 'headerBg' | 'headerText' | 'headerOpacity' | 'cardOpacity' | 'backgroundImage'
  >
>;

export const useThemeStore = defineStore('theme', {
  state: () => ({
    activeThemeKey: readThemeKey(),
    overrides: readCustomTheme(),
    menuMode: readMenuMode()
  }),
  getters: {
    currentTheme(state): AppTheme {
      return {
        ...(themes.find((item) => item.key === state.activeThemeKey) ?? themes[0]),
        ...state.overrides
      };
    }
  },
  actions: {
    setTheme(key: ThemeKey) {
      this.activeThemeKey = key;
      this.overrides = {};
      localStorage.setItem('hxs-theme', key);
      localStorage.removeItem('hxs-theme-custom');
      this.applyThemeVars();
    },
    updateOverrides(values: ThemeOverrides) {
      this.overrides = { ...this.overrides, ...values };
      localStorage.setItem('hxs-theme-custom', JSON.stringify(this.overrides));
      this.applyThemeVars();
    },
    setMenuMode(mode: MenuMode) {
      this.menuMode = mode;
      localStorage.setItem('hxs-menu-mode', mode);
      this.applyThemeVars();
    },
    resetCurrentTheme() {
      this.overrides = {};
      localStorage.removeItem('hxs-theme-custom');
      this.applyThemeVars();
    },
    applyThemeVars() {
      const theme = this.currentTheme;
      document.documentElement.dataset.theme = this.activeThemeKey;
      document.documentElement.dataset.menuMode = this.menuMode;
      document.documentElement.style.setProperty('--primary', theme.primary);
      document.documentElement.style.setProperty('--accent', theme.accent);
      document.documentElement.style.setProperty('--button-color', theme.buttonColor);
      document.documentElement.style.setProperty('--radius', `${theme.radius}px`);
      document.documentElement.style.setProperty('--button-radius', `${theme.buttonRadius}px`);
      document.documentElement.style.setProperty('--font-family', theme.fontFamily);
      document.documentElement.style.setProperty('--header-bg', theme.headerBg);
      document.documentElement.style.setProperty('--header-text', theme.headerText);
      document.documentElement.style.setProperty('--header-opacity', String(theme.headerOpacity));
      document.documentElement.style.setProperty('--card-opacity', String(theme.cardOpacity));
      document.documentElement.style.setProperty('--background-image', theme.backgroundImage ? `url(${theme.backgroundImage})` : 'none');
    }
  }
});

function readThemeKey(): ThemeKey {
  const saved = localStorage.getItem('hxs-theme') as ThemeKey | null;
  return saved && themes.some((item) => item.key === saved) ? saved : 'business';
}

function readMenuMode(): MenuMode {
  return localStorage.getItem('hxs-menu-mode') === 'light' ? 'light' : 'theme';
}

function readCustomTheme(): ThemeOverrides {
  const value = localStorage.getItem('hxs-theme-custom');
  if (!value) return {};
  try {
    const parsed = JSON.parse(value) as ThemeOverrides;
    if (parsed.headerBg && !parsed.headerBg.startsWith('#')) {
      delete parsed.headerBg;
    }
    return parsed;
  } catch {
    return {};
  }
}
