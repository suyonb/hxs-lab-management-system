import {http} from './http';
import type {Lab3dModelVersion,Lab3dNode,Lab3dNodeRequest,Lab3dNodeStatus,Lab3dScene,Lab3dSceneDetail,Lab3dSceneManage,Lab3dSceneRequest,LabSpatialLab,LabSpatialStatus} from '../types/visualization';

const base='/api/lab/3d';
export const visualizationApi={
  layout:()=>http.get<LabSpatialLab[]>(`${base}/layout`).then(r=>r.data),
  spatialStatuses:(labId:string)=>http.get<LabSpatialStatus[]>(`${base}/layout/${labId}/statuses`).then(r=>r.data),
  scenes:()=>http.get<Lab3dScene[]>(`${base}/scenes`).then(r=>r.data),
  scene:(id:string,silentError=false)=>http.get<Lab3dSceneDetail>(`${base}/scenes/${id}`,{silentError}).then(r=>r.data),
  statuses:(id:string)=>http.get<Lab3dNodeStatus[]>(`${base}/scenes/${id}/statuses`).then(r=>r.data),
  manageScenes:(silentError=false)=>http.get<Lab3dSceneManage[]>(`${base}/manage/scenes`,{silentError}).then(r=>r.data),
  createScene:(data:Lab3dSceneRequest)=>http.post<Lab3dSceneManage>(`${base}/manage/scenes`,data).then(r=>r.data),
  updateScene:(id:string,data:Lab3dSceneRequest)=>http.put<Lab3dSceneManage>(`${base}/manage/scenes/${id}`,data).then(r=>r.data),
  removeScene:(id:string)=>http.delete(`${base}/manage/scenes/${id}`),
  uploadModel:async(id:string,file:File)=>{const data=new FormData();data.append('file',file);return http.post(`${base}/manage/scenes/${id}/model`,data,{headers:{'Content-Type':'multipart/form-data'}}).then(r=>r.data);},
  modelVersions:(id:string,silentError=false)=>http.get<Lab3dModelVersion[]>(`${base}/manage/scenes/${id}/models`,{silentError}).then(r=>r.data),
  activateModel:(id:string,fileId:string)=>http.put(`${base}/manage/scenes/${id}/models/${fileId}/activate`),
  createNode:(sceneId:string,data:Lab3dNodeRequest)=>http.post<Lab3dNode>(`${base}/manage/scenes/${sceneId}/nodes`,data).then(r=>r.data),
  updateNode:(id:string,data:Lab3dNodeRequest)=>http.put<Lab3dNode>(`${base}/manage/nodes/${id}`,data).then(r=>r.data),
  removeNode:(id:string)=>http.delete(`${base}/manage/nodes/${id}`),
  setBinding:(id:string,data:{businessType:string;businessId:string})=>http.put<Lab3dNode>(`${base}/manage/nodes/${id}/binding`,data).then(r=>r.data),
  removeBinding:(id:string)=>http.delete(`${base}/manage/nodes/${id}/binding`)
};
