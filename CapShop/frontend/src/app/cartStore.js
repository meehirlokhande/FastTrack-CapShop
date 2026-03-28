import { create } from "zustand";
import { orderApi } from "../api/orderApi";

export const useCartStore = create((set) => ({
  items: [],
  total: 0,
  itemCount: 0,
  loading: false,

  fetchCart: async () => {
    set({ loading: true });
    try {
      const { data } = await orderApi.getCart();
      set({ items: data.items, total: data.total, itemCount: data.itemCount });
    } catch {
      set({ items: [], total: 0, itemCount: 0 });
    } finally {
      set({ loading: false });
    }
  },

  addItem: async (product, quantity = 1) => {
    await orderApi.addCartItem({
      productId: product.id,
      productName: product.name,
      price: product.discountPrice ?? product.price,
      imageUrl: product.imageUrl,
      quantity,
    });
    const { data } = await orderApi.getCart();
    set({ items: data.items, total: data.total, itemCount: data.itemCount });
  },

  updateQuantity: async (itemId, quantity) => {
    await orderApi.updateCartItem(itemId, { quantity });
    const { data } = await orderApi.getCart();
    set({ items: data.items, total: data.total, itemCount: data.itemCount });
  },

  removeItem: async (itemId) => {
    await orderApi.removeCartItem(itemId);
    const { data } = await orderApi.getCart();
    set({ items: data.items, total: data.total, itemCount: data.itemCount });
  },

  clearLocal: () => set({ items: [], total: 0, itemCount: 0 }),
}));
