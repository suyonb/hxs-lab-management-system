<template>
  <div class="page-surface management-page instrument-business-page">
    <PageToolbar :eyebrow="config.eyebrow" :title="config.title">
      <a-button v-if="exportType&&hasPermission('lab:export')" @click="operationsApi.export(exportType)"><DownloadOutlined />导出</a-button>
      <a-button v-if="mode === 'approvals' || canCreate" type="primary" class="action-create" @click="openCreate">{{ config.createText }}</a-button>
    </PageToolbar>

    <a-table row-key="id" class="management-table" bordered :columns="columns" :data-source="rows" :loading="loading" :pagination="{ pageSize: 12 }" :scroll="{ x: 1100, y: 'calc(100vh - 330px)' }">
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'status'"><a-tag :color="statusColor[record.status] || 'default'">{{ statusText[record.status] || record.status }}</a-tag></template>
        <template v-else-if="column.key === 'active'"><a-tag :color="record.isActive ? 'green' : 'default'">{{ record.isActive ? '启用' : '停用' }}</a-tag></template>
        <template v-else-if="column.key === 'time'">{{ formatTime(record.startTime) }}<br><span class="table-secondary">至 {{ formatTime(record.endTime) }}</span></template>
        <template v-else-if="column.key === 'actions'">
          <a-space wrap>
            <a-button v-if="mode === 'instruments' && hasPermission('lab:instrument:manage')" size="small" class="action-edit" @click="openEdit(record)">编辑</a-button>
            <a-button v-if="mode === 'bookings' && hasPermission('lab:booking:cancel') && ['pending','approved'].includes(record.status)" size="small" class="action-delete" @click="act('cancelBooking', record.id)">取消</a-button>
            <template v-if="mode === 'approvals' && hasPermission('lab:booking:approve') && record.status === 'pending'">
              <a-button size="small" class="action-edit" @click="act('approveBooking', record.id)">通过</a-button><a-button size="small" class="action-delete" @click="act('rejectBooking', record.id)">驳回</a-button>
            </template>
            <template v-if="mode === 'repairs'">
              <a-button v-if="hasPermission('lab:repair:approve') && record.status === 'pending'" size="small" class="action-edit" @click="act('approveRepair',record.id)">通过</a-button>
              <a-button v-if="hasPermission('lab:repair:approve') && record.status === 'pending'" size="small" class="action-delete" @click="act('rejectRepair',record.id)">驳回</a-button>
              <a-button v-if="hasPermission('lab:repair:work') && record.status === 'approved'" size="small" class="action-edit" @click="openWork(record,'start')">开始维修</a-button>
              <a-button v-if="hasPermission('lab:repair:work') && record.status === 'repairing'" size="small" class="action-create" @click="openWork(record,'complete')">完成维修</a-button>
            </template>
          </a-space>
        </template>
      </template>
    </a-table>

    <a-modal v-model:open="editorOpen" width="720px" :ok-text="config.okText" @ok="save">
      <template #title><AppModalTitle :title="editing ? '编辑仪器' : config.createText" :subtitle="config.modalSubtitle" :icon="mode === 'repairs' ? 'maintenance' : 'experiment'" /></template>
      <a-form layout="vertical">
        <template v-if="mode === 'instruments'">
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="仪器编号"><a-input v-model:value="form.instrumentCode" :disabled="!!editing" /></a-form-item></a-col><a-col :span="12"><a-form-item label="仪器名称"><a-input v-model:value="form.instrumentName" /></a-form-item></a-col></a-row>
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="实验室"><a-select v-model:value="form.labId" :options="labOptions" @change="form.locationId=undefined" /></a-form-item></a-col><a-col :span="12"><a-form-item label="位置"><a-select v-model:value="form.locationId" :options="locationOptions" /></a-form-item></a-col></a-row>
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="仪器分类"><a-select v-model:value="form.categoryId" allow-clear :options="categoryOptions" /></a-form-item></a-col><a-col :span="12"><a-form-item label="供应商"><a-select v-model:value="form.supplierId" allow-clear :options="supplierOptions" /></a-form-item></a-col></a-row>
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="型号"><a-input v-model:value="form.model" /></a-form-item></a-col><a-col :span="12"><a-form-item label="制造商"><a-input v-model:value="form.manufacturer" /></a-form-item></a-col></a-row>
          <a-row :gutter="16"><a-col :span="12"><a-form-item label="运行状态"><a-select v-model:value="form.status" :options="instrumentStatusOptions" /></a-form-item></a-col><a-col :span="12"><a-form-item label="启用状态"><a-switch v-model:checked="form.isActive" checked-children="启用" un-checked-children="停用" /></a-form-item></a-col></a-row>
          <a-form-item label="说明"><a-textarea v-model:value="form.description" :rows="3" /></a-form-item>
        </template>
        <template v-else-if="mode === 'bookings'">
          <a-form-item label="仪器"><a-select v-model:value="form.instrumentId" show-search :options="instrumentOptions" /></a-form-item>
          <a-form-item label="预约时间"><a-range-picker v-model:value="form.period" show-time :minute-step="30" format="YYYY-MM-DD HH:mm" /></a-form-item>
          <a-form-item label="课题组"><a-select v-model:value="form.groupId" allow-clear :options="groupOptions" /></a-form-item>
          <a-form-item label="用途"><a-textarea v-model:value="form.purpose" :rows="3" /></a-form-item>
        </template>
        <template v-else-if="mode === 'usages'">
          <a-form-item label="仪器"><a-select v-model:value="form.instrumentId" :options="instrumentOptions" /></a-form-item>
          <a-form-item label="关联预约"><a-select v-model:value="form.bookingId" allow-clear :options="approvedBookingOptions" /></a-form-item>
          <a-form-item label="使用时间"><a-range-picker v-model:value="form.period" show-time :minute-step="30" format="YYYY-MM-DD HH:mm" /></a-form-item>
          <a-form-item label="实验内容"><a-textarea v-model:value="form.experimentContent" :rows="3" /></a-form-item>
          <a-form-item label="备注"><a-input v-model:value="form.remark" /></a-form-item>
        </template>
        <template v-else-if="mode === 'repairs'">
          <a-form-item label="故障仪器"><a-select v-model:value="form.instrumentId" :options="instrumentOptions" /></a-form-item>
          <a-form-item label="故障描述"><a-textarea v-model:value="form.faultDescription" :rows="5" /></a-form-item>
        </template>
      </a-form>
    </a-modal>

    <a-modal v-model:open="workOpen" :ok-text="workAction === 'start' ? '开始维修' : '完成维修'" @ok="saveWork">
      <template #title><AppModalTitle :title="workAction === 'start' ? '开始维修' : '完成维修'" subtitle="登记维修执行人员、处理内容和结果备注" icon="maintenance" tone="approval" /></template>
      <a-form layout="vertical"><a-form-item label="维修人员"><a-input v-model:value="workForm.repairer" /></a-form-item><a-form-item label="维修内容"><a-textarea v-model:value="workForm.repairContent" :rows="4" /></a-form-item><a-form-item label="备注"><a-input v-model:value="workForm.remark" /></a-form-item></a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { message } from 'ant-design-vue';
