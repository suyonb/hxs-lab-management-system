<template>
  <div class="page-surface management-page">
    <PageToolbar eyebrow="Audit" title="操作日志">
      <a-input-search v-model:value="keyword" class="search-input" placeholder="用户、模块或操作" @search="search" />
      <a-range-picker v-model:value="dateRange" show-time @change="search" />
    </PageToolbar>
    <a-table row-key="id" class="management-table" bordered :columns="columns" :data-source="items" :loading="loading" :pagination="pagination" :scroll="tableScroll" @change="changePage">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'result'">
          <a-tag :color="record.result === 'success' ? 'green' : 'red'">{{ record.result === 'success' ? '成功' : '失败' }}</a-tag>
        </template>
        <template v-if="column.key === 'createTime'">{{ dayjs(record.createTime).format('YYYY-MM-DD HH:mm:ss') }}</template>
        <template v-if="column.key === 'action'">{{ record.moduleCode }} / {{ record.actionCode }}</template>
      </template>
    </a-table>
  </div>
</template>

<script setup lang="ts">
import dayjs, { type Dayjs } from 'dayjs';
import { computed, onMounted, ref } from 'vue';
import { auditApi } from '../../api/system';
import PageToolbar from '../../components/PageToolbar.vue';
import type { AuditLogDto } from '../../types/system';

const loading = ref(false);
const items = ref<AuditLogDto[]>([]);
const keyword = ref('');
const dateRange = ref<[Dayjs, Dayjs]>();
const pageIndex = ref(1);
const pageSize = ref(20);
const total = ref(0);
const pagination = computed(() => ({ current: pageIndex.value, pageSize: pageSize.value, total: total.value, showSizeChanger: true }));
const columns = [
  { title: '时间', key: 'createTime', width: 180 },
  { title: '用户', dataIndex: 'userName', width: 140 },
  { title: '模块 / 操作', key: 'action', width: 210 },
  { title: '请求', dataIndex: 'requestPath', ellipsis: true },
  { title: '方法', dataIndex: 'httpMethod', width: 90 },
  { title: '业务ID', dataIndex: 'businessId', width: 180, ellipsis: true },
  { title: '结果', key: 'result', width: 90 },
  { title: 'IP', dataIndex: 'ipAddress', width: 120 }
];
const tableScroll = { x: 1190, y: 'calc(100vh - 350px)' };

onMounted(load);

async function load() {
  loading.value = true;
  try {
    const result = await auditApi.page({
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
      keyword: keyword.value || undefined,
      startTime: dateRange.value?.[0].toISOString(),
      endTime: dateRange.value?.[1].toISOString()
    });
    items.value = result.items;
    total.value = result.total;
  } finally {
    loading.value = false;
  }
}

function search() {
  pageIndex.value = 1;
  load();
}

function changePage(value: { current?: number; pageSize?: number }) {
  pageIndex.value = value.current ?? 1;
  pageSize.value = value.pageSize ?? 20;
  load();
}
</script>
