import { useEffect, useState, useCallback } from "react";
import { useSearchParams } from "react-router-dom";
import { useProductStore } from "../app/productStore";
import ProductCard from "../components/product/ProductCard";
import ProductFilters from "../components/product/ProductFilters";
import Pagination from "../components/ui/Pagination";
import Spinner from "../components/ui/Spinner";

export default function ShopPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { products, totalPages, page, loading, fetchProducts, fetchCategories, categories } =
    useProductStore();

  const [filters, setFilters] = useState({
    query: searchParams.get("query") || "",
    category: searchParams.get("category") || "",
    minPrice: searchParams.get("minPrice") || undefined,
    maxPrice: searchParams.get("maxPrice") || undefined,
    sort: searchParams.get("sort") || undefined,
    page: Number(searchParams.get("page") || 1),
    pageSize: 12,
  });

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  useEffect(() => {
    const params = {};
    Object.entries(filters).forEach(([k, v]) => {
      if (v !== undefined && v !== "" && v !== null) params[k] = v;
    });
    setSearchParams(params, { replace: true });
    fetchProducts(filters);
  }, [filters, fetchProducts, setSearchParams]);

  const handleFilterChange = useCallback((changed) => {
    setFilters((prev) => ({ ...prev, ...changed, page: 1 }));
  }, []);

  const handlePageChange = (newPage) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Shop</h1>

      {/* Search bar */}
      <div className="mb-6">
        <input
          type="text"
          placeholder="Search products..."
          value={filters.query}
          onChange={(e) => handleFilterChange({ query: e.target.value })}
          className="w-full sm:max-w-md border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-action-main/50"
        />
      </div>

      <div className="flex flex-col lg:flex-row gap-8">
        <ProductFilters
          filters={filters}
          categories={categories}
          onChange={handleFilterChange}
        />

        <div className="flex-1">
          {loading ? (
            <Spinner />
          ) : products.length === 0 ? (
            <div className="text-center py-20 text-gray-500">
              No products found. Try adjusting filters.
            </div>
          ) : (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
                {products.map((p) => (
                  <ProductCard key={p.id} product={p} />
                ))}
              </div>
              <Pagination
                page={page}
                totalPages={totalPages}
                onPageChange={handlePageChange}
              />
            </>
          )}
        </div>
      </div>
    </div>
  );
}
