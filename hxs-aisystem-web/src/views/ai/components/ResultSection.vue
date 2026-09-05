<template>
  <section class="result-section" :class="`result-section--${tone}`">
    <h4>{{ title }}</h4>
    <ul>
      <li v-for="item in items" :key="item">{{ item }}</li>
      <li v-if="!items.length">暂未识别</li>
    </ul>
  </section>
</template>

<script setup lang="ts">
defineProps<{ title: string; tone: 'fact' | 'inference' | 'risk' | 'suggestion' | 'missing'; items: string[] }>();
</script>
