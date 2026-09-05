import type { AiConversation, AiMessage } from '../types/ai';
import type { ExperimentDto } from '../types/experiment';
import type { BookingDto, InstrumentDto, RepairDto, UsageDto } from '../types/instrument';
import type { MaterialDto, RequisitionDto, StockBatchDto, StockFlowDto } from '../types/inventory';
import type { DictItemDto, DictTypeDto, GroupMemberDto, LabDto, LabGroupDto, LocationDto, SupplierDto } from '../types/lab';
import type { AuditLogDto, MenuDto, OrgDto, RoleDto, UserDto } from '../types/system';
import type { Lab3dModelVersion, Lab3dNode, Lab3dScene } from '../types/visualization';

export interface DemoState {
  version: number;
  users: UserDto[];
  roles: RoleDto[];
  userRoles: Record<string, string[]>;
  orgs: OrgDto[];
  menus: MenuDto[];
  roleMenus: Record<string, string[]>;
  auditLogs: AuditLogDto[];
  labs: LabDto[];
  locations: LocationDto[];
  groups: LabGroupDto[];
  members: GroupMemberDto[];
  suppliers: SupplierDto[];
  dictTypes: DictTypeDto[];
  dictItems: DictItemDto[];
  instruments: InstrumentDto[];
  bookings: BookingDto[];
  usages: UsageDto[];
  repairs: RepairDto[];
  materials: MaterialDto[];
  batches: StockBatchDto[];
  flows: StockFlowDto[];
  requisitions: RequisitionDto[];
  experiments: ExperimentDto[];
  conversations: AiConversation[];
  messages: AiMessage[];
  scenes: Array<{ scene: Lab3dScene; nodes: Lab3dNode[]; models: Lab3dModelVersion[]; createTime: string; updateTime: string }>;
}

const now = new Date();
const at = (dayOffset: number, hour = 9, minute = 0) => {
  const value = new Date(now);
  value.setDate(value.getDate() + dayOffset);
  value.setHours(hour, minute, 0, 0);
  return value.toISOString();
};

const menu = (id: string, menuName: string, menuType: string, routePath: string | null, component: string | null, icon: string, sortNo: number, parentId: string | null = null, permissionCode: string | null = null): MenuDto => ({
  id,
  parentId,
  menuCode: id,
  menuName,
  menuType,
  routePath,
  component,
  icon,
  permissionCode,
  sortNo,
  isVisible: menuType !== 'button',
  isActive: true
});

const page = (id: string, name: string, route: string, component: string, icon: string, sort: number, parent: string, permission: string) =>
  menu(id, name, 'page', route, component, icon, sort, parent, permission);

const button = (permission: string, name: string, parent: string, sort = 1000) =>
  menu(`permission:${permission}`, name, 'button', null, null, 'check', sort, parent, permission);

