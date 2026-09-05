<template>
  <div class="lab-dashboard">
    <section class="lab-dashboard__head">
      <div><p class="eyebrow">Laboratory Overview</p><h1>实验室运行总览</h1><p>仪器状态、预约安排与设备维护信息集中呈现。</p></div>
      <div class="lab-dashboard__head-meta"><span>{{ todayText }}</span><a-tag :color="healthOk ? 'green' : 'orange'">{{ healthOk ? '服务正常' : '服务检查中' }}</a-tag></div>
    </section>

    <div class="lab-metric-grid">
      <article v-for="item in metrics" :key="item.label" class="lab-metric-card">
        <span class="lab-metric-card__icon" :style="{ color: item.color, background: item.background }"><component :is="item.icon" /></span>
        <div><span>{{ item.label }}</span><strong>{{ item.value }}</strong><small>{{ item.note }}</small></div>
      </article>
    </div>

    <div class="lab-dashboard__primary">
      <section class="lab-panel lab-panel--schedule">
        <header><div><span>Today</span><h2>今日仪器预约</h2></div><a-button type="link" @click="router.push('/lab/bookings')">查看全部</a-button></header>
        <div v-if="todayBookings.length" class="booking-timeline">
          <div v-for="item in todayBookings.slice(0, 6)" :key="item.id" class="booking-timeline__item">
            <time>{{ formatTime(item.startTime) }}</time><i :class="`is-${item.status}`"></i>
            <div><strong>{{ item.instrumentName }}</strong><span>{{ item.purpose }}</span></div>
            <a-tag :color="statusColor[item.status] || 'default'">{{ statusText[item.status] || item.status }}</a-tag>
          </div>
        </div>
        <a-empty v-else :image="simpleImage" description="今日暂无仪器预约" />
      </section>

      <section class="lab-panel lab-panel--status">
        <header><div><span>Equipment</span><h2>仪器运行状态</h2></div></header>
        <div class="instrument-status-chart">
          <div class="instrument-status-chart__ring" :style="ringStyle"><strong>{{ availableRate }}%</strong><span>可用率</span></div>
          <div class="instrument-status-chart__legend"><div v-for="item in instrumentStatus" :key="item.key"><i :style="{ background: item.color }"></i><span>{{ item.label }}</span><strong>{{ item.value }}</strong></div></div>
        </div>
      </section>
    </div>

    <div class="lab-dashboard__secondary">
      <section class="lab-panel">
        <header><div><span>Bookings</span><h2>预约状态分布</h2></div></header>
        <div class="lab-bar-list"><div v-for="item in bookingStatus" :key="item.key"><div><span>{{ item.label }}</span><strong>{{ item.value }}</strong></div><b><i :style="{ width: `${item.percent}%`, background: item.color }"></i></b></div></div>
      </section>
      <section class="lab-panel">
        <header><div><span>Resources</span><h2>实验室资源</h2></div></header>
        <div class="lab-resource-list"><div><span><ExperimentOutlined />实验室</span><strong>{{ counts.labs }}</strong></div><div><span><EnvironmentOutlined />位置节点</span><strong>{{ counts.locations }}</strong></div><div><span><TeamOutlined />课题组</span><strong>{{ counts.groups }}</strong></div><div><span><DatabaseOutlined />使用记录</span><strong>{{ counts.usages }}</strong></div></div>
      </section>
      <section class="lab-panel">
        <header><div><span>Maintenance</span><h2>最近报修</h2></div><a-button type="link" @click="router.push('/lab/repairs')">进入报修</a-button></header>
        <div v-if="recentRepairs.length" class="repair-list"><div v-for="item in recentRepairs.slice(0, 4)" :key="item.id"><span><strong>{{ item.instrumentName }}</strong><small>{{ item.faultDescription }}</small></span><a-tag :color="statusColor[item.status] || 'default'">{{ statusText[item.status] || item.status }}</a-tag></div></div>
        <a-empty v-else :image="simpleImage" description="暂无报修记录" />
      </section>
    </div>
    <div class="lab-dashboard__trends">
      <section class="lab-panel"><header><div><span>Usage Trend</span><h2>仪器使用趋势</h2></div><small>近 7 天</small></header><div class="mini-trend"><div v-for="item in operations.instrumentUsageTrend" :key="item.date"><b><i :style="{height:trendHeight(item.value,operations.instrumentUsageTrend)}"></i></b><strong>{{item.value}}</strong><span>{{item.date}}</span></div></div></section>
      <section class="lab-panel"><header><div><span>Consumption</span><h2>试剂消耗趋势</h2></div><small>近 7 天</small></header><div class="mini-trend mini-trend--green"><div v-for="item in operations.materialConsumptionTrend" :key="item.date"><b><i :style="{height:trendHeight(item.value,operations.materialConsumptionTrend)}"></i></b><strong>{{item.value}}</strong><span>{{item.date}}</span></div></div></section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { CalendarOutlined, CheckCircleOutlined, DatabaseOutlined, EnvironmentOutlined, ExperimentOutlined, TeamOutlined, ToolOutlined } from '@ant-design/icons-vue';
