<template>
  <div class="reasoning-result">
    <div class="reasoning-result__summary">
      <span>核心结论</span>
      <strong>{{ result.summary }}</strong>
      <div class="confidence-line">
        <span>置信度</span>
        <a-progress :percent="confidence" :show-info="false" size="small" />
        <b>{{ confidence }}%</b>
      </div>
    </div>
    <div class="reasoning-result__grid">
      <ResultSection title="已识别事实" tone="fact" :items="result.facts" />
      <ResultSection title="合理推测" tone="inference" :items="result.inferences" />
      <ResultSection title="风险关注" tone="risk" :items="result.risks" />
      <ResultSection title="建议行动" tone="suggestion" :items="result.suggestions" />
    </div>
    <ResultSection title="信息缺口" tone="missing" :items="result.missingInformation" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { ReasoningResult } from '../../../types/ai';
import ResultSection from './ResultSection.vue';

const props = defineProps<{ result: ReasoningResult }>();
const confidence = computed(() => Math.round(Math.max(0, Math.min(1, props.result.confidence)) * 100));
</script>
