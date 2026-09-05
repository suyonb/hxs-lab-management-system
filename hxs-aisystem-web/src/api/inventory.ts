import { http } from './http';
import type { InventoryWarningDto,MaterialDto,RequisitionDto,StockBatchDto,StockFlowDto } from '../types/inventory';
const base='/api/lab/inventory';
export const inventoryApi={
  materials:(enabledOnly=false)=>http.get<MaterialDto[]>(`${base}/materials`,{params:{enabledOnly}}).then(r=>r.data),
  createMaterial:(data:Record<string,unknown>)=>http.post(`${base}/materials`,data),updateMaterial:(id:string,data:Record<string,unknown>)=>http.put(`${base}/materials/${id}`,data),
  batches:(materialId?:string)=>http.get<StockBatchDto[]>(`${base}/batches`,{params:{materialId}}).then(r=>r.data),stockIn:(data:Record<string,unknown>)=>http.post(`${base}/batches`,data),adjust:(id:string,data:Record<string,unknown>)=>http.post(`${base}/batches/${id}/adjust`,data),
  flows:(materialId?:string)=>http.get<StockFlowDto[]>(`${base}/flows`,{params:{materialId}}).then(r=>r.data),
  requisitions:(mine=false,status?:string)=>http.get<RequisitionDto[]>(`${base}/requisitions`,{params:{mine,status}}).then(r=>r.data),createRequisition:(data:Record<string,unknown>)=>http.post(`${base}/requisitions`,data),cancel:(id:string)=>http.post(`${base}/requisitions/${id}/cancel`),approve:(id:string,data:Record<string,unknown>)=>http.post(`${base}/requisitions/${id}/approve`,data),reject:(id:string,remark='')=>http.post(`${base}/requisitions/${id}/reject`,{remark}),
  warnings:(expiryDays=30)=>http.get<InventoryWarningDto[]>(`${base}/warnings`,{params:{expiryDays}}).then(r=>r.data)
};
