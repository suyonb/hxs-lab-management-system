import type { AxiosAdapter, AxiosResponse } from 'axios';
import type { MenuDto, OrgDto } from '../types/system';
import type { LocationDto } from '../types/lab';
import type { DemoState } from './seed';
import { createDemoState } from './seed';

const storageKey = 'hxs-demo-state-v1';
let memoryState: DemoState | undefined;

export const demoAdapter: AxiosAdapter = async (config) => {
  await new Promise((resolve) => window.setTimeout(resolve, 80 + Math.random() * 120));
  const method = (config.method || 'get').toUpperCase();
  const path = new URL(config.url || '/', window.location.origin).pathname;
  const params = (config.params || {}) as Record<string, unknown>;
  const body = parseBody(config.data);
  const data = route(getState(), method, path, params, body);
  const response: AxiosResponse = { data, status: 200, statusText: 'OK', headers: {}, config };
  return response;
};

export function resetDemoState() {
  memoryState = createDemoState();
  localStorage.setItem(storageKey, JSON.stringify(memoryState));
}

function getState() {
  if (memoryState) return memoryState;
  const raw = localStorage.getItem(storageKey);
  if (raw) {
    try {
      const stored = JSON.parse(raw) as DemoState;
      if (stored.version === 1) return (memoryState = stored);
    } catch {
      localStorage.removeItem(storageKey);
    }
  }
  resetDemoState();
  return memoryState!;
}

function save(state: DemoState) {
  memoryState = state;
  localStorage.setItem(storageKey, JSON.stringify(state));
}

function route(state: DemoState, method: string, path: string, params: Record<string, unknown>, body: any): any {
  if (path === '/health') return { status: 'Healthy', mode: 'demo' };
  if (path === '/api/auth/login' && method === 'POST') return loginResponse();
  if (path === '/api/auth/menus') return tree(state.menus.filter((item) => item.isActive));
  if (path === '/api/auth/permissions') return [...new Set(state.menus.map((item) => item.permissionCode).filter(Boolean))];

  const systemResult = routeSystem(state, method, path, params, body);
  if (systemResult.handled) return systemResult.data;
  const foundationResult = routeFoundation(state, method, path, params, body);
  if (foundationResult.handled) return foundationResult.data;
  const instrumentResult = routeInstrument(state, method, path, params, body);
  if (instrumentResult.handled) return instrumentResult.data;
  const inventoryResult = routeInventory(state, method, path, params, body);
  if (inventoryResult.handled) return inventoryResult.data;
  const experimentResult = routeExperiment(state, method, path, body);
  if (experimentResult.handled) return experimentResult.data;
  const operationsResult = routeOperations(state, method, path, params);
  if (operationsResult.handled) return operationsResult.data;
  const aiResult = routeAi(state, method, path, body);
  if (aiResult.handled) return aiResult.data;
  const visualizationResult = routeVisualization(state, method, path, body);
  if (visualizationResult.handled) return visualizationResult.data;
  if (path.startsWith('/api/files/') && method === 'GET') return new Blob(['HXS 实验室管理系统演示附件'], { type: 'text/plain' });
  throw new Error(`演示模式暂未覆盖接口：${method} ${path}`);
}