import { Empty } from 'ant-design-vue';
import dayjs from 'dayjs';
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { instrumentApi } from '../../api/instrument';
import { operationsApi } from '../../api/operations';
import { labFoundationApi } from '../../api/lab';
import { getHealth } from '../../api/system';
import type { BookingDto, InstrumentDto, RepairDto } from '../../types/instrument';
import type { LocationDto } from '../../types/lab';
import type { DashboardSummaryDto,TrendPointDto } from '../../types/operations';

const router=useRouter(), simpleImage=Empty.PRESENTED_IMAGE_SIMPLE, healthOk=ref(false);
const instruments=ref<InstrumentDto[]>([]), bookings=ref<BookingDto[]>([]), repairs=ref<RepairDto[]>([]);
const counts=reactive({labs:0,locations:0,groups:0,usages:0}), todayText=dayjs().format('YYYY年MM月DD日');
const operations=reactive<DashboardSummaryDto>({pendingCount:0,todayBookings:0,repairingInstruments:0,lowStockCount:0,expiringCount:0,expiredCount:0,recentExperimentCount:0,archivedExperimentCount:0,instrumentUsageTrend:[],materialConsumptionTrend:[]});
const statusText:Record<string,string>={normal:'正常',repair:'维修',stopped:'停用',pending:'待审核',approved:'已通过',rejected:'已驳回',cancelled:'已取消',completed:'已完成',repairing:'维修中'};
const statusColor:Record<string,string>={normal:'green',repair:'orange',stopped:'default',pending:'gold',approved:'blue',rejected:'red',cancelled:'default',completed:'green',repairing:'orange'};
const todayBookings=computed(()=>bookings.value.filter(x=>dayjs(x.startTime).isSame(dayjs(),'day')).sort((a,b)=>dayjs(a.startTime).valueOf()-dayjs(b.startTime).valueOf()));
const recentRepairs=computed(()=>[...repairs.value].sort((a,b)=>dayjs(b.createTime).valueOf()-dayjs(a.createTime).valueOf()));
const normalCount=computed(()=>instruments.value.filter(x=>x.isActive&&x.status==='normal').length), repairCount=computed(()=>instruments.value.filter(x=>x.status==='repair').length), stoppedCount=computed(()=>instruments.value.filter(x=>!x.isActive||x.status==='stopped').length);
const availableRate=computed(()=>instruments.value.length?Math.round(normalCount.value/instruments.value.length*100):0);
const metrics=computed(()=>[
  {label:'我的待办',value:operations.pendingCount,note:'预约、领用与报修',icon:CheckCircleOutlined,color:'#7c3aed',background:'rgba(124,58,237,.1)'},
  {label:'今日预约',value:todayBookings.value.length,note:`${todayBookings.value.filter(x=>x.status==='approved').length} 条已通过`,icon:CalendarOutlined,color:'#0f766e',background:'rgba(15,118,110,.1)'},
  {label:'维修中设备',value:repairCount.value,note:repairCount.value?'需要持续跟进':'设备运行稳定',icon:ToolOutlined,color:'#d97706',background:'rgba(217,119,6,.1)'},
  {label:'库存预警',value:operations.lowStockCount+operations.expiringCount+operations.expiredCount,note:`低库存 ${operations.lowStockCount} · 临期 ${operations.expiringCount} · 过期 ${operations.expiredCount}`,icon:DatabaseOutlined,color:'#dc2626',background:'rgba(220,38,38,.1)'}]);
