import { create } from "zustand";
import { catalogApi } from "../api/catalogApi";

export const useProductStore = create((set) => ({
  products: [],
  totalCount: 0,
  page: 1,
  pageSize: 12,
  totalPages: 0,
  featured: [],
  categories: [],
  loading: false,

  fetchProducts: async (params) => {
    set({ loading: true });
    try {
      const { data } = await catalogApi.getProducts(params);
      set({
        products: data.items,
        totalCount: data.totalCount,
        page: data.page,
        pageSize: data.pageSize,
        totalPages: data.totalPages,
      });
    } finally {
      set({ loading: false });
    }
  },

  fetchFeatured: async () => {
    const { data } = await catalogApi.getFeaturedProducts();
    set({ featured: data });
  },

  fetchCategories: async () => {
    const { data } = await catalogApi.getCategories();
    set({ categories: data });
  },
}));
