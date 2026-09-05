import {
  ApartmentOutlined,
  AppstoreOutlined,
  BarChartOutlined,
  BulbOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  DashboardOutlined,
  DatabaseOutlined,
  EnvironmentOutlined,
  ExperimentOutlined,
  FileTextOutlined,
  HistoryOutlined,
  HomeOutlined,
  MenuOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  ShopOutlined,
  TeamOutlined,
  ToolOutlined,
  UserOutlined
} from '@ant-design/icons-vue';
import type { Component } from 'vue';

const menuIcons: Array<{ value: string; label: string; component: Component }> = [
  { value: 'dashboard', label: '首页总览', component: DashboardOutlined },
  { value: 'home', label: '首页', component: HomeOutlined },
  { value: 'experiment', label: '实验室/仪器', component: ExperimentOutlined },
  { value: 'building', label: '组织/楼栋', component: ApartmentOutlined },
  { value: 'location', label: '位置', component: EnvironmentOutlined },
  { value: 'users', label: '用户/课题组', component: TeamOutlined },
  { value: 'user', label: '个人用户', component: UserOutlined },
  { value: 'shield', label: '权限/审批', component: SafetyCertificateOutlined },
  { value: 'check', label: '审核完成', component: CheckCircleOutlined },
  { value: 'calendar', label: '预约/日历', component: CalendarOutlined },
  { value: 'history', label: '记录/历史', component: HistoryOutlined },
  { value: 'database', label: '数据/字典', component: DatabaseOutlined },
  { value: 'shop', label: '供应商/库存', component: ShopOutlined },
  { value: 'file', label: '文件/记录', component: FileTextOutlined },
  { value: 'chart', label: '统计图表', component: BarChartOutlined },
  { value: 'bulb', label: 'AI/推理', component: BulbOutlined },
  { value: 'tool', label: '维修/工具', component: ToolOutlined },
  { value: 'apps', label: '应用模块', component: AppstoreOutlined },
  { value: 'menu', label: '菜单管理', component: MenuOutlined },
  { value: 'settings', label: '系统设置', component: SettingOutlined }
];

const iconMap = Object.fromEntries(menuIcons.map((item) => [item.value, item.component])) as Record<string, Component>;

export const menuIconOptions = menuIcons.map(({ value, label }) => ({ value, label }));

export function resolveMenuIcon(value?: string | null): Component {
  return iconMap[value || 'dashboard'] ?? DashboardOutlined;
}