function routeSystem(state: DemoState, method: string, path: string, params: Record<string, unknown>, body: any) {
  if (path === '/api/system/users' && method === 'GET') {
    const keyword = String(params.keyword || '').toLowerCase();
    return hit(state.users.filter((item) => !keyword || `${item.userName}${item.displayName || ''}`.toLowerCase().includes(keyword)));
  }
  if (path === '/api/system/users' && method === 'POST') {
    const item = { ...body, id: id('user'), lastLoginTime: null };
    state.users.unshift(item); save(state); return hit(item);
  }
  let match = path.match(/^\/api\/system\/users\/([^/]+)(?:\/(roles))?$/);
  if (match) {
    const [, userId, child] = match;
    if (child === 'roles' && method === 'GET') return hit(state.roles.filter((item) => (state.userRoles[userId] || []).includes(item.id)));
    if (child === 'roles' && method === 'PUT') { state.userRoles[userId] = body.roleIds || []; save(state); return hit({}); }
    if (method === 'PUT') return hit(update(state.users, userId, body, state));
    if (method === 'DELETE') return hit(remove(state.users, userId, state));
  }

  if (path === '/api/system/roles' && method === 'GET') return hit(state.roles);
  if (path === '/api/system/roles' && method === 'POST') { const item = { ...body, id: id('role') }; state.roles.push(item); save(state); return hit(item); }
  match = path.match(/^\/api\/system\/roles\/([^/]+)(?:\/(menus))?$/);
  if (match) {
    const [, roleId, child] = match;
    if (child === 'menus' && method === 'GET') return hit(state.menus.filter((item) => (state.roleMenus[roleId] || []).includes(item.id)));
    if (child === 'menus' && method === 'PUT') { state.roleMenus[roleId] = body.menuIds || []; save(state); return hit({}); }
    if (method === 'PUT') return hit(update(state.roles, roleId, body, state));
    if (method === 'DELETE') return hit(remove(state.roles, roleId, state));
  }

  if ((path === '/api/system/orgs' || path === '/api/system/orgs/tree') && method === 'GET') return hit(path.endsWith('/tree') ? tree(state.orgs) : state.orgs);
  if (path === '/api/system/orgs' && method === 'POST') { const item = { ...body, id: id('org') }; state.orgs.push(item); save(state); return hit(item); }
  match = path.match(/^\/api\/system\/orgs\/([^/]+)$/);
  if (match && method === 'PUT') return hit(update(state.orgs, match[1], body, state));
  if (match && method === 'DELETE') return hit(remove(state.orgs, match[1], state));

  if (path === '/api/system/menus' && method === 'GET') return hit(state.menus);
  if (path === '/api/system/menus/tree' && method === 'GET') return hit(tree(state.menus));
  if (path === '/api/system/menus' && method === 'POST') { const item = { ...body, id: id('menu') }; state.menus.push(item); save(state); return hit(item); }
  match = path.match(/^\/api\/system\/menus\/([^/]+)$/);
  if (match && method === 'PUT') return hit(update(state.menus, match[1], body, state));
  if (match && method === 'DELETE') return hit(remove(state.menus, match[1], state));

  if (path === '/api/system/audit-logs' && method === 'GET') {
    const pageIndex = Number(params.pageIndex || 1), pageSize = Number(params.pageSize || 20);
    const keyword = String(params.keyword || '').toLowerCase();
    const rows = state.auditLogs.filter((item) => !keyword || JSON.stringify(item).toLowerCase().includes(keyword));
    return hit({ items: rows.slice((pageIndex - 1) * pageSize, pageIndex * pageSize), pageIndex, pageSize, total: rows.length });
  }
  return miss();
}

function routeFoundation(state: DemoState, method: string, path: string, params: Record<string, unknown>, body: any) {
  const base = '/api/lab/foundation';
  const resources: Record<string, { key: keyof DemoState; names: [string, string]; extra?: (value: any) => any }> = {
    labs: { key: 'labs', names: ['labCode', 'labName'], extra: (value) => ({ ...value, managerName: state.users.find((x) => x.id === value.managerId)?.displayName }) },
    locations: { key: 'locations', names: ['locationCode', 'locationName'] },
    groups: { key: 'groups', names: ['groupCode', 'groupName'], extra: (value) => ({ ...value, leaderName: state.users.find((x) => x.id === value.leaderId)?.displayName }) },
    suppliers: { key: 'suppliers', names: ['supplierCode', 'supplierName'] },
    'dict-types': { key: 'dictTypes', names: ['dictCode', 'dictName'] }
  };
  for (const [resource, definition] of Object.entries(resources)) {
    if (path === `${base}/${resource}` && method === 'GET') {
      let values = [...(state[definition.key] as any[])];
      if (resource === 'locations' && params.labId) values = values.filter((item) => item.labId === params.labId);
      if (params.enabledOnly === true || params.enabledOnly === 'true') values = values.filter((item) => item.isActive);
      return hit(resource === 'locations' ? tree(values) : values);
    }
    if (path === `${base}/${resource}` && method === 'POST') {
      const item = definition.extra?.({ ...body, id: id(resource) }) || { ...body, id: id(resource) };
      (state[definition.key] as any[]).push(item); save(state); return hit(item);
    }
    const match = path.match(new RegExp(`^${base}/${resource}/([^/]+)$`));
    if (match && method === 'PUT') return hit(update(state[definition.key] as any[], match[1], definition.extra?.(body) || body, state));
    if (match && method === 'DELETE') return hit(remove(state[definition.key] as any[], match[1], state));
  }
  let match = path.match(new RegExp(`^${base}/groups/([^/]+)/members$`));
  if (match && method === 'GET') return hit(state.members.filter((item) => item.groupId === match![1]));
  if (match && method === 'POST') {
    const user = state.users.find((item) => item.id === body.userId);
    const item = { id: id('member'), groupId: match[1], userId: body.userId, userName: user?.userName, displayName: user?.displayName, memberRole: body.memberRole };
    state.members.push(item); save(state); return hit(item);
  }
  match = path.match(new RegExp(`^${base}/groups/([^/]+)/members/([^/]+)$`));
  if (match && method === 'DELETE') return hit(remove(state.members, match[2], state));
  match = path.match(new RegExp(`^${base}/dict-types/([^/]+)/items$`));
  if (match && method === 'GET') return hit(state.dictItems.filter((item) => item.dictTypeId === match![1] && (!(params.enabledOnly === true || params.enabledOnly === 'true') || item.isActive)));
  if (match && method === 'POST') { const item = { ...body, id: id('dict-item'), dictTypeId: match[1] }; state.dictItems.push(item); save(state); return hit(item); }
  match = path.match(new RegExp(`^${base}/dict-items/([^/]+)$`));
  if (match && method === 'PUT') return hit(update(state.dictItems, match[1], body, state));
  if (match && method === 'DELETE') return hit(remove(state.dictItems, match[1], state));
  return miss();
}

