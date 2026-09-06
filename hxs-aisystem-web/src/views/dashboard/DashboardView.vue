<template>
  <div class="lab-dashboard">
    <section class="lab-dashboard__context">
      <div class="lab-dashboard__context-copy">
        <div>
          <h1>实验室运行总览</h1>
          <span>仪器、空间与业务待办实时汇总</span>
        </div>
        <span>{{ todayText }} · 今日 {{ todayBookings.length }} 项预约 · {{ operations.pendingCount }} 项待办</span>
        <em :class="{ 'is-warning': !healthOk }"><i></i>{{ healthOk ? '整体运行平稳' : '服务状态检查中' }}</em>
      </div>
      <div class="lab-dashboard__context-actions">
        <a-button @click="router.push('/lab/bookings')"><CalendarOutlined />预约管理</a-button>
        <a-button type="primary" @click="router.push('/lab/approval-center')"><CheckCircleOutlined />处理待办</a-button>
      </div>
    </section>

    <div class="lab-metric-grid">
      <button v-for="item in metrics" :key="item.label" type="button" class="lab-metric-card" :class="`is-${item.tone}`" @click="router.push(item.path)">
        <span class="lab-metric-card__icon"><component :is="item.icon" /></span>
        <span class="lab-metric-card__label">{{ item.label }}</span>
        <span class="lab-metric-card__value"><strong>{{ item.value }}</strong><i>{{ item.unit }}</i></span>
        <small>{{ item.note }}</small>
        <ArrowRightOutlined class="lab-metric-card__arrow" />
      </button>
    </div>

    <div class="lab-dashboard__grid">
      <section class="lab-panel lab-panel--rooms">
        <header class="lab-panel__header">
          <div><strong>实验室运行态势</strong><span>按空间快速查看资源和风险</span></div>
          <button class="lab-text-action" type="button" @click="router.push('/lab/3d')">进入空间视图<ArrowUpOutlined /></button>
        </header>
        <div v-if="floorOptions.length > 1" class="lab-floor-tabs">
          <button v-for="floor in floorOptions" :key="floor" type="button" :class="{ active: activeFloor === floor }" @click="activeFloor = floor">{{ floor }}</button>
        </div>
        <div v-if="visibleRooms.length" class="lab-room-grid">
          <button v-for="room in visibleRooms.slice(0, 6)" :key="room.id" type="button" class="lab-room-card" :class="{ 'is-active': room.bookingCount > 0, 'is-warning': room.repairCount > 0 }" @click="router.push('/lab/3d')">
            <span>{{ room.locationCode }}</span><i>{{ room.repairCount ? '需关注' : room.bookingCount ? '运行中' : '正常' }}</i>
            <strong>{{ room.locationName }}</strong>
            <small>{{ room.instrumentCount }} 台设备 · {{ room.repairCount ? `${room.repairCount} 台维修` : `${room.bookingCount} 项预约` }}</small>
          </button>
        </div>
        <a-empty v-else :image="simpleImage" description="暂无实验室空间数据" />
      </section>

      <section class="lab-panel lab-panel--schedule">
        <header class="lab-panel__header">
          <div><strong>今日安排</strong><span>按时间排序</span></div>
          <button class="lab-icon-action" type="button" title="查看全部预约" @click="router.push('/lab/bookings')"><ArrowRightOutlined /></button>
        </header>
        <div v-if="todayBookings.length" class="lab-schedule-list">
          <button v-for="item in todayBookings.slice(0, 5)" :key="item.id" type="button" @click="router.push('/lab/bookings')">
            <time>{{ formatTime(item.startTime) }}</time><i :class="`is-${item.status}`"></i>
            <span><strong>{{ item.instrumentName || '未命名仪器' }}</strong><small>{{ item.purpose }}</small></span>
            <em>{{ statusText[item.status] || item.status }}</em>
          </button>
        </div>
        <a-empty v-else :image="simpleImage" description="今日暂无仪器预约" />
      </section>

      <section class="lab-panel lab-panel--trend">
        <header class="lab-panel__header">
          <div><strong>近七日设备使用</strong><span>预约与实际使用次数</span></div>
          <div class="lab-chart-legend"><span><i></i>预约</span><span><i></i>使用</span></div>
        </header>
        <div class="lab-bar-chart" aria-label="近七日设备使用柱状图">
          <div v-for="item in usageTrend" :key="item.date">
            <span><i :style="{ height: trendHeight(item.bookings) }"></i><b :style="{ height: trendHeight(item.usages) }"></b></span>
            <small>{{ item.label }}</small>
          </div>
        </div>
      </section>

      <section class="lab-panel lab-panel--risks">
        <header class="lab-panel__header">
          <div><strong>风险与待办</strong><span>需要优先处理</span></div>
          <button class="lab-text-action" type="button" @click="router.push('/lab/approval-center')">查看全部<ArrowRightOutlined /></button>
        </header>
        <div class="lab-risk-list">
          <button v-for="item in riskItems" :key="item.key" type="button" @click="router.push(item.path)">
            <span :class="`is-${item.tone}`"><component :is="item.icon" /></span>
            <b>{{ item.title }}</b><small>{{ item.detail }}</small><ArrowRightOutlined />
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  ArrowRightOutlined,
  ArrowUpOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  DatabaseOutlined,
  ExperimentOutlined,
  ToolOutlined,
  WarningOutlined
} from '@ant-design/icons-vue';
import { Empty } from 'ant-design-vue';
import dayjs from 'dayjs';
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { instrumentApi } from '../../api/instrument';
import { labFoundationApi } from '../../api/lab';
import { operationsApi } from '../../api/operations';
import { getHealth } from '../../api/system';
import type { BookingDto, InstrumentDto, RepairDto } from '../../types/instrument';
import type { LocationDto } from '../../types/lab';
import type { DashboardSummaryDto } from '../../types/operations';

