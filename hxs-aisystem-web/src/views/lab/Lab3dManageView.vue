<template>
  <div class="page-surface management-page scene-manage-page">
    <PageToolbar eyebrow="Spatial" title="3D 场景管理">
      <a-button :loading="loading" @click="loadAll"><ReloadOutlined />刷新</a-button>
      <a-button type="primary" class="action-create" @click="openScene()"><PlusOutlined />新增场景</a-button>
    </PageToolbar>

    <a-alert v-if="loadIssues.length" class="load-alert" type="warning" show-icon closable @close="loadIssues=[]">
      <template #message>部分信息暂未加载</template>
      <template #description>{{ loadIssues.join('、') }}，已保留其余可用数据。</template>
      <template #action><a-button size="small" @click="loadAll">重新加载</a-button></template>
    </a-alert>

    <div class="scene-overview">
      <div><span class="overview-icon is-scene"><ApartmentOutlined /></span><p><span>场景总数</span><strong>{{ scenes.length }}</strong></p></div>
      <div><span class="overview-icon is-node"><NodeIndexOutlined /></span><p><span>空间节点</span><strong>{{ totalNodes }}</strong></p></div>
      <div><span class="overview-icon is-binding"><LinkOutlined /></span><p><span>当前绑定</span><strong>{{ bindingCount }}</strong></p></div>
      <div><span class="overview-icon is-model"><FileOutlined /></span><p><span>模型版本</span><strong>{{ versions.length }}</strong></p></div>
    </div>

    <div class="scene-workbench">
      <aside class="scene-index">
        <div class="panel-heading"><div><strong>场景目录</strong><span>按最近维护时间排列</span></div><a-tag color="blue">{{ scenes.length }}</a-tag></div>
        <div v-if="scenes.length" class="scene-list">
          <button v-for="item in scenes" :key="item.scene.id" type="button" class="scene-item" :class="{active:item.scene.id===selectedId}" @click="selectScene(item.scene.id)">
            <span class="scene-color" :style="{background:item.scene.backgroundColor}" />
            <span class="scene-item__body"><b>{{ item.scene.sceneName }}</b><small>{{ item.scene.labName }}</small></span>
            <span class="scene-item__meta"><a-tag :color="item.scene.isActive?'green':'default'">{{ item.scene.isActive?'启用':'停用' }}</a-tag><small>{{ item.nodeCount }} 节点</small></span>
          </button>
        </div>
        <div v-else class="panel-empty"><a-empty :image="simpleImage" :description="loadIssues.length?'场景目录暂不可用':'尚未创建三维场景'" /><a-button v-if="!loadIssues.length" type="primary" @click="openScene()"><PlusOutlined />创建场景</a-button></div>
      </aside>

      <section class="scene-editor">
        <a-spin :spinning="loading">
          <template v-if="detail">
            <header class="scene-editor__head">
              <div class="scene-title"><span class="scene-color scene-color--large" :style="{background:detail.scene.backgroundColor}" /><div><strong>{{ detail.scene.sceneName }}</strong><span>{{ detail.scene.labName }} · 模型 v{{ detail.scene.version }}</span></div></div>
              <a-space wrap>
                <a-upload :show-upload-list="false" accept=".glb" :before-upload="uploadModel"><a-button><UploadOutlined />上传 GLB</a-button></a-upload>
                <a-button class="action-edit" @click="openScene(selected)"><EditOutlined />编辑</a-button>
                <a-button type="primary" class="action-create" @click="openNode()"><PlusOutlined />新增节点</a-button>
                <a-popconfirm title="删除场景后节点和绑定也会被删除，确认继续？" @confirm="removeScene"><a-button danger class="action-delete"><DeleteOutlined /></a-button></a-popconfirm>
              </a-space>
            </header>

            <div class="scene-summary">
              <div><span>模型状态</span><strong>{{ detail.scene.modelFileId?'已上传':'使用程序化空间' }}</strong></div>
              <div><span>节点数量</span><strong>{{ detail.nodes.length }}</strong></div>
              <div><span>业务绑定</span><strong>{{ detail.nodes.filter(x=>x.businessId).length }}</strong></div>
              <div><span>场景状态</span><strong>{{ detail.scene.isActive?'对外可见':'已停用' }}</strong></div>
            </div>

            <div class="table-heading"><div><strong>空间节点</strong><span>已绑定 {{ bindingCount }} 个业务对象</span></div><a-button size="small" class="action-create" @click="openNode()"><PlusOutlined />新增节点</a-button></div>
            <a-table v-if="detail.nodes.length" row-key="id" class="management-table node-table" bordered size="small" :columns="columns" :data-source="detail.nodes" :pagination="false" :scroll="{x:1050,y:360}">
              <template #bodyCell="{column,record}">
                <template v-if="column.key==='type'"><a-tag>{{ typeName(record.type) }}</a-tag></template>
                <template v-else-if="column.key==='position'">{{ vector(record.x,record.y,record.z) }}</template>
                <template v-else-if="column.key==='scale'">{{ vector(record.scaleX,record.scaleY,record.scaleZ) }}</template>
                <template v-else-if="column.key==='binding'"><span v-if="record.businessId" class="binding-value"><LinkOutlined />{{ record.detail||record.businessId }}</span><span v-else class="muted">未绑定</span></template>
                <a-space v-else-if="column.key==='actions'"><a-button size="small" class="action-edit" @click="openNode(record)">编辑</a-button><a-popconfirm title="确认删除该节点？" @confirm="removeNode(record.id)"><a-button size="small" danger class="action-delete">删除</a-button></a-popconfirm></a-space>
              </template>
            </a-table>
            <div v-else class="node-empty"><NodeIndexOutlined /><strong>当前场景还没有空间节点</strong><span>可添加实验室、位置或仪器节点，并绑定已有基础数据。</span><a-button type="primary" @click="openNode()"><PlusOutlined />添加第一个节点</a-button></div>

            <div class="version-section">
              <div class="table-heading"><div><strong>模型版本</strong><span>上传新模型后自动切换并递增版本</span></div></div>
              <div v-if="versions.length" class="version-list">
                <div v-for="version in versions" :key="version.fileId" class="version-row"><FileOutlined /><span><b>{{ version.fileName }}</b><small>{{ fileSize(version.fileSize) }} · {{ date(version.createTime) }}</small></span><a-tag v-if="version.isCurrent" color="blue">当前版本</a-tag><a-button v-else size="small" @click="activateModel(version.fileId)">设为当前</a-button></div>
              </div>
              <div v-else class="model-empty"><FileOutlined /><span><strong>暂无 GLB 模型</strong><small>当前使用程序化空间，上传模型后会保留历史版本。</small></span><a-upload :show-upload-list="false" accept=".glb" :before-upload="uploadModel"><a-button><UploadOutlined />上传模型</a-button></a-upload></div>
            </div>
          </template>
          <div v-else class="editor-empty"><ApartmentOutlined /><strong>{{ scenes.length?'请选择一个场景':'等待场景数据' }}</strong><span>{{ scenes.length?'选择后可维护空间节点、业务绑定和模型版本。':'创建场景后即可开始空间配置。' }}</span></div>
        </a-spin>
      </section>
    </div>

    <a-modal v-model:open="sceneOpen" width="600px" ok-text="保存场景" @ok="saveScene">
      <template #title><AppModalTitle :title="editingScene ? '编辑场景' : '新增场景'" subtitle="配置实验室空间场景及默认展示背景" icon="spatial" tone="spatial" /></template>
      <a-form layout="vertical" :model="sceneForm">
        <a-form-item label="所属实验室" required><a-select v-model:value="sceneForm.labId" :options="labOptions" /></a-form-item>
        <a-form-item label="场景名称" required><a-input v-model:value="sceneForm.sceneName" :maxlength="100" /></a-form-item>
        <a-row :gutter="16"><a-col :span="12"><a-form-item label="空间背景色"><a-input v-model:value="sceneForm.backgroundColor" type="color" class="color-input" /></a-form-item></a-col><a-col :span="12"><a-form-item label="场景状态"><a-switch v-model:checked="sceneForm.isActive" checked-children="启用" un-checked-children="停用" /></a-form-item></a-col></a-row>
      </a-form>
    </a-modal>

    <a-modal v-model:open="nodeOpen" width="760px" ok-text="保存节点" @ok="saveNode">
      <template #title><AppModalTitle :title="editingNode ? '编辑空间节点' : '新增空间节点'" subtitle="设置节点坐标、空间尺寸并绑定现有业务数据" icon="spatial" tone="spatial" /></template>
      <a-form layout="vertical" :model="nodeForm">
        <a-row :gutter="16"><a-col :span="10"><a-form-item label="节点编码" required><a-input v-model:value="nodeForm.code" /></a-form-item></a-col><a-col :span="10"><a-form-item label="节点名称" required><a-input v-model:value="nodeForm.name" /></a-form-item></a-col><a-col :span="4"><a-form-item label="排序"><a-input-number v-model:value="nodeForm.sortNo" :min="0" /></a-form-item></a-col></a-row>
        <a-form-item label="节点类型"><a-segmented v-model:value="nodeForm.type" :options="nodeTypes" /></a-form-item>
        <div class="coordinate-grid"><a-form-item v-for="key in coordinateKeys" :key="key.value" :label="key.label"><a-input-number v-model:value="nodeForm[key.value]" :step="0.1" /></a-form-item></div>
        <div class="binding-editor"><a-form-item label="业务绑定类型"><a-select v-model:value="nodeForm.businessType" allow-clear :options="bindingTypes" @change="nodeForm.businessId=undefined" /></a-form-item><a-form-item label="绑定业务数据"><a-select v-model:value="nodeForm.businessId" allow-clear show-search option-filter-prop="label" :disabled="!nodeForm.businessType" :options="bindingOptions" /></a-form-item></div>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import {ApartmentOutlined,DeleteOutlined,EditOutlined,FileOutlined,LinkOutlined,NodeIndexOutlined,PlusOutlined,ReloadOutlined,UploadOutlined} from '@ant-design/icons-vue';