function routeInstrument(state: DemoState, method: string, path: string, params: Record<string, unknown>, body: any) {
  const base = '/api/lab/instruments';
  if (path === base && method === 'GET') return hit(state.instruments.filter((item) => !(params.availableOnly === true || params.availableOnly === 'true') || (item.isActive && item.status === 'normal')));
  if (path === base && method === 'POST') { const item = enrichInstrument(state, { ...body, id: id('instrument') }); state.instruments.push(item); save(state); return hit(item); }
  let match = path.match(/^\/api\/lab\/instruments\/([^/]+)$/);
  if (match && method === 'PUT') return hit(update(state.instruments, match[1], enrichInstrument(state, body), state));

  if (path === `${base}/bookings` && method === 'GET') return hit(filterStatus(state.bookings, params.status));
  if (path === `${base}/bookings` && method === 'POST') {
    const instrument = state.instruments.find((item) => item.id === body.instrumentId);
    const group = state.groups.find((item) => item.id === body.groupId);
    const item = { id: id('booking'), bookingNo: number('BK-DEMO', state.bookings.length), instrumentId: body.instrumentId, instrumentName: instrument?.instrumentName, applicantId: 'user-admin', applicantName: '演示管理员', groupId: body.groupId, groupName: group?.groupName, startTime: body.startTime, endTime: body.endTime, purpose: body.purpose, status: 'pending', createTime: new Date().toISOString() };
    state.bookings.unshift(item); save(state); return hit(item);
  }
  match = path.match(/^\/api\/lab\/instruments\/bookings\/([^/]+)\/(cancel|approve|reject|complete)$/);
  if (match && method === 'POST') {
    const status = ({ cancel: 'cancelled', approve: 'approved', reject: 'rejected', complete: 'completed' } as Record<string, string>)[match[2]];
    return hit(update(state.bookings, match[1], { status, approverId: 'user-admin', approveTime: new Date().toISOString(), approveRemark: body?.remark || '' }, state));
  }

  if (path === `${base}/usages` && method === 'GET') return hit(state.usages);
  if (path === `${base}/usages` && method === 'POST') {
    const instrument = state.instruments.find((item) => item.id === body.instrumentId);
    const item = { ...body, id: id('usage'), instrumentName: instrument?.instrumentName, userId: 'user-admin', userName: '演示管理员', createTime: new Date().toISOString() };
    state.usages.unshift(item); save(state); return hit(item);
  }

  if (path === `${base}/repairs` && method === 'GET') return hit(filterStatus(state.repairs, params.status));
  if (path === `${base}/repairs` && method === 'POST') {
    const instrument = state.instruments.find((item) => item.id === body.instrumentId);
    const item = { id: id('repair'), repairNo: number('RP-DEMO', state.repairs.length), instrumentId: body.instrumentId, instrumentName: instrument?.instrumentName, reporterId: 'user-admin', reporterName: '演示管理员', faultDescription: body.faultDescription, status: 'pending', createTime: new Date().toISOString() };
    state.repairs.unshift(item); save(state); return hit(item);
  }
  match = path.match(/^\/api\/lab\/instruments\/repairs\/([^/]+)\/(approve|reject|start|complete)$/);
  if (match && method === 'POST') {
    const updates: Record<string, any> = match[2] === 'approve' ? { status: 'approved', approveTime: new Date().toISOString(), approverId: 'user-admin' } : match[2] === 'reject' ? { status: 'rejected', approveTime: new Date().toISOString() } : match[2] === 'start' ? { ...body, status: 'repairing', repairStartTime: new Date().toISOString() } : { ...body, status: 'completed', repairEndTime: new Date().toISOString() };
    const repaired = update(state.repairs, match[1], updates, state);
    const instrument = repaired ? state.instruments.find((item) => item.id === repaired.instrumentId) : undefined;
    if (instrument) instrument.status = match[2] === 'complete' ? 'normal' : 'repair';
    save(state); return hit(repaired);
  }
  return miss();
}

