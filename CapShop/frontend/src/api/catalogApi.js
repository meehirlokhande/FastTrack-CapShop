import api from "../app/axiosInstance";

export const catalogApi = {
  getCategories: () => api.get("/catalog/categories"),
  getProducts: (params) => api.get("/catalog/products", { params }),
  getFeaturedProducts: () => api.get("/catalog/products/featured"),
  getProductById: (id) => api.get(`/catalog/products/${id}`),
};
