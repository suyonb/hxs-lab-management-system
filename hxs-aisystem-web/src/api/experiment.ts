import { http } from './http';
import type { ExperimentDto,ExperimentRecordDto,ExperimentRequest } from '../types/experiment';
const base='/api/lab/experiments';
export const experimentApi={
  list:(params:Record<string,unknown>={})=>http.get<ExperimentDto[]>(base,{params}).then(r=>r.data),
  detail:(id:string)=>http.get<ExperimentDto>(`${base}/${id}`).then(r=>r.data),
  create:(data:ExperimentRequest)=>http.post<ExperimentDto>(base,data).then(r=>r.data),
  update:(id:string,data:ExperimentRequest)=>http.put(`${base}/${id}`,data),
  start:(id:string)=>http.post(`${base}/${id}/start`),complete:(id:string)=>http.post(`${base}/${id}/complete`),
  reopen:(id:string,reason:string)=>http.post(`${base}/${id}/reopen`,{reason}),archive:(id:string)=>http.post(`${base}/${id}/archive`),unarchive:(id:string,reason:string)=>http.post(`${base}/${id}/unarchive`,{reason}),
  addRecord:(id:string,data:Record<string,unknown>)=>http.post<ExperimentRecordDto>(`${base}/${id}/records`,data).then(r=>r.data),
  upload:async(id:string,file:File)=>{const data=new FormData();data.append('file',file);data.append('businessType','experiment');data.append('businessId',id);return http.post('/api/files',data,{headers:{'Content-Type':'multipart/form-data'}}).then(r=>r.data);},
  download:async(id:string,name:string)=>{const r=await http.get(`/api/files/${id}`,{responseType:'blob'});const url=URL.createObjectURL(r.data);const a=document.createElement('a');a.href=url;a.download=name;a.click();URL.revokeObjectURL(url);}
};
