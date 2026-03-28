import api from "../app/axiosInstance";

export const adminApi = {
  getDashboard: () => api.get("/admin/dashboard/summary"),

  getProducts: () => api.get("/admin/products"),
  getProductById: (id) => api.get(`/admin/products/${id}`),
  createProduct: (payload) => api.post("/admin/products", payload),
  updateProduct: (id, payload) => api.put(`/admin/products/${id}`, payload),
  deleteProduct: (id) => api.delete(`/admin/products/${id}`),
  toggleProductStatus: (id) => api.put(`/admin/products/${id}/toggle-status`),
  updateStock: (id, payload) => api.put(`/admin/products/${id}/stock`, payload),

  getOrders: () => api.get("/admin/orders"),
  updateOrderStatus: (id, payload) =>
    api.put(`/admin/orders/${id}/status`, payload),

  getSalesReport: (from, to) =>
    api.get("/admin/reports/sales", { params: { from, to } }),
  getStatusSplit: () => api.get("/admin/reports/status-split"),

  getAdminCategories: () => api.get("/admin/categories"),
  createCategory: (payload) => api.post("/admin/categories", payload),
  deleteCategory: (id) => api.delete(`/admin/categories/${id}`),
};
