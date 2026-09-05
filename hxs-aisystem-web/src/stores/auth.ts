import { defineStore } from 'pinia';
import type { LoginResponse, UserProfile } from '../types/system';

const tokenKey = 'hxs-access-token';
const userKey = 'hxs-user-profile';
const permissionKey = 'hxs-user-permissions';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: localStorage.getItem(tokenKey) ?? '',
    currentUser: readUser(),
    permissions: readPermissions()
  }),
  actions: {
    setSession(response: LoginResponse) {
      this.accessToken = response.accessToken;
      this.currentUser = response.user;
      localStorage.setItem(tokenKey, response.accessToken);
      localStorage.setItem(userKey, JSON.stringify(response.user));
    },
    clearSession() {
      this.accessToken = '';
      this.currentUser = null;
      this.permissions = [];
      localStorage.removeItem(tokenKey);
      localStorage.removeItem(userKey);
      localStorage.removeItem(permissionKey);
    },
    setPermissions(values: string[]) {
      this.permissions = [...new Set(values)];
      localStorage.setItem(permissionKey, JSON.stringify(this.permissions));
    },
    hasPermission(permission: string) {
      return this.permissions.includes(permission);
    }
  }
});

function readUser(): UserProfile | null {
  const raw = localStorage.getItem(userKey);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as UserProfile;
  } catch {
    return null;
  }
}

function readPermissions(): string[] {
  const raw = localStorage.getItem(permissionKey);
  if (!raw) return [];
  try {
    const values = JSON.parse(raw);
    return Array.isArray(values) ? values.filter((item): item is string => typeof item === 'string') : [];
  } catch {
    return [];
  }
}