function routeInventory(state: DemoState, method: string, path: string, params: Record<string, unknown>, body: any) {
  const base = '/api/lab/inventory';
  if (path === `${base}/materials` && method === 'GET') return hit(state.materials.filter((item) => !(params.enabledOnly === true || params.enabledOnly === 'true') || item.isActive));
  if (path === `${base}/materials` && method === 'POST') { const item = enrichMaterial(state, { ...body, id: id('material'), currentStock: 0 }); state.materials.push(item); save(state); return hit(item); }
  let match = path.match(/^\/api\/lab\/inventory\/materials\/([^/]+)$/);
  if (match && method === 'PUT') return hit(update(state.materials, match[1], enrichMaterial(state, body), state));
  if (path === `${base}/batches` && method === 'GET') return hit(state.batches.filter((item) => !params.materialId || item.materialId === params.materialId));
  if (path === `${base}/batches` && method === 'POST') {
    const material = state.materials.find((item) => item.id === body.materialId)!;
    const item = { id: id('batch'), materialId: body.materialId, materialName: material?.materialName, batchNo: body.batchNo, productionDate: body.productionDate, expiryDate: body.expiryDate, inQuantity: Number(body.quantity), availableQuantity: Number(body.quantity), unitPrice: body.unitPrice, stockInTime: new Date().toISOString(), warningStatus: 'normal' };
    state.batches.unshift(item); if (material) material.currentStock += Number(body.quantity); addFlow(state, item, Number(body.quantity), 'stock-in', body.remark); save(state); return hit(item);
  }
  match = path.match(/^\/api\/lab\/inventory\/batches\/([^/]+)\/adjust$/);
  if (match && method === 'POST') {
    const batch = state.batches.find((item) => item.id === match![1])!; const value = Number(body.quantity); const before = batch.availableQuantity;
    batch.availableQuantity += value; const material = state.materials.find((item) => item.id === batch.materialId); if (material) material.currentStock += value;
    addFlow(state, batch, value, 'adjust', body.reason); save(state); return hit({ beforeQuantity: before, afterQuantity: batch.availableQuantity });
  }
  if (path === `${base}/flows` && method === 'GET') return hit(state.flows.filter((item) => !params.materialId || item.materialId === params.materialId));
  if (path === `${base}/requisitions` && method === 'GET') return hit(filterStatus(state.requisitions, params.status));
  if (path === `${base}/requisitions` && method === 'POST') {
    const group = state.groups.find((item) => item.id === body.groupId);
    const items = (body.items || []).map((entry: any) => { const material = state.materials.find((x) => x.id === entry.materialId); return { id: id('req-item'), materialId: entry.materialId, materialName: material?.materialName, unitName: material?.unitName, requestQuantity: Number(entry.quantity) }; });
    const item = { id: id('req'), requisitionNo: number('REQ-DEMO', state.requisitions.length), applicantName: '演示管理员', groupId: body.groupId, groupName: group?.groupName, purpose: body.purpose, status: 'pending', createTime: new Date().toISOString(), items };
    state.requisitions.unshift(item); save(state); return hit(item);
  }
  match = path.match(/^\/api\/lab\/inventory\/requisitions\/([^/]+)\/(cancel|approve|reject)$/);
  if (match && method === 'POST') {
    const requisition = state.requisitions.find((item) => item.id === match![1])!;
    requisition.status = match[2] === 'cancel' ? 'cancelled' : match[2] === 'approve' ? 'approved' : 'rejected';
    requisition.approverName = '演示管理员'; requisition.approveTime = new Date().toISOString(); requisition.approveRemark = body?.remark || '';
    if (match[2] === 'approve') approveRequisition(state, requisition, body.items || []);
    save(state); return hit(requisition);
  }
  if (path === `${base}/warnings` && method === 'GET') return hit(warnings(state));
  return miss();
}

function routeExperiment(state: DemoState, method: string, path: string, body: any) {
  const base = '/api/lab/experiments';
  if (path === base && method === 'GET') return hit(state.experiments);
  if (path === base && method === 'POST') { const item = enrichExperiment(state, body); state.experiments.unshift(item); save(state); return hit(item); }
  let match = path.match(/^\/api\/lab\/experiments\/([^/]+)$/);
  if (match && method === 'GET') return hit(state.experiments.find((item) => item.id === match![1]));
  if (match && method === 'PUT') return hit(update(state.experiments, match[1], experimentChanges(state, body), state));
  match = path.match(/^\/api\/lab\/experiments\/([^/]+)\/(start|complete|archive|unarchive|reopen)$/);
  if (match && method === 'POST') {
    const status = ({ start: 'in_progress', complete: 'completed', archive: 'archived', unarchive: 'completed', reopen: 'in_progress' } as Record<string, string>)[match[2]];
    return hit(update(state.experiments, match[1], { status, ...(match[2] === 'start' ? { startTime: new Date().toISOString() } : {}), ...(match[2] === 'complete' ? { endTime: new Date().toISOString() } : {}), ...(match[2] === 'archive' ? { archiveTime: new Date().toISOString(), archiveUserId: 'user-admin' } : {}) }, state));
  }
  match = path.match(/^\/api\/lab\/experiments\/([^/]+)\/records$/);
  if (match && method === 'POST') {
    const experiment = state.experiments.find((item) => item.id === match![1])!;
    const record = { id: id('record'), recordType: body.recordType, content: body.content, recordTime: body.recordTime || new Date().toISOString(), creatorId: 'user-admin', creatorName: '演示管理员' };
    experiment.records.unshift(record); save(state); return hit(record);
  }
  if (path === '/api/files' && method === 'POST' && body instanceof FormData) {
    const experiment = state.experiments.find((item) => item.id === body.get('businessId'));
    const file = body.get('file') as File | null;
    const item = { id: id('file'), originalName: file?.name || '演示附件.txt', contentType: file?.type || 'text/plain', fileSize: file?.size || 0, uploaderId: 'user-admin', createTime: new Date().toISOString() };
    experiment?.files.unshift(item); save(state); return hit(item);
  }
  return miss();
}

