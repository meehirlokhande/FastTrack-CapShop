import api from "../app/axiosInstance";

export const orderApi = {
  getCart: () => api.get("/orders/cart"),
  addCartItem: (payload) => api.post("/orders/cart/items", payload),
  updateCartItem: (id, payload) => api.put(`/orders/cart/items/${id}`, payload),
  removeCartItem: (id) => api.delete(`/orders/cart/items/${id}`),

  checkout: (payload) => api.post("/orders/checkout", payload),
  simulatePayment: (payload) => api.post("/orders/payment/simulate", payload),

  getMyOrders: () => api.get("/orders/my"),
  getOrderById: (id) => api.get(`/orders/${id}`),
  cancelOrder: (id) => api.put(`/orders/${id}/cancel`),
};
