export interface AiConversation {
  id: string;
  title?: string | null;
  createTime: string;
  updateTime: string;
}

export interface ReasoningResult {
  summary: string;
  facts: string[];
  inferences: string[];
  risks: string[];
  suggestions: string[];
  missingInformation: string[];
  confidence: number;
}

export interface AiMessage {
  id: string;
  conversationId: string;
  role: 'user' | 'assistant';
  content: string;
  messageType: 'text' | 'reasoning';
  result?: ReasoningResult | null;
  createTime: string;
}

export interface ReasoningResponse {
  userMessage: AiMessage;
  assistantMessage: AiMessage;
  result: ReasoningResult;
  provider: string;
}