function routeOperations(state: DemoState, method: string, path: string, params: Record<string, unknown>) {
  if (path === '/api/lab/operations/dashboard' && method === 'GET') {
    const days = Number(params.days || 7), trends = Array.from({ length: days }, (_, index) => ({ date: dateOnly(index - days + 1), value: 3 + ((index * 3) % 8) }));
    return hit({ pendingCount: state.bookings.filter((x) => x.status === 'pending').length + state.requisitions.filter((x) => x.status === 'pending').length, todayBookings: state.bookings.filter((x) => x.startTime.slice(0, 10) === dateOnly(0)).length, repairingInstruments: state.instruments.filter((x) => x.status === 'repair').length, lowStockCount: warnings(state).filter((x) => x.warningStatus === 'low').length, expiringCount: warnings(state).filter((x) => x.expiringBatchCount).length, expiredCount: warnings(state).filter((x) => x.expiredBatchCount).length, recentExperimentCount: state.experiments.filter((x) => x.status !== 'archived').length, archivedExperimentCount: state.experiments.filter((x) => x.status === 'archived').length, instrumentUsageTrend: trends, materialConsumptionTrend: trends.map((x, i) => ({ ...x, value: 5 + ((i * 5) % 11) })) });
  }
  if (path === '/api/lab/operations/approvals' && method === 'GET') {
    const rows = [
      ...state.bookings.map((x) => ({ businessType: 'booking', businessId: x.id, businessNo: x.bookingNo, applicantId: x.applicantId, applicantName: x.applicantName, applyTime: x.createTime, summary: `${x.instrumentName} · ${x.purpose}`, status: x.status, approverId: x.approverId, approveTime: x.approveTime, detailPath: '/lab/booking-approvals' })),
      ...state.requisitions.map((x) => ({ businessType: 'requisition', businessId: x.id, businessNo: x.requisitionNo, applicantId: 'user-zhou', applicantName: x.applicantName, applyTime: x.createTime, summary: `${x.items.length} 项物资 · ${x.purpose}`, status: x.status, approveTime: x.approveTime, detailPath: '/lab/requisition-approvals' }))
    ];
    return hit(filterStatus(rows, params.status));
  }
  if (path.startsWith('/api/lab/operations/exports/') && method === 'GET') return hit(new Blob(['编号\t名称\t状态\nDEMO-001\t演示数据\t正常'], { type: 'application/vnd.ms-excel' }));
  return miss();
}

function routeAi(state: DemoState, method: string, path: string, body: any) {
  if (path === '/api/ai/conversations' && method === 'GET') return hit(state.conversations);
  if (path === '/api/ai/conversations' && method === 'POST') { const time = new Date().toISOString(); const item = { id: id('conversation'), title: body?.title || '新的推理会话', createTime: time, updateTime: time }; state.conversations.unshift(item); save(state); return hit(item); }
  let match = path.match(/^\/api\/ai\/conversations\/([^/]+)\/messages$/);
  if (match && method === 'GET') return hit(state.messages.filter((item) => item.conversationId === match![1]));
  match = path.match(/^\/api\/ai\/conversations\/([^/]+)\/reason$/);
  if (match && method === 'POST') {
    const time = new Date().toISOString(), result = reasoning(body.content);
    const userMessage = { id: id('message'), conversationId: match[1], role: 'user' as const, content: body.content, messageType: 'text' as const, createTime: time };
    const assistantMessage = { id: id('message'), conversationId: match[1], role: 'assistant' as const, content: result.summary, messageType: 'reasoning' as const, result, createTime: time };
    state.messages.push(userMessage, assistantMessage); const conversation = state.conversations.find((item) => item.id === match![1]); if (conversation) { conversation.title = body.content.slice(0, 18); conversation.updateTime = time; } save(state);
    return hit({ userMessage, assistantMessage, result, provider: 'HXS Demo Reasoner' });
  }
  match = path.match(/^\/api\/ai\/conversations\/([^/]+)$/);
  if (match && method === 'DELETE') { remove(state.conversations, match[1], state); state.messages = state.messages.filter((item) => item.conversationId !== match![1]); save(state); return hit({}); }
  return miss();
}

