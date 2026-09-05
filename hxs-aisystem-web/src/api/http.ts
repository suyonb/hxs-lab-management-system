import axios from 'axios';
import { message } from 'ant-design-vue';
import router from '../router';
import { useAuthStore } from '../stores/auth';
import { demoAdapter } from '../demo/adapter';
import { isDemoMode } from '../demo/mode';

declare module 'axios' {
  export interface AxiosRequestConfig {
    silentError?: boolean;
  }

  export interface InternalAxiosRequestConfig {
    silentError?: boolean;
  }
}

export const http = axios.create({
  baseURL: '',
  timeout: 15000,
  ...(isDemoMode ? { adapter: demoAdapter } : {})
});

http.interceptors.request.use((config) => {
  const authStore = useAuthStore();
  if (authStore.accessToken) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${authStore.accessToken}`;
  }
  return config;
});

http.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    const text = error?.response?.data?.message ?? '请求失败，请稍后重试。';
    if (status === 401) {
      useAuthStore().clearSession();
      message.warning(text);
      router.replace('/login');
    } else if (!error?.config?.silentError) {
      message.error(text);
    }
    return Promise.reject(error);
  }
);