import {Empty,message} from 'ant-design-vue';
import {computed,onMounted,reactive,ref} from 'vue';
import PageToolbar from '../../components/PageToolbar.vue';
import {visualizationApi as api} from '../../api/visualization';
import {labFoundationApi} from '../../api/lab';
import {instrumentApi} from '../../api/instrument';
import type {InstrumentDto} from '../../types/instrument';
import type {LabDto,LocationDto} from '../../types/lab';
import type {Lab3dModelVersion,Lab3dNode,Lab3dSceneDetail,Lab3dSceneManage} from '../../types/visualization';

const simpleImage=Empty.PRESENTED_IMAGE_SIMPLE;const loading=ref(false);const loadIssues=ref<string[]>([]);const scenes=ref<Lab3dSceneManage[]>([]);const selectedId=ref('');const detail=ref<Lab3dSceneDetail>();const versions=ref<Lab3dModelVersion[]>([]);const labs=ref<LabDto[]>([]);const locations=ref<LocationDto[]>([]);const instruments=ref<InstrumentDto[]>([]);
const sceneOpen=ref(false);const editingScene=ref<Lab3dSceneManage>();const nodeOpen=ref(false);const editingNode=ref<Lab3dNode>();
const sceneForm=reactive({labId:'',sceneName:'',backgroundColor:'#eef3f5',isActive:true});
const nodeForm=reactive<any>({code:'',name:'',type:'instrument',x:0,y:0,z:0,scaleX:1,scaleY:1,scaleZ:1,sortNo:0,businessType:undefined,businessId:undefined});
const selected=computed(()=>scenes.value.find(x=>x.scene.id===selectedId.value));const labOptions=computed(()=>labs.value.map(x=>({label:`${x.labCode} · ${x.labName}`,value:x.id})));const totalNodes=computed(()=>scenes.value.reduce((sum,x)=>sum+x.nodeCount,0));const bindingCount=computed(()=>detail.value?.nodes.filter(x=>x.businessId).length||0);
const nodeTypes=[{label:'实验室',value:'lab'},{label:'位置',value:'location'},{label:'仪器',value:'instrument'}];const bindingTypes=[...nodeTypes];
const coordinateKeys=[{label:'位置 X',value:'x'},{label:'位置 Y',value:'y'},{label:'位置 Z',value:'z'},{label:'缩放 X',value:'scaleX'},{label:'缩放 Y',value:'scaleY'},{label:'缩放 Z',value:'scaleZ'}];
const columns=[{title:'编码',dataIndex:'code',width:120},{title:'节点名称',dataIndex:'name',width:160},{title:'类型',key:'type',width:90},{title:'位置 X/Y/Z',key:'position',width:150},{title:'缩放 X/Y/Z',key:'scale',width:150},{title:'业务绑定',key:'binding',width:230},{title:'操作',key:'actions',width:140,fixed:'right' as const}];
const flatLocations=computed(()=>flatten(locations.value));
const bindingOptions=computed(()=>{const labId=detail.value?.scene.labId;if(nodeForm.businessType==='lab')return labs.value.filter(x=>x.id===labId).map(x=>({label:x.labName,value:x.id}));if(nodeForm.businessType==='location')return flatLocations.value.filter(x=>x.labId===labId).map(x=>({label:`${x.locationCode} · ${x.locationName}`,value:x.id}));if(nodeForm.businessType==='instrument')return instruments.value.filter(x=>x.labId===labId).map(x=>({label:`${x.instrumentCode} · ${x.instrumentName}`,value:x.id}));return[];});

