import api from "../app/axiosInstance";

export const authApi = {
  signup: (payload) => api.post("/auth/signup", payload),
  login: (payload) => api.post("/auth/login", payload),
  me: () => api.get("/auth/me"),

  getProfile: () => api.get("/auth/profile"),
  updateProfile: (payload) => api.put("/auth/profile", payload),
  changePassword: (payload) => api.put("/auth/profile/password", payload),
  uploadAvatar: (formData) => api.post("/auth/profile/avatar", formData),
  removeAvatar: () => api.delete("/auth/profile/avatar"),

  // 2FA — login flow (no token required)
  verifyTwoFactor: (payload) => api.post("/auth/2fa/verify", payload),
  resendTwoFactor: (tempToken) => api.post("/auth/2fa/resend", { tempToken }),

  // 2FA — setup (requires full JWT, called from account settings)
  getTwoFactorStatus: () => api.get("/auth/2fa/status"),
  setupTotp: () => api.get("/auth/2fa/setup/totp"),
  confirmTotp: (code) => api.post("/auth/2fa/confirm/totp", { code }),
  setupEmail: () => api.post("/auth/2fa/setup/email"),
  confirmEmail: (code) => api.post("/auth/2fa/confirm/email", { code }),
  setupSms: () => api.post("/auth/2fa/setup/sms"),
  confirmSms: (code) => api.post("/auth/2fa/confirm/sms", { code }),
  disableTwoFactor: () => api.post("/auth/2fa/disable"),
};