const router = useRouter();
const simpleImage = Empty.PRESENTED_IMAGE_SIMPLE;
const healthOk = ref(false);
const activeFloor = ref('全部楼层');
const instruments = ref<InstrumentDto[]>([]);
const bookings = ref<BookingDto[]>([]);
const repairs = ref<RepairDto[]>([]);
const locations = ref<LocationDto[]>([]);
const todayText = dayjs().format('YYYY年MM月DD日');
const operations = reactive<DashboardSummaryDto>({
  pendingCount: 0,
  todayBookings: 0,
  repairingInstruments: 0,
  lowStockCount: 0,
  expiringCount: 0,
  expiredCount: 0,
  recentExperimentCount: 0,
  archivedExperimentCount: 0,
  instrumentUsageTrend: [],
  materialConsumptionTrend: []
});

const statusText: Record<string, string> = {
  pending: '待审核',
  approved: '已通过',
  rejected: '已驳回',
  cancelled: '已取消',
  completed: '已完成'
};

const todayBookings = computed(() => bookings.value
  .filter((item) => dayjs(item.startTime).isSame(dayjs(), 'day'))
  .sort((a, b) => dayjs(a.startTime).valueOf() - dayjs(b.startTime).valueOf()));
const normalCount = computed(() => instruments.value.filter((item) => item.isActive && item.status === 'normal').length);
const repairCount = computed(() => instruments.value.filter((item) => item.status === 'repair').length);
const availableRate = computed(() => instruments.value.length ? Math.round(normalCount.value / instruments.value.length * 1000) / 10 : 0);
const warningCount = computed(() => operations.lowStockCount + operations.expiringCount + operations.expiredCount);

const metrics = computed(() => [
  { label: '今日预约', value: todayBookings.value.length, unit: '项', note: `${todayBookings.value.filter((item) => item.status === 'approved').length} 项已通过 · ${todayBookings.value.filter((item) => item.status === 'pending').length} 项待审`, icon: CalendarOutlined, tone: 'blue', path: '/lab/bookings' },
  { label: '设备可用率', value: availableRate.value, unit: '%', note: `${normalCount.value} 台正常 · ${repairCount.value} 台维修`, icon: CheckCircleOutlined, tone: 'green', path: '/lab/instruments' },
  { label: '库存预警', value: warningCount.value, unit: '项', note: `低库存 ${operations.lowStockCount} · 临期 ${operations.expiringCount} · 过期 ${operations.expiredCount}`, icon: WarningOutlined, tone: 'amber', path: '/lab/inventory-warnings' },
  { label: '近七日实验', value: operations.recentExperimentCount, unit: '项', note: `累计归档 ${operations.archivedExperimentCount} 项`, icon: ExperimentOutlined, tone: 'violet', path: '/lab/experiments' }
]);

const flatLocations = computed(() => flattenLocations(locations.value));
const locationMap = computed(() => new Map(flatLocations.value.map((item) => [item.id, item])));
const floorOptions = computed(() => ['全部楼层', ...new Set(flatLocations.value.filter((item) => item.locationType === 'floor').map((item) => item.locationName))]);
const roomSummaries = computed(() => flatLocations.value
  .filter((item) => item.locationType === 'room' && item.isActive)
  .map((room) => {
    const roomInstruments = instruments.value.filter((instrument) => belongsToRoom(instrument.locationId, room.id));
    const ids = new Set(roomInstruments.map((instrument) => instrument.id));
    const floor = findAncestor(room, 'floor');
    return {
      ...room,
      floorName: floor?.locationName || '未分层',
      instrumentCount: roomInstruments.length,
      repairCount: roomInstruments.filter((instrument) => instrument.status === 'repair').length,
      bookingCount: todayBookings.value.filter((booking) => ids.has(booking.instrumentId)).length
    };
  })
  .sort((a, b) => a.sortNo - b.sortNo));
