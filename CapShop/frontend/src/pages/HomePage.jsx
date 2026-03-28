import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useProductStore } from "../app/productStore";
import ProductCard from "../components/product/ProductCard";
import Spinner from "../components/ui/Spinner";

export default function HomePage() {
  const { featured, categories, loading, fetchFeatured, fetchCategories } =
    useProductStore();

  useEffect(() => {
    fetchFeatured();
    fetchCategories();
  }, [fetchFeatured, fetchCategories]);

  return (
    <div>
      {/* Hero */}
      <section className="bg-gradient-to-br from-indigo-600 to-indigo-800 text-white">
        <div className="max-w-7xl mx-auto px-4 py-20 flex flex-col items-center text-center gap-6">
          <h1 className="text-4xl sm:text-5xl font-extrabold leading-tight">
            Shop smarter,<br className="hidden sm:block" /> live better.
          </h1>
          <p className="text-indigo-200 text-lg max-w-xl">
            Discover thousands of products at unbeatable prices. Fast delivery,
            easy returns.
          </p>
          <Link
            to="/shop"
            className="bg-white text-indigo-700 font-semibold px-8 py-3 rounded-full hover:bg-indigo-50 transition-colors"
          >
            Shop Now
          </Link>
        </div>
      </section>

      {/* Categories */}
      {categories.length > 0 && (
        <section className="max-w-7xl mx-auto px-4 py-12">
          <h2 className="text-xl font-bold text-gray-900 mb-6">Shop by Category</h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {categories.map((cat) => (
              <Link
                key={cat.id}
                to={`/shop?category=${encodeURIComponent(cat.name)}`}
                className="flex flex-col items-center gap-2 p-4 bg-white rounded-xl border border-gray-100 hover:shadow-md hover:border-indigo-200 transition-all"
              >
                <div className="w-12 h-12 bg-indigo-50 rounded-full flex items-center justify-center text-indigo-600 font-bold text-lg">
                  {cat.name.charAt(0)}
                </div>
                <span className="text-xs font-medium text-gray-700 text-center line-clamp-2">
                  {cat.name}
                </span>
              </Link>
            ))}
          </div>
        </section>
      )}

      {/* Featured Products */}
      <section className="max-w-7xl mx-auto px-4 py-8 pb-16">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold text-gray-900">Featured Products</h2>
          <Link
            to="/shop"
            className="text-sm font-medium text-indigo-600 hover:text-indigo-700"
          >
            View all
          </Link>
        </div>
        {loading ? (
          <Spinner />
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
            {featured.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
