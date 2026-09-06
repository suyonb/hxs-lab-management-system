<template>
  <span class="app-modal-title" :class="`app-modal-title--${tone}`">
    <span class="app-modal-title__icon" aria-hidden="true">
      <component :is="iconComponent" />
    </span>
    <span class="app-modal-title__copy">
      <strong>{{ title }}</strong>
      <small v-if="subtitle">{{ subtitle }}</small>
    </span>
    <span v-if="status" class="app-modal-title__status">{{ status }}</span>
  </span>
</template>

<script setup lang="ts">
import {
  ApartmentOutlined,
  AppstoreOutlined,
  AuditOutlined,
  BgColorsOutlined,
  DatabaseOutlined,
  DeploymentUnitOutlined,
  ExperimentOutlined,
  FileTextOutlined,
  MenuOutlined,
  SafetyCertificateOutlined,
  TagsOutlined,
  TeamOutlined,
  ToolOutlined,
  UserOutlined
} from '@ant-design/icons-vue';
import { computed } from 'vue';

type ModalIcon =
  | 'approval'
  | 'default'
  | 'dictionary'
  | 'experiment'
  | 'inventory'
  | 'maintenance'
  | 'members'
  | 'menu'
  | 'organization'
  | 'record'
  | 'role'
  | 'spatial'
  | 'theme'
  | 'user';

const props = withDefaults(defineProps<{
  title: string;
  subtitle?: string;
  icon?: ModalIcon;
  status?: string;
  tone?: 'default' | 'approval' | 'danger' | 'spatial';
}>(), {
  subtitle: '',
  icon: 'default',
  status: '',
  tone: 'default'
});

const icons = {
  approval: AuditOutlined,
  default: AppstoreOutlined,
  dictionary: TagsOutlined,
  experiment: ExperimentOutlined,
  inventory: DatabaseOutlined,
  maintenance: ToolOutlined,
  members: TeamOutlined,
  menu: MenuOutlined,
  organization: ApartmentOutlined,
  record: FileTextOutlined,
  role: SafetyCertificateOutlined,
  spatial: DeploymentUnitOutlined,
  theme: BgColorsOutlined,
  user: UserOutlined
};

const iconComponent = computed(() => icons[props.icon]);
</script>