function routeVisualization(state: DemoState, method: string, path: string, body: any) {
  const base = '/api/lab/3d';
  if (path === `${base}/layout` && method === 'GET') return hit(spatialLayout(state));
  let match = path.match(/^\/api\/lab\/3d\/layout\/([^/]+)\/statuses$/);
  if (match && method === 'GET') return hit(spatialLayout(state).find((item) => item.id === match![1])?.rooms.map((room: any) => ({ nodeId: room.id, nodeType: 'room', status: room.repairingInstrumentCount ? 'repair' : 'normal', detail: room.fullPath, upcomingBookingCount: room.upcomingBookingCount, repairingInstrumentCount: room.repairingInstrumentCount, nextBookingTime: room.instruments.map((x: any) => x.nextBookingTime).filter(Boolean)[0] })) || []);
  if (path === `${base}/scenes` && method === 'GET') return hit(state.scenes.map((item) => item.scene));
  if (path === `${base}/manage/scenes` && method === 'GET') return hit(state.scenes.map((item) => ({ scene: item.scene, nodeCount: item.nodes.length, createTime: item.createTime, updateTime: item.updateTime })));
  if (path === `${base}/manage/scenes` && method === 'POST') {
    const lab = state.labs.find((item) => item.id === body.labId)!; const time = new Date().toISOString();
    const value = { scene: { ...body, id: id('scene'), labName: lab?.labName, version: 1 }, nodes: [], models: [], createTime: time, updateTime: time };
    state.scenes.push(value); save(state); return hit({ scene: value.scene, nodeCount: 0, createTime: time, updateTime: time });
  }
  match = path.match(/^\/api\/lab\/3d\/scenes\/([^/]+)(?:\/(statuses))?$/);
  if (match && method === 'GET') { const value = state.scenes.find((item) => item.scene.id === match![1]); return hit(match[2] ? value?.nodes.map((node) => ({ nodeId: node.id, status: node.status, detail: node.detail })) || [] : { scene: value?.scene, nodes: value?.nodes || [] }); }
  match = path.match(/^\/api\/lab\/3d\/manage\/scenes\/([^/]+)$/);
  if (match && method === 'PUT') { const value = state.scenes.find((item) => item.scene.id === match![1])!; Object.assign(value.scene, body); value.updateTime = new Date().toISOString(); save(state); return hit({ scene: value.scene, nodeCount: value.nodes.length, createTime: value.createTime, updateTime: value.updateTime }); }
  if (match && method === 'DELETE') { state.scenes = state.scenes.filter((item) => item.scene.id !== match![1]); save(state); return hit({}); }
  match = path.match(/^\/api\/lab\/3d\/manage\/scenes\/([^/]+)\/models$/);
  if (match && method === 'GET') return hit(state.scenes.find((item) => item.scene.id === match![1])?.models || []);
  match = path.match(/^\/api\/lab\/3d\/manage\/scenes\/([^/]+)\/model$/);
  if (match && method === 'POST') { const value = state.scenes.find((item) => item.scene.id === match![1])!; value.models.forEach((x) => x.isCurrent = false); const file = body instanceof FormData ? body.get('file') as File | null : null; const model = { fileId: id('model'), fileName: file?.name || 'demo-model.glb', fileSize: file?.size || 0, uploaderId: 'user-admin', createTime: new Date().toISOString(), isCurrent: true }; value.models.unshift(model); value.scene.version++; save(state); return hit(model); }
  match = path.match(/^\/api\/lab\/3d\/manage\/scenes\/([^/]+)\/models\/([^/]+)\/activate$/);
  if (match && method === 'PUT') { const value = state.scenes.find((item) => item.scene.id === match![1])!; value.models.forEach((x) => x.isCurrent = x.fileId === match![2]); save(state); return hit({}); }
  match = path.match(/^\/api\/lab\/3d\/manage\/scenes\/([^/]+)\/nodes$/);
  if (match && method === 'POST') { const value = state.scenes.find((item) => item.scene.id === match![1])!; const node = { ...body, id: id('node'), status: 'normal' }; value.nodes.push(node); save(state); return hit(node); }
  match = path.match(/^\/api\/lab\/3d\/manage\/nodes\/([^/]+)(?:\/(binding))?$/);
  if (match) {
    const value = state.scenes.find((scene) => scene.nodes.some((node) => node.id === match![1]))!;
    if (match[2] === 'binding' && method === 'PUT') return hit(update(value.nodes, match[1], body, state));
    if (match[2] === 'binding' && method === 'DELETE') return hit(update(value.nodes, match[1], { businessType: undefined, businessId: undefined }, state));
    if (method === 'PUT') return hit(update(value.nodes, match[1], body, state));
    if (method === 'DELETE') return hit(remove(value.nodes, match[1], state));
  }
  return miss();
}

