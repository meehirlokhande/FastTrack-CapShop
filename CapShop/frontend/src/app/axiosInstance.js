import axios from "axios";

const api = axios.create({
  baseURL: "/gateway",
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    const url = err.config?.url ?? "";
    const is2faEndpoint = url.includes("/auth/2fa/verify") || url.includes("/auth/2fa/resend");

    if (err.response?.status === 401 && !is2faEndpoint) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(err);
  }
);

export default api;