onMounted(loadAll);
async function loadAll(){loading.value=true;loadIssues.value=[];try{const keep=selectedId.value;const results=await Promise.allSettled([api.manageScenes(true),labFoundationApi.labs(true,true),labFoundationApi.locations(undefined,true,true),instrumentApi.instruments(false,true)]);const labels=['场景目录','实验室档案','位置数据','仪器数据'];results.forEach((result,index)=>{if(result.status==='rejected')loadIssues.value.push(labels[index]);});if(results[0].status==='fulfilled')scenes.value=results[0].value;if(results[1].status==='fulfilled')labs.value=results[1].value;if(results[2].status==='fulfilled')locations.value=results[2].value;if(results[3].status==='fulfilled')instruments.value=results[3].value;const next=scenes.value.some(x=>x.scene.id===keep)?keep:scenes.value[0]?.scene.id||'';if(next)await selectScene(next);else{selectedId.value='';detail.value=undefined;versions.value=[];}}finally{loading.value=false;}}
async function selectScene(id:string){selectedId.value=id;loadIssues.value=loadIssues.value.filter(x=>x!=='场景详情'&&x!=='模型版本');const results=await Promise.allSettled([api.scene(id,true),api.modelVersions(id,true)]);if(results[0].status==='fulfilled')detail.value=results[0].value;else{detail.value=undefined;loadIssues.value.push('场景详情');}if(results[1].status==='fulfilled')versions.value=results[1].value;else{versions.value=[];loadIssues.value.push('模型版本');}}
function openScene(row?:Lab3dSceneManage){editingScene.value=row;Object.assign(sceneForm,{labId:row?.scene.labId||labs.value[0]?.id||'',sceneName:row?.scene.sceneName||'',backgroundColor:row?.scene.backgroundColor||'#eef3f5',isActive:row?.scene.isActive??true});sceneOpen.value=true;}
async function saveScene(){if(!sceneForm.labId||!sceneForm.sceneName.trim()){message.warning('请填写实验室和场景名称');return;}const saved=editingScene.value?await api.updateScene(editingScene.value.scene.id,{...sceneForm}):await api.createScene({...sceneForm});sceneOpen.value=false;selectedId.value=saved.scene.id;message.success('场景已保存');await loadAll();}
async function removeScene(){if(!selectedId.value)return;await api.removeScene(selectedId.value);message.success('场景已删除');selectedId.value='';await loadAll();}
async function uploadModel(file:File){if(!file.name.toLowerCase().endsWith('.glb')){message.warning('请选择 GLB 模型文件');return false;}await api.uploadModel(selectedId.value,file);message.success('模型已上传并切换到新版本');await loadAll();return false;}
async function activateModel(fileId:string){await api.activateModel(selectedId.value,fileId);message.success('已切换模型版本');await loadAll();}
function openNode(value?:Record<string,any>){const row=value as Lab3dNode|undefined;editingNode.value=row;Object.assign(nodeForm,{code:row?.code||'',name:row?.name||'',type:row?.type||'instrument',x:row?.x||0,y:row?.y||0,z:row?.z||0,scaleX:row?.scaleX||1,scaleY:row?.scaleY||1,scaleZ:row?.scaleZ||1,sortNo:0,businessType:row?.businessType||undefined,businessId:row?.businessId||undefined});nodeOpen.value=true;}
async function saveNode(){if(!nodeForm.code.trim()||!nodeForm.name.trim()){message.warning('请填写节点编码和名称');return;}const payload={code:nodeForm.code,name:nodeForm.name,type:nodeForm.type,x:nodeForm.x,y:nodeForm.y,z:nodeForm.z,scaleX:nodeForm.scaleX,scaleY:nodeForm.scaleY,scaleZ:nodeForm.scaleZ,sortNo:nodeForm.sortNo};const node=editingNode.value?await api.updateNode(editingNode.value.id,payload):await api.createNode(selectedId.value,payload);if(nodeForm.businessType&&nodeForm.businessId)await api.setBinding(node.id,{businessType:nodeForm.businessType,businessId:nodeForm.businessId});else if(editingNode.value?.businessId)await api.removeBinding(node.id);nodeOpen.value=false;message.success('节点已保存');await selectScene(selectedId.value);await refreshSceneCount();}
async function removeNode(id:string){await api.removeNode(id);message.success('节点已删除');await selectScene(selectedId.value);await refreshSceneCount();}
async function refreshSceneCount(){scenes.value=await api.manageScenes();}
function flatten(rows:LocationDto[]):LocationDto[]{return rows.flatMap(x=>[x,...flatten(x.children||[])]);}function vector(x:number,y:number,z:number){return [x,y,z].map(v=>Number(v).toFixed(1)).join(' / ');}function typeName(type:string){return nodeTypes.find(x=>x.value===type)?.label||type;}function date(value:string){return new Date(value).toLocaleString('zh-CN');}function fileSize(value:number){return value<1024*1024?`${(value/1024).toFixed(1)} KB`:`${(value/1024/1024).toFixed(1)} MB`;}
</script>