function loginResponse() {
  return { accessToken: `demo-${Date.now()}`, tokenType: 'Bearer', expiresAt: new Date(Date.now() + 8 * 3600_000).toISOString(), user: { id: 'user-admin', userName: 'admin', displayName: '演示管理员' } };
}

function parseBody(value: unknown) {
  if (value instanceof FormData) return value;
  if (typeof value !== 'string') return value || {};
  try { return JSON.parse(value); } catch { return value; }
}

function hit(data: any) { return { handled: true, data }; }
function miss() { return { handled: false, data: undefined }; }
function id(prefix: string) { return `${prefix}-${crypto.randomUUID()}`; }
function number(prefix: string, length: number) { return `${prefix}-${String(length + 1).padStart(3, '0')}`; }

function update<T extends { id: string }>(rows: T[], itemId: string, changes: Partial<T>, state: DemoState) {
  const item = rows.find((row) => row.id === itemId);
  if (item) Object.assign(item, changes);
  save(state);
  return item;
}

function remove<T extends { id: string }>(rows: T[], itemId: string, state: DemoState) {
  const index = rows.findIndex((row) => row.id === itemId);
  if (index >= 0) rows.splice(index, 1);
  save(state);
  return {};
}

function tree<T extends { id: string; parentId?: string | null }>(rows: T[]): T[] {
  const nodes = new Map(rows.map((row) => [row.id, { ...row, children: [] as T[] }]));
  const roots: T[] = [];
  for (const node of nodes.values()) {
    const parent = node.parentId ? nodes.get(node.parentId) : undefined;
    if (parent) parent.children.push(node as T); else roots.push(node as T);
  }
  const sortNodes = (values: any[]): void => {
    values.sort((a, b) => (a.sortNo || 0) - (b.sortNo || 0)).forEach((item) => sortNodes(item.children || []));
  };
  sortNodes(roots);
  return roots;
}

function filterStatus<T extends { status: string }>(rows: T[], status: unknown) { return status ? rows.filter((item) => item.status === status) : rows; }

function enrichInstrument(state: DemoState, value: any) {
  return { ...value, categoryName: state.dictItems.find((x) => x.id === value.categoryId)?.itemLabel, supplierName: state.suppliers.find((x) => x.id === value.supplierId)?.supplierName, labName: state.labs.find((x) => x.id === value.labId)?.labName, locationName: state.locations.find((x) => x.id === value.locationId)?.locationName };
}

function enrichMaterial(state: DemoState, value: any) {
  return { ...value, categoryName: state.dictItems.find((x) => x.id === value.categoryId)?.itemLabel, unitName: state.dictItems.find((x) => x.id === value.unitId)?.itemLabel, supplierName: state.suppliers.find((x) => x.id === value.supplierId)?.supplierName, storageLocationName: state.locations.find((x) => x.id === value.storageLocationId)?.locationName };
}

function enrichExperiment(state: DemoState, body: any) {
  const time = new Date().toISOString();
  return { id: id('experiment'), experimentNo: number('EXP-DEMO', state.experiments.length), experimentName: body.experimentName, groupId: body.groupId, groupName: state.groups.find((x) => x.id === body.groupId)?.groupName, ownerId: 'user-admin', ownerName: '演示管理员', topicName: body.topicName, purpose: body.purpose, status: 'draft', startTime: null, endTime: null, archiveUserId: null, archiveTime: null, createTime: time, records: [], files: [], ...experimentChanges(state, body) };
}

function experimentChanges(state: DemoState, body: any) {
  return { ...body, groupName: state.groups.find((x) => x.id === body.groupId)?.groupName, instruments: (body.instruments || []).map((entry: any) => ({ id: id('exp-ins'), ...entry, instrumentName: state.instruments.find((x) => x.id === entry.instrumentId)?.instrumentName, bookingNo: state.bookings.find((x) => x.id === entry.bookingId)?.bookingNo })), materials: (body.materials || []).map((entry: any) => ({ id: id('exp-mat'), ...entry, materialName: state.materials.find((x) => x.id === entry.materialId)?.materialName, unitName: state.materials.find((x) => x.id === entry.materialId)?.unitName, requisitionNo: state.requisitions.find((x) => x.id === entry.requisitionId)?.requisitionNo })) };
}

