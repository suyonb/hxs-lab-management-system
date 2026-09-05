<template>
  <div class="reasoning-workbench">
    <aside class="reasoning-sidebar page-surface">
      <div class="reasoning-sidebar__head">
        <div>
          <span>Reasoning</span>
          <strong>推理会话</strong>
        </div>
        <a-button type="primary" shape="circle" title="新建会话" @click="createConversation">
          <PlusOutlined />
        </a-button>
      </div>

      <div class="conversation-list">
        <button
          v-for="item in conversations"
          :key="item.id"
          class="conversation-item"
          :class="{ 'conversation-item--active': item.id === activeConversationId }"
          @click="selectConversation(item.id)"
        >
          <MessageOutlined />
          <span>
            <strong>{{ item.title || '新推理会话' }}</strong>
            <small>{{ formatTime(item.updateTime) }}</small>
          </span>
          <a-popconfirm title="删除该会话及全部消息？" @confirm.stop="removeConversation(item.id)">
            <DeleteOutlined class="conversation-item__delete" @click.stop />
          </a-popconfirm>
        </button>

        <a-empty v-if="!conversationLoading && !conversations.length" description="暂无推理会话" />
        <a-spin v-if="conversationLoading" />
      </div>
    </aside>

    <section class="reasoning-main page-surface">
      <div class="reasoning-main__head">
        <div>
          <span class="reasoning-kicker">结构化分析</span>
          <h2>{{ activeConversation?.title || '数据推理' }}</h2>
        </div>
        <a-tag :color="lastProvider === 'demo' ? 'gold' : 'green'">
          {{ lastProvider === 'demo' ? 'Demo 分析' : 'AI 模型' }}
        </a-tag>
      </div>

      <div ref="messageScroller" class="reasoning-messages">
        <div v-if="!messages.length && !messageLoading" class="reasoning-empty">
          <BulbOutlined />
          <h3>输入一段数据或业务描述</h3>
          <p>系统将提取事实、推测、风险、建议和信息缺口。</p>
          <div class="reasoning-examples">
            <button v-for="example in examples" :key="example" @click="input = example">{{ example }}</button>
          </div>
        </div>

        <a-spin v-if="messageLoading" />

        <article v-for="item in messages" :key="item.id" class="reasoning-message" :class="`reasoning-message--${item.role}`">
          <div class="reasoning-message__identity">
            <UserOutlined v-if="item.role === 'user'" />
            <BulbOutlined v-else />
            <span>{{ item.role === 'user' ? '我的输入' : '推理结果' }}</span>
            <small>{{ formatTime(item.createTime) }}</small>
          </div>
          <p v-if="item.role === 'user'" class="reasoning-message__text">{{ item.content }}</p>
          <ReasoningResultCard v-else-if="item.result" :result="item.result" />
          <p v-else class="reasoning-message__text">{{ item.content }}</p>
        </article>
      </div>

      <div class="reasoning-composer">
        <a-textarea
          v-model:value="input"
          :auto-size="{ minRows: 3, maxRows: 7 }"
          :maxlength="12000"
          placeholder="输入需要分析的数据、现象或业务描述…"
          @keydown.ctrl.enter.prevent="send"
          @keydown.meta.enter.prevent="send"
        />
        <div class="reasoning-composer__footer">
          <span>{{ input.length }} / 12000</span>
          <a-button type="primary" :loading="sending" :disabled="!input.trim()" @click="send">
            <SendOutlined />
            开始推理
          </a-button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { BulbOutlined, DeleteOutlined, MessageOutlined, PlusOutlined, SendOutlined, UserOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { computed, nextTick, onMounted, ref } from 'vue';
import { aiApi } from '../../api/ai';
import type { AiConversation, AiMessage } from '../../types/ai';
import ReasoningResultCard from './components/ReasoningResultCard.vue';

const conversations = ref<AiConversation[]>([]);
const messages = ref<AiMessage[]>([]);
const activeConversationId = ref<string>();
const conversationLoading = ref(false);
const messageLoading = ref(false);
const sending = ref(false);
const input = ref('');
const lastProvider = ref('demo');
const messageScroller = ref<HTMLElement>();
const activeConversation = computed(() => conversations.value.find((item) => item.id === activeConversationId.value));
const examples = [
  '华东区域本月销售额下降12%，其中江苏下降最明显，但客户访问量增长了20%。',
  '项目原计划月底上线，目前核心接口完成80%，测试发现3个高风险问题，可能延期一周。'
];

onMounted(loadConversations);

async function loadConversations() {
  conversationLoading.value = true;
  try {
    conversations.value = await aiApi.conversations();
    if (conversations.value.length) await selectConversation(conversations.value[0].id);
  } finally {
    conversationLoading.value = false;
  }
}

async function createConversation() {
  const item = await aiApi.createConversation();
  conversations.value.unshift(item);
  activeConversationId.value = item.id;
  messages.value = [];
}

async function selectConversation(id: string) {
  activeConversationId.value = id;
  messageLoading.value = true;
  try {
    messages.value = await aiApi.messages(id);
    await scrollToBottom();
  } finally {
    messageLoading.value = false;
  }
}

async function send() {
  const content = input.value.trim();
  if (!content || sending.value) return;
  sending.value = true;
  try {
    if (!activeConversationId.value) await createConversation();
    const response = await aiApi.reason(activeConversationId.value!, content);
    messages.value.push(response.userMessage, response.assistantMessage);
    lastProvider.value = response.provider;
    input.value = '';
    await refreshConversations();
    await scrollToBottom();
  } finally {
    sending.value = false;
  }
}

async function removeConversation(id: string) {
  await aiApi.removeConversation(id);
  conversations.value = conversations.value.filter((item) => item.id !== id);
  if (activeConversationId.value === id) {
    activeConversationId.value = conversations.value[0]?.id;
    messages.value = activeConversationId.value ? await aiApi.messages(activeConversationId.value) : [];
  }
  message.success('会话已删除');
}

async function refreshConversations() {
  conversations.value = await aiApi.conversations();
}

async function scrollToBottom() {
  await nextTick();
  if (messageScroller.value) messageScroller.value.scrollTop = messageScroller.value.scrollHeight;
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' }).format(new Date(value));
}
</script>
