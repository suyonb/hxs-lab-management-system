import { http } from './http';
import type { AiConversation, AiMessage, ReasoningResponse } from '../types/ai';

export const aiApi = {
  conversations: () => http.get<AiConversation[]>('/api/ai/conversations').then((res) => res.data),
  createConversation: (title?: string) => http.post<AiConversation>('/api/ai/conversations', { title }).then((res) => res.data),
  messages: (id: string) => http.get<AiMessage[]>(`/api/ai/conversations/${id}/messages`).then((res) => res.data),
  reason: (id: string, content: string) => http.post<ReasoningResponse>(`/api/ai/conversations/${id}/reason`, { content }).then((res) => res.data),
  removeConversation: (id: string) => http.delete(`/api/ai/conversations/${id}`)
};