const instrumentStatus=computed(()=>[{key:'normal',label:'正常可用',value:normalCount.value,color:'#16a34a'},{key:'repair',label:'维修中',value:repairCount.value,color:'#f59e0b'},{key:'stopped',label:'停用',value:stoppedCount.value,color:'#94a3b8'}]);
const ringStyle=computed(()=>{const total=Math.max(instruments.value.length,1),normal=normalCount.value/total*100,repair=repairCount.value/total*100;return {background:`conic-gradient(#16a34a 0 ${normal}%, #f59e0b ${normal}% ${normal+repair}%, #94a3b8 ${normal+repair}% 100%)`};});
const bookingStatus=computed(()=>{const defs=[['pending','待审核','#f59e0b'],['approved','已通过','#2563eb'],['completed','已完成','#16a34a'],['cancelled','已取消','#94a3b8']];const values=defs.map(([key,label,color])=>({key,label,color,value:bookings.value.filter(x=>x.status===key).length}));const max=Math.max(...values.map(x=>x.value),1);return values.map(x=>({...x,percent:x.value?Math.max(8,Math.round(x.value/max*100)):0}));});

onMounted(async()=>{const results=await Promise.allSettled([instrumentApi.instruments(false,true),instrumentApi.bookings(false,undefined,true),instrumentApi.repairs(false,undefined,true),instrumentApi.usages(false,true),labFoundationApi.labs(false,true),labFoundationApi.locations(undefined,false,true),labFoundationApi.groups(undefined,false,true),getHealth(true),operationsApi.dashboard(7,true)]);if(results[0].status==='fulfilled')instruments.value=results[0].value;if(results[1].status==='fulfilled')bookings.value=results[1].value;if(results[2].status==='fulfilled')repairs.value=results[2].value;if(results[3].status==='fulfilled')counts.usages=results[3].value.length;if(results[4].status==='fulfilled')counts.labs=results[4].value.length;if(results[5].status==='fulfilled')counts.locations=flattenLocations(results[5].value).length;if(results[6].status==='fulfilled')counts.groups=results[6].value.length;healthOk.value=results[7].status==='fulfilled';if(results[8].status==='fulfilled')Object.assign(operations,results[8].value);});
function flattenLocations(items:LocationDto[]):LocationDto[]{return items.flatMap(x=>[x,...flattenLocations(x.children||[])]);}function formatTime(value:string){return dayjs(value).format('HH:mm');}function trendHeight(value:number,items:TrendPointDto[]){const max=Math.max(...items.map(x=>x.value),1);return `${Math.max(value?12:3,Math.round(value/max*82))}%`;}
</script>
<style scoped>.lab-dashboard__trends{display:grid;grid-template-columns:1fr 1fr;gap:14px}.lab-panel header>small{color:var(--text-secondary);font-size:12px}.mini-trend{display:grid;grid-template-columns:repeat(7,1fr);gap:8px;height:190px;padding-top:14px}.mini-trend>div{display:grid;grid-template-rows:1fr 18px 20px;align-items:end;text-align:center;min-width:0}.mini-trend b{height:120px;display:flex;align-items:flex-end;justify-content:center}.mini-trend i{display:block;width:min(28px,70%);min-height:3px;border-radius:4px 4px 1px 1px;background:#2563eb}.mini-trend strong{font-size:12px}.mini-trend span{font-size:11px;color:var(--text-secondary)}.mini-trend--green i{background:#139454}@media(max-width:900px){.lab-dashboard__trends{grid-template-columns:1fr}}</style>