<style scoped>
.scene-manage-page{overflow:auto}.load-alert{margin-bottom:14px}.scene-overview{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:12px;margin-bottom:14px}.scene-overview>div{display:flex;align-items:center;gap:12px;min-width:0;padding:14px 16px;border:1px solid var(--border-color);border-radius:8px;background:var(--surface-color);box-shadow:0 5px 18px rgba(15,23,42,.04)}.scene-overview p{display:flex;flex-direction:column;gap:2px;margin:0}.scene-overview p span{color:var(--text-secondary);font-size:12px}.scene-overview strong{font-size:21px;line-height:1.1}.overview-icon{display:grid;place-items:center;width:36px;height:36px;flex:0 0 36px;border-radius:7px;font-size:17px}.overview-icon.is-scene{color:#2563eb;background:rgba(37,99,235,.1)}.overview-icon.is-node{color:#0f766e;background:rgba(15,118,110,.1)}.overview-icon.is-binding{color:#7c3aed;background:rgba(124,58,237,.1)}.overview-icon.is-model{color:#d97706;background:rgba(217,119,6,.1)}
.scene-workbench{display:grid;grid-template-columns:300px minmax(0,1fr);gap:14px;min-height:610px;flex:1}.scene-index,.scene-editor{border:1px solid var(--border-color);background:var(--surface-color);border-radius:8px;min-height:0;box-shadow:0 6px 22px rgba(15,23,42,.04)}.scene-index{display:flex;flex-direction:column;overflow:hidden}.panel-heading,.table-heading{display:flex;align-items:center;justify-content:space-between;gap:12px;padding:14px 16px;border-bottom:1px solid var(--border-color)}.panel-heading div,.table-heading div{display:flex;flex-direction:column;gap:3px}.panel-heading span,.table-heading span,.scene-title span{font-size:12px;color:var(--text-secondary)}.scene-list{padding:8px;overflow:auto}.scene-item{width:100%;display:grid;grid-template-columns:12px minmax(0,1fr) auto;gap:10px;align-items:center;padding:12px 10px;border:1px solid transparent;border-radius:6px;background:transparent;color:inherit;text-align:left;cursor:pointer;transition:background .18s,border-color .18s}.scene-item:hover{background:var(--hover-color)}.scene-item.active{border-color:color-mix(in srgb,var(--primary-color) 65%,var(--border-color));background:color-mix(in srgb,var(--primary-color) 8%,var(--surface-color))}.scene-color{width:10px;height:42px;border-radius:3px;border:1px solid rgba(0,0,0,.12)}.scene-color--large{width:12px;height:46px}.scene-item__body,.scene-item__meta{display:flex;flex-direction:column;gap:4px;min-width:0}.scene-item__body b,.scene-item__body small{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.scene-item__body small,.scene-item__meta small{color:var(--text-secondary);font-size:12px}.scene-item__meta{align-items:flex-end}.panel-empty{display:flex;flex:1;flex-direction:column;align-items:center;justify-content:center;padding:30px 16px}.panel-empty :deep(.ant-empty){margin:0 0 14px}
.scene-editor{overflow:auto}.scene-editor :deep(.ant-spin-nested-loading),.scene-editor :deep(.ant-spin-container){min-height:100%}.scene-editor__head{display:flex;justify-content:space-between;align-items:center;gap:16px;padding:16px}.scene-title{display:flex;align-items:center;gap:12px;min-width:0}.scene-title div{display:flex;flex-direction:column;gap:4px;min-width:0}.scene-title strong{font-size:17px}.scene-summary{display:grid;grid-template-columns:repeat(4,1fr);border-block:1px solid var(--border-color);background:color-mix(in srgb,var(--surface-color) 94%,var(--primary-color))}.scene-summary div{display:flex;flex-direction:column;gap:5px;padding:13px 16px;border-right:1px solid var(--border-color)}.scene-summary div:last-child{border-right:0}.scene-summary span{font-size:12px;color:var(--text-secondary)}.scene-summary strong{font-size:15px}.node-table{margin:0 16px 16px}.binding-value{display:inline-flex;align-items:center;gap:6px}.muted{color:var(--text-secondary)}.node-empty,.editor-empty{display:flex;flex-direction:column;align-items:center;justify-content:center;gap:8px;color:var(--text-secondary);text-align:center}.node-empty{min-height:230px;margin:0 16px 16px;border:1px dashed var(--border-color);border-radius:7px;background:color-mix(in srgb,var(--surface-color) 96%,var(--primary-color))}.node-empty>svg,.editor-empty>svg{font-size:28px;color:var(--primary-color)}.node-empty strong,.editor-empty strong{color:var(--text-color);font-size:15px}.node-empty .ant-btn{margin-top:8px}.editor-empty{min-height:600px}
.version-section{border-top:1px solid var(--border-color)}.version-list{padding:6px 16px 16px}.version-row{display:grid;grid-template-columns:20px minmax(0,1fr) auto;gap:10px;align-items:center;padding:10px 0;border-bottom:1px solid var(--border-color)}.version-row span,.model-empty>span{display:flex;flex-direction:column}.version-row small,.model-empty small{color:var(--text-secondary)}.model-empty{display:grid;grid-template-columns:24px minmax(0,1fr) auto;gap:12px;align-items:center;margin:12px 16px 16px;padding:14px;border:1px dashed var(--border-color);border-radius:7px;background:color-mix(in srgb,var(--surface-color) 96%,var(--primary-color))}.model-empty>svg{font-size:20px;color:#d97706}.color-input{height:34px;padding:3px}.coordinate-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:0 14px}.coordinate-grid :deep(.ant-input-number){width:100%}.binding-editor{display:grid;grid-template-columns:1fr 2fr;gap:14px;padding-top:12px;border-top:1px solid var(--border-color)}
@media(max-width:1100px){.scene-overview{grid-template-columns:repeat(2,1fr)}.scene-workbench{grid-template-columns:270px minmax(0,1fr)}}@media(max-width:900px){.scene-workbench{grid-template-columns:1fr}.scene-index{max-height:280px}.scene-summary{grid-template-columns:repeat(2,1fr)}.scene-editor__head{align-items:flex-start;flex-direction:column}}@media(max-width:620px){.scene-overview,.scene-summary,.coordinate-grid,.binding-editor{grid-template-columns:1fr}.scene-summary div{border-right:0;border-bottom:1px solid var(--border-color)}.model-empty{grid-template-columns:24px 1fr}.model-empty .ant-upload-wrapper{grid-column:1/-1}.scene-editor__head :deep(.ant-space){width:100%}}
</style>
