<template>
  <main class="login-page">
    <section class="login-visual" aria-label="登录页动画背景">
      <div class="login-orbit" aria-hidden="true">
        <div class="login-tesseract">
          <span class="login-tesseract__face login-tesseract__face--front"></span>
          <span class="login-tesseract__face login-tesseract__face--back"></span>
          <span class="login-tesseract__edge login-tesseract__edge--one"></span>
          <span class="login-tesseract__edge login-tesseract__edge--two"></span>
          <span class="login-tesseract__edge login-tesseract__edge--three"></span>
          <span class="login-tesseract__edge login-tesseract__edge--four"></span>
        </div>
        <span class="login-orbit__ring login-orbit__ring--outer"></span>
        <span class="login-orbit__ring login-orbit__ring--middle"></span>
        <span class="login-orbit__ring login-orbit__ring--inner"></span>
        <span class="login-orbit__core"></span>
        <span class="login-orbit__dot login-orbit__dot--one"></span>
        <span class="login-orbit__dot login-orbit__dot--two"></span>
        <span class="login-orbit__dot login-orbit__dot--three"></span>
      </div>
      <div class="login-lines" aria-hidden="true">
        <i v-for="item in 18" :key="item"></i>
      </div>
    </section>
    <section class="login-panel">
      <a-card class="login-card" :bordered="false">
        <div class="login-card__head">
          <p>Welcome back</p>
          <h2>实验室系统</h2>
        </div>
        <a-form layout="vertical" :model="form" @finish="submit">
          <a-form-item label="用户名" name="userName" :rules="[{ required: true, message: '请输入用户名' }]">
            <a-input v-model:value="form.userName" size="large" placeholder="请输入用户名">
              <template #prefix><UserOutlined /></template>
            </a-input>
          </a-form-item>
          <a-form-item label="密码" name="password" :rules="[{ required: true, message: '请输入密码' }]">
            <a-input-password v-model:value="form.password" size="large" placeholder="请输入密码">
              <template #prefix><LockOutlined /></template>
            </a-input-password>
          </a-form-item>
          <a-button type="primary" html-type="submit" size="large" block :loading="loading">
            <span>进入系统</span>
            <ArrowRightOutlined />
          </a-button>
        </a-form>
        <div class="theme-strip">
          <button
            v-for="item in themes"
            :key="item.key"
            type="button"
            :title="item.description"
            :class="{ active: item.key === activeThemeKey }"
            @click="setTheme(item.key)"
          >
            <i :style="{ background: item.primary }"></i>
            {{ item.name }}
          </button>
        </div>
      </a-card>
    </section>
  </main>
</template>

<script setup lang="ts">
import { ArrowRightOutlined, LockOutlined, UserOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { login } from '../../api/system';
import { useAuthStore } from '../../stores/auth';
import { themes, useThemeStore } from '../../stores/theme';

const router = useRouter();
const authStore = useAuthStore();
const themeStore = useThemeStore();
const loading = ref(false);
const form = reactive({ userName: 'admin', password: 'Admin@123456' });
const activeThemeKey = computed(() => themeStore.activeThemeKey);
const setTheme = themeStore.setTheme;

async function submit() {
  loading.value = true;
  try {
    const response = await login(form);
    authStore.setSession(response);
    message.success('登录成功');
    router.replace('/');
  } finally {
    loading.value = false;
  }
}
</script>