function addFlow(state: DemoState, batch: any, quantity: number, sourceType: string, remark?: string) {
  const after = batch.availableQuantity, before = after - quantity;
  state.flows.unshift({ id: id('flow'), flowNo: number('FL-DEMO', state.flows.length), materialId: batch.materialId, materialName: batch.materialName, batchId: batch.id, batchNo: batch.batchNo, flowType: quantity >= 0 ? 'in' : 'out', quantity, beforeQuantity: before, afterQuantity: after, sourceType, operatorName: '演示管理员', remark, createTime: new Date().toISOString() });
}

function approveRequisition(state: DemoState, requisition: any, approvals: any[]) {
  for (const item of requisition.items) {
    const approved = Number(approvals.find((x) => x.itemId === item.id)?.approvedQuantity ?? item.requestQuantity);
    item.approvedQuantity = approved;
    const batch = state.batches.find((x) => x.materialId === item.materialId && x.availableQuantity >= approved);
    const material = state.materials.find((x) => x.id === item.materialId);
    if (batch) { batch.availableQuantity -= approved; addFlow(state, batch, -approved, 'requisition', requisition.requisitionNo); }
    if (material) material.currentStock -= approved;
  }
}

function warnings(state: DemoState) {
  return state.materials.map((material) => {
    const batches = state.batches.filter((x) => x.materialId === material.id), today = Date.now();
    const expiredBatchCount = batches.filter((x) => x.expiryDate && new Date(x.expiryDate).getTime() < today).length;
    const expiringBatchCount = batches.filter((x) => x.expiryDate && new Date(x.expiryDate).getTime() >= today && new Date(x.expiryDate).getTime() < today + 30 * 86400_000).length;
    return { materialId: material.id, materialCode: material.materialCode, materialName: material.materialName, currentStock: material.currentStock, minStock: material.minStock, expiringBatchCount, expiredBatchCount, warningStatus: expiredBatchCount ? 'expired' : material.currentStock < material.minStock ? 'low' : expiringBatchCount ? 'expiring' : 'normal' };
  });
}

function dateOnly(offset: number) { const value = new Date(); value.setDate(value.getDate() + offset); return value.toISOString().slice(0, 10); }

function reasoning(content: string) {
  return { summary: `已从“${content.slice(0, 24)}${content.length > 24 ? '…' : ''}”中提取实验室业务信息，并形成初步判断。`, facts: ['输入描述已记录', '当前结果由在线演示规则生成'], inferences: ['问题可能涉及设备状态、实验条件或操作流程', '需要结合原始记录和设备日志进一步确认'], risks: ['信息不足可能导致结论偏差', '关键操作前应由实验负责人复核'], suggestions: ['补充时间、设备编号和实验条件', '核对同批次样品与历史记录', '形成复核记录后再执行处置'], missingInformation: ['设备运行日志', '原始检测数据', '操作人员与环境条件'], confidence: 0.82 };
}

function spatialLayout(state: DemoState) {
  return state.labs.map((lab) => {
    const roomLocations = state.locations.filter((item) => item.labId === lab.id && item.locationType === 'room');
    const rooms = roomLocations.map((room) => {
      const floor = ancestors(state.locations, room).find((item) => item.locationType === 'floor');
      const building = ancestors(state.locations, room).find((item) => item.locationType === 'building');
      const instruments = state.instruments.filter((item) => item.locationId === room.id).map((instrument) => {
        const bookings = state.bookings.filter((item) => item.instrumentId === instrument.id && ['pending', 'approved'].includes(item.status));
        const repair = state.repairs.find((item) => item.instrumentId === instrument.id && ['pending', 'approved', 'repairing'].includes(item.status));
        return { id: instrument.id, code: instrument.instrumentCode, name: instrument.instrumentName, status: instrument.status, model: instrument.model, locationName: room.locationName, upcomingBookingCount: bookings.length, nextBookingTime: bookings.map((x) => x.startTime).sort()[0], repairStatus: repair?.status };
      });
      return { id: room.id, code: room.locationCode, name: room.locationName, buildingName: building?.locationName || '实验楼', floorName: floor?.locationName || '楼层', floorNumber: Number(floor?.locationCode.match(/(\d+)F/)?.[1] || room.locationCode.match(/(\d)/)?.[1] || 1), fullPath: [building?.locationName, floor?.locationName, room.locationName].filter(Boolean).join(' / '), upcomingBookingCount: instruments.reduce((sum, item) => sum + item.upcomingBookingCount, 0), repairingInstrumentCount: instruments.filter((item) => item.repairStatus).length, instruments };
    });
    return { id: lab.id, code: lab.labCode, name: lab.labName, description: lab.description, rooms };
  });
}

function ancestors(rows: LocationDto[], item: LocationDto) {
  const result: LocationDto[] = []; let current: LocationDto | undefined = item;
  while (current?.parentId) { current = rows.find((row) => row.id === current!.parentId); if (current) result.push(current); }
  return result;
}
