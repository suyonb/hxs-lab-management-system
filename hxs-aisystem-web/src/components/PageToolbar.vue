<template>
  <div class="page-toolbar">
    <div class="page-toolbar__actions">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  eyebrow: string;
  title: string;
}>();
</script>