function buildMenus(): MenuDto[] {
  const rows = [
    page('dashboard', '实验室首页', '/', 'views/dashboard/DashboardView.vue', 'home', 1, '', 'dashboard:view'),
    page('ai:reasoning', '数据推理', '/ai/reasoning', 'views/ai/ReasoningView.vue', 'bulb', 5, '', 'ai:reasoning:use'),
    menu('lab', '实验室业务', 'directory', null, null, 'experiment', 10),
    menu('lab:foundation-group', '基础资料', 'directory', null, null, 'apps', 10, 'lab'),
    page('lab:labs', '实验室管理', '/lab/labs', 'views/lab/LabView.vue', 'experiment', 10, 'lab:foundation-group', 'lab:base:view'),
    page('lab:locations', '位置管理', '/lab/locations', 'views/lab/LocationView.vue', 'environment', 20, 'lab:foundation-group', 'lab:base:view'),
    page('lab:groups', '课题组管理', '/lab/groups', 'views/lab/GroupView.vue', 'team', 30, 'lab:foundation-group', 'lab:base:view'),
    page('lab:suppliers', '供应商管理', '/lab/suppliers', 'views/lab/SupplierView.vue', 'shop', 40, 'lab:foundation-group', 'lab:base:view'),
    page('lab:dictionaries', '数据字典', '/lab/dictionaries', 'views/lab/DictionaryView.vue', 'database', 50, 'lab:foundation-group', 'lab:base:view'),
    menu('lab:instrument-group', '仪器业务', 'directory', null, null, 'experiment', 20, 'lab'),
    page('lab:instruments', '仪器台账', '/lab/instruments', 'views/lab/InstrumentView.vue', 'experiment', 10, 'lab:instrument-group', 'lab:instrument:view'),
    page('lab:bookings', '仪器预约', '/lab/bookings', 'views/lab/BookingView.vue', 'calendar', 20, 'lab:instrument-group', 'lab:booking:view'),
    page('lab:usages', '使用记录', '/lab/usages', 'views/lab/UsageView.vue', 'database', 30, 'lab:instrument-group', 'lab:usage:view'),
    page('lab:repairs', '设备报修', '/lab/repairs', 'views/lab/RepairView.vue', 'setting', 40, 'lab:instrument-group', 'lab:repair:view'),
    menu('lab:inventory-group', '库存业务', 'directory', null, null, 'database', 30, 'lab'),
    page('lab:materials', '试剂耗材', '/lab/materials', 'views/lab/MaterialView.vue', 'shop', 10, 'lab:inventory-group', 'lab:inventory:view'),
    page('lab:stock-batches', '批次库存', '/lab/stock-batches', 'views/lab/StockBatchView.vue', 'database', 20, 'lab:inventory-group', 'lab:inventory:view'),
    page('lab:stock-flows', '库存流水', '/lab/stock-flows', 'views/lab/StockFlowView.vue', 'history', 30, 'lab:inventory-group', 'lab:inventory:view'),
    page('lab:requisitions', '领用申请', '/lab/requisitions', 'views/lab/RequisitionView.vue', 'file', 40, 'lab:inventory-group', 'lab:requisition:view'),
    page('lab:inventory-warnings', '库存预警', '/lab/inventory-warnings', 'views/lab/InventoryWarningView.vue', 'chart', 50, 'lab:inventory-group', 'lab:inventory:view'),
    menu('lab:experiment-group', '实验业务', 'directory', null, null, 'file', 40, 'lab'),
    page('lab:experiments', '我的实验', '/lab/experiments', 'views/lab/ExperimentView.vue', 'experiment', 10, 'lab:experiment-group', 'lab:experiment:view'),
    page('lab:experiments-query', '实验任务查询', '/lab/experiment-query', 'views/lab/ExperimentQueryView.vue', 'search', 20, 'lab:experiment-group', 'lab:experiment:view'),
    menu('lab:approval-group', '审批中心', 'directory', null, null, 'check', 50, 'lab'),
    page('lab:booking-approvals', '预约审批', '/lab/booking-approvals', 'views/lab/BookingApprovalView.vue', 'shield', 10, 'lab:approval-group', 'lab:booking:approve'),
    page('lab:requisition-approvals', '领用审批', '/lab/requisition-approvals', 'views/lab/RequisitionApprovalView.vue', 'check', 20, 'lab:approval-group', 'lab:requisition:approve'),
    page('lab:approval-center', '统一审批', '/lab/approval-center', 'views/lab/ApprovalCenterView.vue', 'check', 30, 'lab:approval-group', 'lab:approval:center:view'),
    menu('lab:visual-group', '空间可视化', 'directory', null, null, 'apps', 60, 'lab'),
    page('lab:3d', '3D 实验室', '/lab/3d', 'views/lab/Lab3dView.vue', 'experiment', 10, 'lab:visual-group', 'lab:3d:view'),
    page('lab:3d-manage', '3D 场景管理', '/lab/3d/manage', 'views/lab/Lab3dManageView.vue', 'setting', 20, 'lab:visual-group', 'lab:3d:manage'),
    menu('sys', '系统管理', 'directory', null, null, 'setting', 90),
    page('sys:user', '用户管理', '/system/users', 'views/system/UserView.vue', 'user', 10, 'sys', 'sys:user:list'),
    page('sys:role', '角色管理', '/system/roles', 'views/system/RoleView.vue', 'shield', 20, 'sys', 'sys:role:list'),
    page('sys:menu', '菜单管理', '/system/menus', 'views/system/MenuView.vue', 'menu', 30, 'sys', 'sys:menu:list'),
    page('sys:org', '组织架构', '/system/orgs', 'views/system/OrgView.vue', 'team', 40, 'sys', 'sys:org:list'),
    page('sys:audit', '操作日志', '/system/audit-logs', 'views/system/AuditLogView.vue', 'history', 50, 'sys', 'sys:audit:list')
  ];

  const permissions: Array<[string, string, string]> = [
    ['lab:base:manage', '维护基础资料', 'lab:labs'], ['lab:instrument:manage', '维护仪器', 'lab:instruments'],
    ['lab:booking:create', '申请预约', 'lab:bookings'], ['lab:booking:cancel', '取消预约', 'lab:bookings'],
    ['lab:booking:approve', '审批预约', 'lab:booking-approvals'], ['lab:usage:create', '登记使用', 'lab:usages'],
    ['lab:repair:create', '提交报修', 'lab:repairs'], ['lab:repair:approve', '审批报修', 'lab:repairs'],
    ['lab:repair:work', '维修处理', 'lab:repairs'], ['lab:material:manage', '维护试剂耗材', 'lab:materials'],
    ['lab:stock:in', '登记入库', 'lab:stock-batches'], ['lab:stock:adjust', '调整库存', 'lab:stock-batches'],
    ['lab:requisition:create', '提交领用', 'lab:requisitions'], ['lab:requisition:cancel', '取消领用', 'lab:requisitions'],
    ['lab:requisition:approve', '审批领用', 'lab:requisition-approvals'], ['lab:experiment:create', '新建实验', 'lab:experiments'],
    ['lab:experiment:edit', '编辑实验', 'lab:experiments'], ['lab:experiment:record', '记录实验', 'lab:experiments'],
    ['lab:experiment:archive', '归档实验', 'lab:experiments'], ['lab:experiment:unarchive', '解档实验', 'lab:experiments-query'],
    ['lab:export', '导出数据', 'lab:approval-center'], ['lab:3d:manage', '维护三维场景', 'lab:3d-manage'],
    ['sys:user:create', '新增用户', 'sys:user'], ['sys:user:edit', '编辑用户', 'sys:user'], ['sys:user:delete', '删除用户', 'sys:user'], ['sys:user:assign-role', '分配角色', 'sys:user'],
    ['sys:role:create', '新增角色', 'sys:role'], ['sys:role:edit', '编辑角色', 'sys:role'], ['sys:role:delete', '删除角色', 'sys:role'], ['sys:role:assign-menu', '角色授权', 'sys:role'],
    ['sys:menu:create', '新增菜单', 'sys:menu'], ['sys:menu:edit', '编辑菜单', 'sys:menu'], ['sys:menu:delete', '删除菜单', 'sys:menu'],
    ['sys:org:create', '新增组织', 'sys:org'], ['sys:org:edit', '编辑组织', 'sys:org'], ['sys:org:delete', '删除组织', 'sys:org']
  ];
  rows.push(...permissions.map(([code, name, parent], index) => button(code, name, parent, 1000 + index)));
  return rows.map((item) => ({ ...item, parentId: item.parentId || null }));
}