const visibleRooms = computed(() => activeFloor.value === '全部楼层' ? roomSummaries.value : roomSummaries.value.filter((item) => item.floorName === activeFloor.value));

const usageTrend = computed(() => Array.from({ length: 7 }, (_, index) => {
  const date = dayjs().subtract(6 - index, 'day');
  const dateKey = date.format('YYYY-MM-DD');
  const shortKey = date.format('MM-DD');
  const usages = operations.instrumentUsageTrend.find((item) => item.date === dateKey || item.date === shortKey)?.value || 0;
  return {
    date: dateKey,
    label: index === 6 ? '今日' : `周${'日一二三四五六'[date.day()]}`,
    bookings: bookings.value.filter((item) => dayjs(item.startTime).format('YYYY-MM-DD') === dateKey).length,
    usages
  };
}));
const trendMax = computed(() => Math.max(...usageTrend.value.flatMap((item) => [item.bookings, item.usages]), 1));

const activeRepair = computed(() => [...repairs.value]
  .filter((item) => ['pending', 'approved', 'repairing'].includes(item.status))
  .sort((a, b) => dayjs(b.createTime).valueOf() - dayjs(a.createTime).valueOf())[0]);
const riskItems = computed(() => [
  {
    key: 'repair',
    title: activeRepair.value ? `${activeRepair.value.instrumentName || '仪器'}报修待处理` : '当前无设备报修待处理',
    detail: activeRepair.value?.faultDescription || '设备运行状态稳定',
    tone: activeRepair.value ? 'red' : 'green',
    icon: ToolOutlined,
    path: '/lab/repairs'
  },
  {
    key: 'stock',
    title: warningCount.value ? `${warningCount.value} 项库存预警` : '当前无库存预警',
    detail: warningCount.value ? `低库存 ${operations.lowStockCount} · 临期/过期 ${operations.expiringCount + operations.expiredCount}` : '库存状态正常',
    tone: warningCount.value ? 'amber' : 'green',
    icon: DatabaseOutlined,
    path: '/lab/inventory-warnings'
  },
  {
    key: 'approval',
    title: operations.pendingCount ? `${operations.pendingCount} 项业务申请待审批` : '当前无业务申请待审批',
    detail: operations.pendingCount ? '预约、领用与报修需要处理' : '审批任务已处理完成',
    tone: operations.pendingCount ? 'blue' : 'green',
    icon: CheckCircleOutlined,
    path: '/lab/approval-center'
  }
]);

onMounted(async () => {
  const results = await Promise.allSettled([
    instrumentApi.instruments(false, true),
    instrumentApi.bookings(false, undefined, true),
    instrumentApi.repairs(false, undefined, true),
    labFoundationApi.locations(undefined, false, true),
    getHealth(true),
    operationsApi.dashboard(7, true)
  ]);
  if (results[0].status === 'fulfilled') instruments.value = results[0].value;
  if (results[1].status === 'fulfilled') bookings.value = results[1].value;
  if (results[2].status === 'fulfilled') repairs.value = results[2].value;
  if (results[3].status === 'fulfilled') locations.value = results[3].value;
  healthOk.value = results[4].status === 'fulfilled';
  if (results[5].status === 'fulfilled') Object.assign(operations, results[5].value);
});

function flattenLocations(items: LocationDto[]): LocationDto[] {
  return items.flatMap((item) => [item, ...flattenLocations(item.children || [])]);
}

function findAncestor(location: LocationDto, type: string): LocationDto | undefined {
  let current: LocationDto | undefined = location;
  while (current) {
    if (current.locationType === type) return current;
    current = current.parentId ? locationMap.value.get(current.parentId) : undefined;
  }
}

function belongsToRoom(locationId: string, roomId: string) {
  let current = locationMap.value.get(locationId);
  while (current) {
    if (current.id === roomId) return true;
    current = current.parentId ? locationMap.value.get(current.parentId) : undefined;
  }
  return false;
}

function formatTime(value: string) {
  return dayjs(value).format('HH:mm');
}

function trendHeight(value: number) {
  return `${Math.max(value ? 12 : 3, Math.round(value / trendMax.value * 94))}%`;
}
</script>