import { DownloadOutlined } from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import PageToolbar from '../../components/PageToolbar.vue';
import { instrumentApi } from '../../api/instrument';
import { labFoundationApi } from '../../api/lab';
import type { InstrumentDto } from '../../types/instrument';
import type { LocationDto } from '../../types/lab';
import { useAuthStore } from '../../stores/auth';
import { operationsApi } from '../../api/operations';

const props = defineProps<{mode:'instruments'|'bookings'|'approvals'|'usages'|'repairs'}>();
const authStore = useAuthStore();
const rows=ref<any[]>([]), instruments=ref<InstrumentDto[]>([]), labs=ref<any[]>([]), locations=ref<LocationDto[]>([]), suppliers=ref<any[]>([]), groups=ref<any[]>([]), categories=ref<any[]>([]), approvedBookings=ref<any[]>([]);
const loading=ref(false), editorOpen=ref(false), editing=ref<any>(null), workOpen=ref(false), workTarget=ref<any>(null), workAction=ref<'start'|'complete'>('start');
const form=reactive<any>({}), workForm=reactive({repairer:'',repairContent:'',remark:''});
const mode=computed(()=>props.mode);
const configs:any={instruments:{eyebrow:'Assets',title:'仪器台账',createText:'新增仪器',okText:'保存仪器',modalSubtitle:'录入仪器台账、实验室归属与运行状态'},bookings:{eyebrow:'Bookings',title:'我的仪器预约',createText:'申请预约',okText:'提交预约',modalSubtitle:'选择仪器与连续时间段并说明使用用途'},approvals:{eyebrow:'Approvals',title:'预约审批',createText:'刷新列表',okText:'保存',modalSubtitle:'核对预约申请与仪器可用时间'},usages:{eyebrow:'Usage',title:'仪器使用记录',createText:'登记使用',okText:'保存记录',modalSubtitle:'关联预约并登记实际使用时间与实验内容'},repairs:{eyebrow:'Repairs',title:'设备报修与维修',createText:'提交报修',okText:'提交报修',modalSubtitle:'选择故障仪器并完整描述异常情况'}};
const config=computed(()=>configs[mode.value]);
const exportType=computed(()=>mode.value==='instruments'?'instruments':mode.value==='bookings'||mode.value==='approvals'?'bookings':'');
const createPermissions:any={instruments:'lab:instrument:manage',bookings:'lab:booking:create',usages:'lab:usage:create',repairs:'lab:repair:create'};
const canCreate=computed(()=>hasPermission(createPermissions[mode.value]));
const statusText:any={normal:'正常',repair:'维修',stopped:'停用',pending:'待审核',approved:'已通过',rejected:'已驳回',cancelled:'已取消',completed:'已完成',repairing:'维修中'};
const statusColor:any={normal:'green',repair:'orange',stopped:'default',pending:'gold',approved:'blue',rejected:'red',cancelled:'default',completed:'green',repairing:'orange'};
const columnMap:any={instruments:[{title:'编号',dataIndex:'instrumentCode'},{title:'仪器名称',dataIndex:'instrumentName'},{title:'分类',dataIndex:'categoryName'},{title:'型号',dataIndex:'model'},{title:'实验室',dataIndex:'labName'},{title:'位置',dataIndex:'locationName'},{title:'状态',key:'status',width:90},{title:'启停',key:'active',width:80},{title:'操作',key:'actions',width:90}],bookings:[{title:'预约单号',dataIndex:'bookingNo'},{title:'仪器',dataIndex:'instrumentName'},{title:'预约时间',key:'time',width:190},{title:'用途',dataIndex:'purpose'},{title:'状态',key:'status',width:90},{title:'操作',key:'actions',width:90}],approvals:[{title:'预约单号',dataIndex:'bookingNo'},{title:'申请人',dataIndex:'applicantName'},{title:'仪器',dataIndex:'instrumentName'},{title:'预约时间',key:'time',width:190},{title:'用途',dataIndex:'purpose'},{title:'状态',key:'status',width:90},{title:'操作',key:'actions',width:140}],usages:[{title:'仪器',dataIndex:'instrumentName'},{title:'使用人',dataIndex:'userName'},{title:'使用时间',key:'time',width:190},{title:'实验内容',dataIndex:'experimentContent'},{title:'备注',dataIndex:'remark'}],repairs:[{title:'报修单号',dataIndex:'repairNo'},{title:'仪器',dataIndex:'instrumentName'},{title:'报修人',dataIndex:'reporterName'},{title:'故障描述',dataIndex:'faultDescription'},{title:'维修人员',dataIndex:'repairer'},{title:'状态',key:'status',width:90},{title:'操作',key:'actions',width:210}]};
const columns=computed(()=>columnMap[mode.value]);
const labOptions=computed(()=>labs.value.map(x=>({label:x.labName,value:x.id}))), supplierOptions=computed(()=>suppliers.value.map(x=>({label:x.supplierName,value:x.id}))), groupOptions=computed(()=>groups.value.map(x=>({label:x.groupName,value:x.id}))), categoryOptions=computed(()=>categories.value.map(x=>({label:x.itemLabel,value:x.id})));
const flatLocations=computed(()=>flatten(locations.value)); const locationOptions=computed(()=>flatLocations.value.filter(x=>!form.labId||x.labId===form.labId).map(x=>({label:x.locationName,value:x.id})));
const instrumentOptions=computed(()=>instruments.value.filter(x=>x.isActive&&x.status==='normal').map(x=>({label:`${x.instrumentCode} · ${x.instrumentName}`,value:x.id}))); const approvedBookingOptions=computed(()=>approvedBookings.value.map(x=>({label:`${x.bookingNo} · ${x.instrumentName}`,value:x.id})));
const instrumentStatusOptions=[{label:'正常',value:'normal'},{label:'维修',value:'repair'},{label:'停用',value:'stopped'}];
onMounted(init); watch(()=>props.mode,init);
async function init(){ loading.value=true; try { await loadRefs(); rows.value=mode.value==='instruments'?instruments.value:mode.value==='bookings'?await instrumentApi.bookings(true):mode.value==='approvals'?await instrumentApi.bookings(false):mode.value==='usages'?await instrumentApi.usages(false):await instrumentApi.repairs(false); } finally {loading.value=false;} }
async function loadRefs(){ const [ins,l,loc,s,g,types]=await Promise.all([instrumentApi.instruments(),labFoundationApi.labs(true),labFoundationApi.locations(),labFoundationApi.suppliers(true),labFoundationApi.groups(),labFoundationApi.dictTypes(true)]); instruments.value=ins;labs.value=l;locations.value=loc;suppliers.value=s;groups.value=g; const type=types.find(x=>x.dictCode==='instrument_category'); categories.value=type?await labFoundationApi.dictItems(type.id):[]; approvedBookings.value=await instrumentApi.bookings(true,'approved'); }
function reset(){Object.assign(form,{instrumentCode:'',instrumentName:'',categoryId:undefined,model:'',manufacturer:'',supplierId:undefined,labId:undefined,locationId:undefined,status:'normal',description:'',isActive:true,instrumentId:undefined,groupId:undefined,period:[],purpose:'',bookingId:undefined,experimentContent:'',remark:'',faultDescription:''});}
function openCreate(){if(mode.value==='approvals'){init();return;} editing.value=null;reset();editorOpen.value=true;}
function openEdit(row:any){editing.value=row;reset();Object.assign(form,row);editorOpen.value=true;}
async function save(){ if(mode.value==='instruments'){const data={...form}; editing.value?await instrumentApi.updateInstrument(editing.value.id,data):await instrumentApi.createInstrument(data);} else {const [start,end]=form.period||[]; const times={startTime:start?.second(0).millisecond(0).format('YYYY-MM-DDTHH:mm:ss'),endTime:end?.second(0).millisecond(0).format('YYYY-MM-DDTHH:mm:ss')}; if(mode.value==='bookings')await instrumentApi.createBooking({...form,...times}); if(mode.value==='usages')await instrumentApi.createUsage({...form,...times}); if(mode.value==='repairs')await instrumentApi.createRepair(form);} message.success('保存成功');editorOpen.value=false;await init();}
async function act(name:string,id:string){await (instrumentApi as any)[name](id);message.success('操作成功');await init();}
function openWork(row:any,action:'start'|'complete'){workTarget.value=row;workAction.value=action;Object.assign(workForm,{repairer:row.repairer||'',repairContent:row.repairContent||'',remark:row.remark||''});workOpen.value=true;}
async function saveWork(){const fn=workAction.value==='start'?'startRepair':'completeRepair';await (instrumentApi as any)[fn](workTarget.value.id,workForm);message.success('维修状态已更新');workOpen.value=false;await init();}
function flatten(items:LocationDto[]):LocationDto[]{return items.flatMap(x=>[x,...flatten(x.children||[])]);} function formatTime(v:string){return v?dayjs(v).format('YYYY-MM-DD HH:mm'):'-';}
function hasPermission(code?:string){return !!code&&authStore.hasPermission(code);}
</script>