export function createDemoState(): DemoState {
  const menus = buildMenus();
  const allMenuIds = menus.map((item) => item.id);
  const today = at(0, 10);
  return {
    version: 1,
    users: [
      { id: 'user-admin', orgId: 'org-lab', userName: 'admin', displayName: '演示管理员', phone: '138****1001', email: 'admin@example.com', isActive: true, lastLoginTime: today },
      { id: 'user-lin', orgId: 'org-analysis', userName: 'linyan', displayName: '林妍', phone: '138****2036', email: 'linyan@example.com', isActive: true, lastLoginTime: at(-1, 16) },
      { id: 'user-zhou', orgId: 'org-biology', userName: 'zhouming', displayName: '周明', phone: '138****2871', email: 'zhouming@example.com', isActive: true, lastLoginTime: at(-2, 11) },
      { id: 'user-chen', orgId: 'org-analysis', userName: 'chenxi', displayName: '陈曦', phone: '138****3320', email: 'chenxi@example.com', isActive: true, lastLoginTime: at(-3, 9) }
    ],
    roles: [
      { id: 'role-admin', roleCode: 'admin', roleName: '系统管理员', description: '拥有在线演示的全部功能权限', isActive: true },
      { id: 'role-lab-admin', roleCode: 'lab_admin', roleName: '实验管理员', description: '维护实验室业务并执行审批', isActive: true },
      { id: 'role-lab-user', roleCode: 'lab_user', roleName: '普通实验员', description: '提交申请并维护实验记录', isActive: true }
    ],
    userRoles: { 'user-admin': ['role-admin'], 'user-lin': ['role-lab-admin'], 'user-zhou': ['role-lab-user'], 'user-chen': ['role-lab-user'] },
    orgs: [
      { id: 'org-lab', parentId: null, orgName: '材料科学研究院', orgCode: 'MSRI', orgType: 'company', sortNo: 1, isActive: true },
      { id: 'org-analysis', parentId: 'org-lab', orgName: '分析测试中心', orgCode: 'ANALYSIS', orgType: 'department', sortNo: 10, isActive: true },
      { id: 'org-biology', parentId: 'org-lab', orgName: '生物实验中心', orgCode: 'BIOLOGY', orgType: 'department', sortNo: 20, isActive: true }
    ],
    menus,
    roleMenus: { 'role-admin': allMenuIds, 'role-lab-admin': allMenuIds.filter((id) => !id.startsWith('sys')), 'role-lab-user': allMenuIds.filter((id) => !id.startsWith('sys') && !id.includes('approval') && !id.includes('manage')) },
    auditLogs: [
      { id: 'audit-1', userId: 'user-admin', userName: 'admin', moduleCode: 'lab:booking', actionCode: 'approve', businessId: 'booking-1', requestPath: '/api/lab/instruments/bookings/booking-1/approve', httpMethod: 'POST', result: 'success', ipAddress: '10.0.0.8', createTime: at(0, 9, 20) },
      { id: 'audit-2', userId: 'user-lin', userName: 'linyan', moduleCode: 'lab:experiment', actionCode: 'record', businessId: 'experiment-1', requestPath: '/api/lab/experiments/experiment-1/records', httpMethod: 'POST', result: 'success', ipAddress: '10.0.0.16', createTime: at(-1, 15, 42) },
      { id: 'audit-3', userId: 'user-admin', userName: 'admin', moduleCode: 'lab:inventory', actionCode: 'stock-in', businessId: 'batch-1', requestPath: '/api/lab/inventory/batches', httpMethod: 'POST', result: 'success', ipAddress: '10.0.0.8', createTime: at(-2, 10, 15) }
    ],
    labs: [
      { id: 'lab-central', labCode: 'LAB-CENTRAL', labName: '中心分析实验室', managerId: 'user-lin', managerName: '林妍', description: '承担材料表征、光谱与色谱分析任务', isActive: true },
      { id: 'lab-bio', labCode: 'LAB-BIO', labName: '生物技术实验室', managerId: 'user-zhou', managerName: '周明', description: '开展微生物培养与分子检测', isActive: true }
    ],
    locations: [
      { id: 'loc-building-a', labId: 'lab-central', parentId: null, locationCode: 'A', locationName: '科研 A 栋', locationType: 'building', sortNo: 1, isActive: true },
      { id: 'loc-a-1f', labId: 'lab-central', parentId: 'loc-building-a', locationCode: 'A-1F', locationName: '一层', locationType: 'floor', sortNo: 10, isActive: true },
      { id: 'room-101', labId: 'lab-central', parentId: 'loc-a-1f', locationCode: 'A101', locationName: '光谱分析室', locationType: 'room', sortNo: 11, isActive: true },
      { id: 'room-102', labId: 'lab-central', parentId: 'loc-a-1f', locationCode: 'A102', locationName: '样品制备室', locationType: 'room', sortNo: 12, isActive: true },
      { id: 'loc-a-2f', labId: 'lab-central', parentId: 'loc-building-a', locationCode: 'A-2F', locationName: '二层', locationType: 'floor', sortNo: 20, isActive: true },
      { id: 'room-201', labId: 'lab-central', parentId: 'loc-a-2f', locationCode: 'A201', locationName: '色谱分析室', locationType: 'room', sortNo: 21, isActive: true },
      { id: 'room-202', labId: 'lab-central', parentId: 'loc-a-2f', locationCode: 'A202', locationName: '精密仪器室', locationType: 'room', sortNo: 22, isActive: true },
      { id: 'loc-a-3f', labId: 'lab-central', parentId: 'loc-building-a', locationCode: 'A-3F', locationName: '三层', locationType: 'floor', sortNo: 30, isActive: true },
      { id: 'room-301', labId: 'lab-central', parentId: 'loc-a-3f', locationCode: 'A301', locationName: '电子显微镜室', locationType: 'room', sortNo: 31, isActive: true },
      { id: 'room-302', labId: 'lab-central', parentId: 'loc-a-3f', locationCode: 'A302', locationName: '数据处理室', locationType: 'room', sortNo: 32, isActive: true },
      { id: 'loc-building-b', labId: 'lab-bio', parentId: null, locationCode: 'B', locationName: '科研 B 栋', locationType: 'building', sortNo: 1, isActive: true },
      { id: 'room-b203', labId: 'lab-bio', parentId: 'loc-building-b', locationCode: 'B203', locationName: '细胞培养室', locationType: 'room', sortNo: 20, isActive: true }
    ],
    groups: [
      { id: 'group-material', labId: 'lab-central', groupCode: 'GRP-MAT', groupName: '先进材料课题组', leaderId: 'user-lin', leaderName: '林妍', description: '功能材料结构与性能研究', isActive: true },
      { id: 'group-bio', labId: 'lab-bio', groupCode: 'GRP-BIO', groupName: '分子生物课题组', leaderId: 'user-zhou', leaderName: '周明', description: '分子检测和培养实验', isActive: true }
    ],
    members: [
      { id: 'member-1', groupId: 'group-material', userId: 'user-lin', userName: 'linyan', displayName: '林妍', memberRole: 'leader' },
      { id: 'member-2', groupId: 'group-material', userId: 'user-chen', userName: 'chenxi', displayName: '陈曦', memberRole: 'member' },
      { id: 'member-3', groupId: 'group-bio', userId: 'user-zhou', userName: 'zhouming', displayName: '周明', memberRole: 'leader' }
    ],
    suppliers: [
      { id: 'supplier-1', supplierCode: 'SUP-THERMO', supplierName: '赛默飞世尔科技', contactName: '王工', phone: '021-****8890', email: 'service@example.com', address: '上海市浦东新区', isActive: true },
      { id: 'supplier-2', supplierCode: 'SUP-LOCAL', supplierName: '华科实验物资', contactName: '赵经理', phone: '010-****6128', email: 'sales@example.com', address: '北京市海淀区', isActive: true }
    ],
    dictTypes: [
      { id: 'dict-instrument', dictCode: 'instrument_category', dictName: '仪器分类', description: '实验室仪器分类', isActive: true },
      { id: 'dict-reagent', dictCode: 'reagent_category', dictName: '试剂分类', description: '化学试剂分类', isActive: true },
      { id: 'dict-consumable', dictCode: 'consumable_category', dictName: '耗材分类', description: '实验耗材分类', isActive: true },
      { id: 'dict-unit', dictCode: 'measurement_unit', dictName: '计量单位', description: '库存计量单位', isActive: true }
    ],
    dictItems: [
      { id: 'cat-spectrum', dictTypeId: 'dict-instrument', itemValue: 'spectrum', itemLabel: '光谱仪器', sortNo: 10, isActive: true },
      { id: 'cat-chromatography', dictTypeId: 'dict-instrument', itemValue: 'chromatography', itemLabel: '色谱仪器', sortNo: 20, isActive: true },
      { id: 'cat-microscope', dictTypeId: 'dict-instrument', itemValue: 'microscope', itemLabel: '显微分析', sortNo: 30, isActive: true },
      { id: 'cat-solvent', dictTypeId: 'dict-reagent', itemValue: 'solvent', itemLabel: '有机溶剂', sortNo: 10, isActive: true },
      { id: 'cat-standard', dictTypeId: 'dict-reagent', itemValue: 'standard', itemLabel: '标准品', sortNo: 20, isActive: true },
      { id: 'cat-glass', dictTypeId: 'dict-consumable', itemValue: 'glass', itemLabel: '玻璃器皿', sortNo: 10, isActive: true },
      { id: 'unit-ml', dictTypeId: 'dict-unit', itemValue: 'ml', itemLabel: '毫升', sortNo: 10, isActive: true },
      { id: 'unit-piece', dictTypeId: 'dict-unit', itemValue: 'piece', itemLabel: '个', sortNo: 20, isActive: true }
    ],
    instruments: [
      { id: 'ins-hplc', instrumentCode: 'INS-HPLC-001', instrumentName: '高效液相色谱仪', categoryId: 'cat-chromatography', categoryName: '色谱仪器', model: 'Vanquish Core', manufacturer: 'Thermo Fisher', supplierId: 'supplier-1', supplierName: '赛默飞世尔科技', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-201', locationName: 'A201 色谱分析室', status: 'normal', description: '公共分析检测设备', isActive: true },
      { id: 'ins-gc', instrumentCode: 'INS-GC-002', instrumentName: '气相色谱质谱联用仪', categoryId: 'cat-chromatography', categoryName: '色谱仪器', model: '8890-5977B', manufacturer: 'Agilent', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-201', locationName: 'A201 色谱分析室', status: 'normal', description: '挥发性组分定性定量分析', isActive: true },
      { id: 'ins-uv', instrumentCode: 'INS-UV-003', instrumentName: '紫外可见分光光度计', categoryId: 'cat-spectrum', categoryName: '光谱仪器', model: 'UV-2600i', manufacturer: 'Shimadzu', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-101', locationName: 'A101 光谱分析室', status: 'normal', description: '常规吸收光谱分析', isActive: true },
      { id: 'ins-ftir', instrumentCode: 'INS-FTIR-004', instrumentName: '傅里叶红外光谱仪', categoryId: 'cat-spectrum', categoryName: '光谱仪器', model: 'Nicolet iS50', manufacturer: 'Thermo Fisher', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-101', locationName: 'A101 光谱分析室', status: 'repair', description: '官能团及材料结构分析', isActive: true },
      { id: 'ins-sem', instrumentCode: 'INS-SEM-005', instrumentName: '场发射扫描电子显微镜', categoryId: 'cat-microscope', categoryName: '显微分析', model: 'GeminiSEM 360', manufacturer: 'ZEISS', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-301', locationName: 'A301 电子显微镜室', status: 'normal', description: '材料微观形貌与能谱分析', isActive: true },
      { id: 'ins-xrd', instrumentCode: 'INS-XRD-006', instrumentName: 'X 射线衍射仪', categoryId: 'cat-spectrum', categoryName: '光谱仪器', model: 'D8 ADVANCE', manufacturer: 'Bruker', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-202', locationName: 'A202 精密仪器室', status: 'normal', description: '晶体结构和物相分析', isActive: true },
      { id: 'ins-balance', instrumentCode: 'INS-BAL-007', instrumentName: '十万分之一分析天平', model: 'XPR205', manufacturer: 'Mettler Toledo', labId: 'lab-central', labName: '中心分析实验室', locationId: 'room-102', locationName: 'A102 样品制备室', status: 'normal', description: '精密称量', isActive: true },
      { id: 'ins-incubator', instrumentCode: 'INS-INC-008', instrumentName: '二氧化碳培养箱', model: 'Heracell 150i', manufacturer: 'Thermo Fisher', labId: 'lab-bio', labName: '生物技术实验室', locationId: 'room-b203', locationName: 'B203 细胞培养室', status: 'stopped', description: '细胞培养设备', isActive: false }
    ],
    bookings: [
      { id: 'booking-1', bookingNo: 'BK-DEMO-001', instrumentId: 'ins-hplc', instrumentName: '高效液相色谱仪', applicantId: 'user-chen', applicantName: '陈曦', groupId: 'group-material', groupName: '先进材料课题组', startTime: at(0, 10), endTime: at(0, 11, 30), purpose: '聚合物样品纯度分析', status: 'approved', approverId: 'user-lin', approveTime: at(-1, 16), approveRemark: '按规范使用流动相', createTime: at(-2, 14) },
      { id: 'booking-2', bookingNo: 'BK-DEMO-002', instrumentId: 'ins-sem', instrumentName: '场发射扫描电子显微镜', applicantId: 'user-zhou', applicantName: '周明', groupId: 'group-bio', groupName: '分子生物课题组', startTime: at(1, 14), endTime: at(1, 16), purpose: '复合膜表面形貌观察', status: 'pending', createTime: at(0, 8, 30) },
      { id: 'booking-3', bookingNo: 'BK-DEMO-003', instrumentId: 'ins-uv', instrumentName: '紫外可见分光光度计', applicantId: 'user-chen', applicantName: '陈曦', groupId: 'group-material', groupName: '先进材料课题组', startTime: at(2, 9), endTime: at(2, 10), purpose: '吸收光谱扫描', status: 'pending', createTime: at(-1, 17) },
      { id: 'booking-4', bookingNo: 'BK-DEMO-004', instrumentId: 'ins-xrd', instrumentName: 'X 射线衍射仪', applicantId: 'user-lin', applicantName: '林妍', groupId: 'group-material', groupName: '先进材料课题组', startTime: at(-1, 9), endTime: at(-1, 11), purpose: '晶相结构分析', status: 'completed', approverId: 'user-admin', approveTime: at(-3, 10), approveRemark: '', createTime: at(-4, 15) }
    ],
    usages: [
      { id: 'usage-1', instrumentId: 'ins-xrd', instrumentName: 'X 射线衍射仪', bookingId: 'booking-4', userId: 'user-lin', userName: '林妍', startTime: at(-1, 9, 5), endTime: at(-1, 10, 48), experimentContent: '钙钛矿薄膜晶相结构测试', remark: '设备运行正常', createTime: at(-1, 11) },
      { id: 'usage-2', instrumentId: 'ins-uv', instrumentName: '紫外可见分光光度计', userId: 'user-chen', userName: '陈曦', startTime: at(-3, 14), endTime: at(-3, 15), experimentContent: '标准溶液吸光度校准', remark: '', createTime: at(-3, 15) }
    ],
    repairs: [
      { id: 'repair-1', repairNo: 'RP-DEMO-001', instrumentId: 'ins-ftir', instrumentName: '傅里叶红外光谱仪', reporterId: 'user-chen', reporterName: '陈曦', faultDescription: '光源能量异常，背景扫描噪声偏高', status: 'repairing', approverId: 'user-lin', approveTime: at(-2, 10), repairer: '设备工程师', repairContent: '检查光源模块并清洁光路', repairStartTime: at(-1, 9), remark: '预计今日完成', createTime: at(-3, 16) },
      { id: 'repair-2', repairNo: 'RP-DEMO-002', instrumentId: 'ins-balance', instrumentName: '十万分之一分析天平', reporterId: 'user-lin', reporterName: '林妍', faultDescription: '称量结果偶发漂移，需要校准', status: 'pending', createTime: at(0, 9) }
    ],
    materials: [
      { id: 'mat-acn', materialCode: 'MAT-R-001', materialName: '色谱纯乙腈', materialType: 'reagent', categoryId: 'cat-solvent', categoryName: '有机溶剂', specification: '4L/瓶', casNo: '75-05-8', unitId: 'unit-ml', unitName: '毫升', supplierId: 'supplier-1', supplierName: '赛默飞世尔科技', storageLocationId: 'room-102', storageLocationName: 'A102 防爆试剂柜', minStock: 5000, currentStock: 7800, description: 'HPLC 流动相', isActive: true },
      { id: 'mat-methanol', materialCode: 'MAT-R-002', materialName: '色谱纯甲醇', materialType: 'reagent', categoryId: 'cat-solvent', categoryName: '有机溶剂', specification: '4L/瓶', casNo: '67-56-1', unitId: 'unit-ml', unitName: '毫升', supplierId: 'supplier-1', supplierName: '赛默飞世尔科技', storageLocationId: 'room-102', storageLocationName: 'A102 防爆试剂柜', minStock: 6000, currentStock: 3200, description: 'HPLC 流动相', isActive: true },
      { id: 'mat-vial', materialCode: 'MAT-C-003', materialName: '色谱进样瓶', materialType: 'consumable', categoryId: 'cat-glass', categoryName: '玻璃器皿', specification: '2mL/100个', unitId: 'unit-piece', unitName: '个', supplierId: 'supplier-2', supplierName: '华科实验物资', storageLocationId: 'room-102', storageLocationName: 'A102 耗材柜', minStock: 200, currentStock: 156, description: '透明螺口进样瓶', isActive: true }
    ],
    batches: [
      { id: 'batch-1', materialId: 'mat-acn', materialName: '色谱纯乙腈', batchNo: 'ACN-260801', productionDate: at(-90).slice(0, 10), expiryDate: at(270).slice(0, 10), inQuantity: 12000, availableQuantity: 7800, unitPrice: 0.18, stockInTime: at(-30, 10), warningStatus: 'normal' },
      { id: 'batch-2', materialId: 'mat-methanol', materialName: '色谱纯甲醇', batchNo: 'MET-260615', productionDate: at(-120).slice(0, 10), expiryDate: at(18).slice(0, 10), inQuantity: 8000, availableQuantity: 3200, unitPrice: 0.12, stockInTime: at(-60, 14), warningStatus: 'expiring' },
      { id: 'batch-3', materialId: 'mat-vial', materialName: '色谱进样瓶', batchNo: 'VIAL-260720', productionDate: at(-70).slice(0, 10), expiryDate: null, inQuantity: 500, availableQuantity: 156, unitPrice: 1.25, stockInTime: at(-45, 9), warningStatus: 'low' }
    ],
    flows: [
      { id: 'flow-1', flowNo: 'FL-DEMO-001', materialId: 'mat-acn', materialName: '色谱纯乙腈', batchId: 'batch-1', batchNo: 'ACN-260801', flowType: 'out', quantity: -1200, beforeQuantity: 9000, afterQuantity: 7800, sourceType: 'requisition', sourceId: 'req-1', operatorName: '林妍', remark: '实验领用', createTime: at(-1, 10) },
      { id: 'flow-2', flowNo: 'FL-DEMO-002', materialId: 'mat-vial', materialName: '色谱进样瓶', batchId: 'batch-3', batchNo: 'VIAL-260720', flowType: 'out', quantity: -44, beforeQuantity: 200, afterQuantity: 156, sourceType: 'requisition', sourceId: 'req-1', operatorName: '林妍', remark: '实验领用', createTime: at(-1, 10) }
    ],
    requisitions: [
      { id: 'req-1', requisitionNo: 'REQ-DEMO-001', applicantName: '陈曦', groupId: 'group-material', groupName: '先进材料课题组', purpose: '聚合物纯度分析实验', status: 'approved', approverName: '林妍', approveTime: at(-1, 10), approveRemark: '按批准数量领用', createTime: at(-2, 9), items: [
        { id: 'req-item-1', materialId: 'mat-acn', materialName: '色谱纯乙腈', unitName: '毫升', requestQuantity: 1200, approvedQuantity: 1200 },
        { id: 'req-item-2', materialId: 'mat-vial', materialName: '色谱进样瓶', unitName: '个', requestQuantity: 50, approvedQuantity: 44 }
      ] },
      { id: 'req-2', requisitionNo: 'REQ-DEMO-002', applicantName: '周明', groupId: 'group-bio', groupName: '分子生物课题组', purpose: '样品前处理与检测', status: 'pending', createTime: at(0, 8), items: [
        { id: 'req-item-3', materialId: 'mat-methanol', materialName: '色谱纯甲醇', unitName: '毫升', requestQuantity: 500 }
      ] }
    ],
    experiments: [
      { id: 'experiment-1', experimentNo: 'EXP-DEMO-001', experimentName: '功能聚合物分子量分布研究', groupId: 'group-material', groupName: '先进材料课题组', ownerId: 'user-chen', ownerName: '陈曦', topicName: '高性能功能聚合物研究', purpose: '分析不同反应条件对聚合物分子量分布的影响', status: 'in_progress', startTime: at(-2, 9), endTime: null, archiveUserId: null, archiveTime: null, createTime: at(-5, 14), instruments: [{ id: 'exp-ins-1', instrumentId: 'ins-hplc', instrumentName: '高效液相色谱仪', bookingId: 'booking-1', bookingNo: 'BK-DEMO-001' }], materials: [{ id: 'exp-mat-1', materialId: 'mat-acn', materialName: '色谱纯乙腈', requisitionId: 'req-1', requisitionNo: 'REQ-DEMO-001', quantity: 1200, unitName: '毫升' }], records: [{ id: 'record-1', recordType: 'observation', content: '样品已完成溶解和过滤，溶液澄清无可见颗粒。', recordTime: at(-1, 11), creatorId: 'user-chen', creatorName: '陈曦' }], files: [{ id: 'file-1', originalName: '聚合物色谱分析结果.xlsx', contentType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', fileSize: 28672, uploaderId: 'user-chen', createTime: at(-1, 16) }] },
      { id: 'experiment-2', experimentNo: 'EXP-DEMO-002', experimentName: '钙钛矿薄膜物相分析', groupId: 'group-material', groupName: '先进材料课题组', ownerId: 'user-lin', ownerName: '林妍', topicName: '新型光电薄膜材料', purpose: '对比不同退火温度下薄膜的晶体结构', status: 'archived', startTime: at(-12, 9), endTime: at(-8, 17), archiveUserId: 'user-lin', archiveTime: at(-7, 10), createTime: at(-14, 10), instruments: [{ id: 'exp-ins-2', instrumentId: 'ins-xrd', instrumentName: 'X 射线衍射仪', bookingId: 'booking-4', bookingNo: 'BK-DEMO-004' }], materials: [], records: [{ id: 'record-2', recordType: 'result', content: '在 150°C 退火条件下获得最高结晶度，未观察到明显杂相峰。', recordTime: at(-8, 16), creatorId: 'user-lin', creatorName: '林妍' }], files: [] }
    ],
    conversations: [{ id: 'conversation-1', title: '色谱结果风险分析', createTime: at(-1, 14), updateTime: at(-1, 14) }],
    messages: [
      { id: 'message-1', conversationId: 'conversation-1', role: 'user', content: '液相色谱基线出现周期性波动，峰面积重复性下降，请分析可能原因。', messageType: 'text', createTime: at(-1, 14) },
      { id: 'message-2', conversationId: 'conversation-1', role: 'assistant', content: '可能与流动相脱气、泵密封或环境温度波动有关。', messageType: 'reasoning', result: { summary: '基线周期性波动并伴随峰面积重复性下降，应优先排查输液系统稳定性。', facts: ['基线存在周期性波动', '峰面积重复性下降'], inferences: ['流动相可能脱气不足', '泵密封或单向阀可能存在异常'], risks: ['继续测试可能导致定量结果失真'], suggestions: ['重新脱气并更换流动相', '执行泵压力稳定性测试', '使用标准样连续进样确认重复性'], missingInformation: ['压力曲线', '波动周期', '柱温箱温度记录'], confidence: 0.86 }, createTime: at(-1, 14, 1) }
    ],
    scenes: [{
      scene: { id: 'scene-central', labId: 'lab-central', labName: '中心分析实验室', sceneName: '科研 A 栋数字空间', backgroundColor: '#e9f0f2', version: 2, isActive: true },
      nodes: [
        { id: 'node-101', code: 'A101', name: '光谱分析室', type: 'location', x: -3, y: 0.7, z: -2, scaleX: 2, scaleY: 1, scaleZ: 1.4, businessType: 'location', businessId: 'room-101', status: 'repair', detail: '科研 A 栋 / 一层 / 光谱分析室', floor: 1, upcomingBookingCount: 1, repairingInstrumentCount: 1 },
        { id: 'node-201', code: 'A201', name: '色谱分析室', type: 'location', x: 2, y: 3.9, z: -2, scaleX: 2, scaleY: 1, scaleZ: 1.4, businessType: 'location', businessId: 'room-201', status: 'normal', detail: '科研 A 栋 / 二层 / 色谱分析室', floor: 2, upcomingBookingCount: 2, repairingInstrumentCount: 0, nextBookingTime: at(0, 10) },
        { id: 'node-301', code: 'A301', name: '电子显微镜室', type: 'location', x: 0, y: 7.1, z: -2, scaleX: 2, scaleY: 1, scaleZ: 1.4, businessType: 'location', businessId: 'room-301', status: 'normal', detail: '科研 A 栋 / 三层 / 电子显微镜室', floor: 3, upcomingBookingCount: 1, repairingInstrumentCount: 0, nextBookingTime: at(1, 14) }
      ],
      models: [{ fileId: 'model-2', fileName: 'building-a-v2.glb', fileSize: 1842000, uploaderId: 'user-admin', createTime: at(-3, 11), isCurrent: true }, { fileId: 'model-1', fileName: 'building-a-v1.glb', fileSize: 1655000, uploaderId: 'user-admin', createTime: at(-10, 9), isCurrent: false }],
      createTime: at(-15, 9),
      updateTime: at(-3, 11)
    }]
  };
}
