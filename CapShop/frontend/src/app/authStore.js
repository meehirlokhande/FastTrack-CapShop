import { create } from "zustand";
import { authApi } from "../api/authApi";

export const useAuthStore = create((set, get) => ({
  user: null,
  token: localStorage.getItem("token"),
  role: localStorage.getItem("role"),
  isLoading: true,

  login: async (email, password) => {
    const { data } = await authApi.login({ email, password });
    localStorage.setItem("token", data.token);
    localStorage.setItem("role", data.role);
    set({ token: data.token, role: data.role });
    await get().fetchUser();
    return data.role;
  },

  signup: async (payload) => {
    const { data } = await authApi.signup(payload);
    return data.message;
  },

  fetchUser: async () => {
    try {
      const { data } = await authApi.me();
      set({ user: data, role: data.role, isLoading: false });
    } catch {
      set({ user: null, isLoading: false });
    }
  },

  hydrate: async () => {
    const token = localStorage.getItem("token");
    if (!token) {
      set({ isLoading: false });
      return;
    }
    await get().fetchUser();
  },

  logout: () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    set({ user: null, token: null, role: null });
  },
}));
