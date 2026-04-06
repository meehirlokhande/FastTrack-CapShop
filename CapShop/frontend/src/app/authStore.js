import { create } from "zustand";
import { authApi } from "../api/authApi";

export const useAuthStore = create((set, get) => ({
  user: null,
  token: localStorage.getItem("token"),
  role: localStorage.getItem("role"),
  isLoading: true,

  // Holds state when backend returns requires2FA: true
  pendingTwoFactor: null, // { tempToken, method }

  login: async (email, password) => {
    const { data } = await authApi.login({ email, password });

    if (data.requiresTwoFactor) {
      set({ pendingTwoFactor: { tempToken: data.tempToken, method: data.twoFactorMethod } });
      return { requiresTwoFactor: true, method: data.twoFactorMethod };
    }

    localStorage.setItem("token", data.token);
    localStorage.setItem("role", data.role);
    set({ token: data.token, role: data.role, pendingTwoFactor: null });
    await get().fetchUser();
    return { requiresTwoFactor: false, role: data.role };
  },

  verifyTwoFactor: async (code) => {
    const pending = get().pendingTwoFactor;
    if (!pending) throw new Error("No pending 2FA session.");

    const { data } = await authApi.verifyTwoFactor({ tempToken: pending.tempToken, code });
    localStorage.setItem("token", data.token);
    localStorage.setItem("role", data.role);
    set({ token: data.token, role: data.role, pendingTwoFactor: null });
    await get().fetchUser();
    return data.role;
  },

  resendTwoFactor: async () => {
    const pending = get().pendingTwoFactor;
    if (!pending) throw new Error("No pending 2FA session.");
    await authApi.resendTwoFactor(pending.tempToken);
  },

  cancelTwoFactor: () => {
    set({ pendingTwoFactor: null });
  },

  signup: async (payload) => {
    const { data } = await authApi.signup(payload);
    return data.message;
  },

  fetchUser: async () => {
    try {
      const { data } = await authApi.getProfile();
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
    set({ user: null, token: null, role: null, pendingTwoFactor: null });
  },
}));
