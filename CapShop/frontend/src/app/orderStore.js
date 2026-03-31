import { create } from "zustand";
import { orderApi } from "../api/orderApi";

export const useOrderStore = create((set) => ({
  orders: [],
  currentOrder: null,
  loading: false,

  fetchMyOrders: async () => {
    set({ loading: true });
    try {
      const { data } = await orderApi.getMyOrders();
      set({ orders: data });
    } finally {
      set({ loading: false });
    }
  },

  fetchOrderById: async (id) => {
    set({ loading: true });
    try {
      const { data } = await orderApi.getOrderById(id);
      set({ currentOrder: data });
    } finally {
      set({ loading: false });
    }
  },

  checkout: async (payload) => {
    const { data } = await orderApi.checkout(payload);
    return data;
  },

  simulatePayment: async (orderId, paymentMethod, amount) => {
    const { data } = await orderApi.simulatePayment({ orderId, paymentMethod, amount });
    return data;
  },

  cancelOrder: async (id) => {
    await orderApi.cancelOrder(id);
  },
}));
