import { http } from './http';
import type { BookingDto, InstrumentDto, RepairDto, UsageDto } from '../types/instrument';
const base = '/api/lab/instruments';
export const instrumentApi = {
  instruments: (availableOnly=false,silentError=false) => http.get<InstrumentDto[]>(base,{params:{availableOnly},silentError}).then(r=>r.data),
  createInstrument: (data:Partial<InstrumentDto>) => http.post<InstrumentDto>(base,data).then(r=>r.data),
  updateInstrument: (id:string,data:Partial<InstrumentDto>) => http.put(`${base}/${id}`,data),
  bookings: (mine=false,status?:string,silentError=false) => http.get<BookingDto[]>(`${base}/bookings`,{params:{mine,status},silentError}).then(r=>r.data),
  createBooking: (data:Record<string,unknown>) => http.post(`${base}/bookings`,data),
  cancelBooking: (id:string) => http.post(`${base}/bookings/${id}/cancel`),
  approveBooking: (id:string,remark='') => http.post(`${base}/bookings/${id}/approve`,{remark}),
  rejectBooking: (id:string,remark='') => http.post(`${base}/bookings/${id}/reject`,{remark}),
  completeBooking: (id:string) => http.post(`${base}/bookings/${id}/complete`),
  usages: (mine=false,silentError=false) => http.get<UsageDto[]>(`${base}/usages`,{params:{mine},silentError}).then(r=>r.data),
  createUsage: (data:Record<string,unknown>) => http.post(`${base}/usages`,data),
  repairs: (mine=false,status?:string,silentError=false) => http.get<RepairDto[]>(`${base}/repairs`,{params:{mine,status},silentError}).then(r=>r.data),
  createRepair: (data:Record<string,unknown>) => http.post(`${base}/repairs`,data),
  approveRepair: (id:string,remark='') => http.post(`${base}/repairs/${id}/approve`,{remark}),
  rejectRepair: (id:string,remark='') => http.post(`${base}/repairs/${id}/reject`,{remark}),
  startRepair: (id:string,data:Record<string,unknown>) => http.post(`${base}/repairs/${id}/start`,data),
  completeRepair: (id:string,data:Record<string,unknown>) => http.post(`${base}/repairs/${id}/complete`,data)
};
