import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { FiShield, FiPackage, FiClock } from "react-icons/fi";
import { catalogApi } from "../api/catalogApi";
import { useCartStore } from "../app/cartStore";
import { useAuthStore } from "../app/authStore";
import { formatCurrency } from "../utils/formatCurrency";
import StatusBadge from "../components/ui/StatusBadge";
import EmptyState from "../components/ui/EmptyState";
import ProductCard from "../components/product/ProductCard";
import toast from "react-hot-toast";

const TRUST_POINTS = [
  { Icon: FiShield, label: "Secure checkout" },
  { Icon: FiPackage, label: "Easy returns" },
  { Icon: FiClock, label: "Live order updates" },
];

function ProductDetailSkeleton() {
  return (
    <div className="max-w-6xl mx-auto px-4 py-8 animate-pulse">
      <div className="h-4 bg-gray-200 rounded w-48 mb-8" />
      <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
        <div className="aspect-square bg-gray-200 rounded-2xl" />
        <div className="flex flex-col gap-5">
          <div className="h-3 bg-gray-200 rounded w-20" />
          <div className="h-7 bg-gray-200 rounded w-3/4" />
          <div className="h-9 bg-gray-200 rounded w-36" />
          <div className="h-5 bg-gray-200 rounded w-28" />
          <div className="h-px bg-gray-200 my-1" />
          <div className="h-14 bg-gray-200 rounded-xl w-full" />
        </div>
      </div>
    </div>
  );
}

export default function ProductDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const token = useAuthStore((s) => s.token);
  const role = useAuthStore((s) => s.role);
  const addItem = useCartStore((s) => s.addItem);

  const [product, setProduct] = useState(null);
  const [related, setRelated] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [qty, setQty] = useState(1);
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    setLoading(true);
    setLoadError(false);
    setProduct(null);
    catalogApi
      .getProductById(id)
      .then(({ data }) => setProduct(data))
      .catch(() => {
        setLoadError(true);
        toast.error("Product not found.");
      })
      .finally(() => setLoading(false));
  }, [id]);

  useEffect(() => {
    if (!product) return;
    document.title = `${product.name} | CapShop`;
    return () => {
      document.title = "CapShop";
    };
  }, [product]);

  useEffect(() => {
    if (!product) {
      setRelated([]);
      return;
    }
    let cancelled = false;
    catalogApi
      .getFeaturedProducts()
      .then(({ data }) => {
        if (!cancelled) {
          setRelated(data.filter((p) => p.id !== product.id).slice(0, 4));
        }
      })
      .catch(() => {
        if (!cancelled) setRelated([]);
      });
    return () => {
      cancelled = true;
    };
  }, [product]);

  const handleAddToCart = async () => {
    if (!token) {
      navigate("/login");
      return;
    }
    if (role === "Admin") {
      toast.error("Admin accounts cannot add items to cart.");
      return;
    }
    setAdding(true);
    try {
      await addItem(product, qty);
      toast.success("Added to cart!");
    } catch {
      toast.error("Failed to add to cart.");
    } finally {
      setAdding(false);
    }
  };

  if (loading) return <ProductDetailSkeleton />;
  if (loadError || !product) {
    return (
      <div className="max-w-6xl mx-auto px-4 py-8">
        <EmptyState
          title="Product not found"
          description="This item may have been removed or the link is incorrect."
          action={
            <Link
              to="/shop"
              className="inline-flex items-center justify-center rounded-lg bg-action-main hover:bg-action-hover text-white font-medium px-6 py-2.5 transition-colors"
            >
              Back to shop
            </Link>
          }
        />
      </div>
    );
  }

  const effectivePrice = product.discountPrice ?? product.price;
  const hasDiscount = product.discountPrice && product.discountPrice < product.price;
  const discount = hasDiscount
    ? Math.round(((product.price - product.discountPrice) / product.price) * 100)
    : 0;
  const inStock = product.stock > 0;
  const lowStock = inStock && product.stock <= 5;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <nav className="text-sm text-gray-500 mb-6" aria-label="Breadcrumb">
        <Link to="/" className="hover:text-action-main">Home</Link>
        <span className="mx-2">/</span>
        <Link to="/shop" className="hover:text-action-main">Shop</Link>
        <span className="mx-2">/</span>
        <span className="text-gray-800">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
        {/* Image */}
        <div className="group relative bg-gray-50 rounded-2xl overflow-hidden aspect-square shadow-sm">
          <img
            src={product.imageUrl || "https://placehold.co/600x600?text=No+Image"}
            alt={product.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
          />
          {product.isFeatured && (
            <span className="absolute top-4 left-4 bg-amber-500 text-white text-xs font-bold px-2.5 py-1 rounded-full shadow-sm">
              Featured
            </span>
          )}
          {hasDiscount && (
            <span className="absolute top-4 right-4 bg-red-500 text-white text-sm font-bold px-2.5 py-1 rounded-full shadow-sm">
              -{discount}%
            </span>
          )}
          {!inStock && (
            <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
              <span className="text-white font-semibold text-base">Out of Stock</span>
            </div>
          )}
        </div>

        {/* Info panel */}
        <div className="flex flex-col gap-4 md:sticky md:top-24 md:self-start">
          <div>
            <p className="text-sm text-action-main font-medium mb-1">{product.categoryName}</p>
            <h1 className="text-2xl md:text-3xl font-bold text-gray-900 leading-tight">{product.name}</h1>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <span className="text-3xl font-bold text-gray-900">
              {formatCurrency(effectivePrice)}
            </span>
            {hasDiscount && (
              <>
                <span className="text-lg text-gray-400 line-through">
                  {formatCurrency(product.price)}
                </span>
                <span className="bg-red-100 text-red-600 text-sm font-semibold px-2 py-0.5 rounded-full">
                  -{discount}% off
                </span>
              </>
            )}
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <StatusBadge status={product.status} />
            <span className="text-sm text-gray-500">
              {inStock ? `${product.stock} in stock` : "Out of stock"}
            </span>
            {lowStock && (
              <span className="text-xs font-semibold text-amber-700 bg-amber-50 border border-amber-200 px-2 py-0.5 rounded-full">
                Only {product.stock} left
              </span>
            )}
          </div>

          <hr className="border-gray-100" />

          {inStock ? (
            <div className="bg-gray-50 border border-gray-100 rounded-xl p-4 flex flex-col gap-4">
              <div className="flex flex-col sm:flex-row sm:items-center gap-3">
                <div className="flex items-center border border-gray-300 bg-white rounded-lg overflow-hidden shrink-0">
                  <button
                    type="button"
                    onClick={() => setQty((q) => Math.max(1, q - 1))}
                    aria-label="Decrease quantity"
                    className="px-3 py-2.5 hover:bg-gray-100 text-gray-700 font-medium min-w-[44px] transition-colors"
                  >
                    −
                  </button>
                  <span className="px-5 py-2.5 text-sm font-semibold tabular-nums min-w-12 text-center">
                    {qty}
                  </span>
                  <button
                    type="button"
                    onClick={() => setQty((q) => Math.min(product.stock, q + 1))}
                    aria-label="Increase quantity"
                    className="px-3 py-2.5 hover:bg-gray-100 text-gray-700 font-medium min-w-[44px] transition-colors"
                  >
                    +
                  </button>
                </div>
                <button
                  type="button"
                  onClick={handleAddToCart}
                  disabled={adding}
                  className="flex-1 bg-action-main hover:bg-action-hover text-white font-medium py-3 rounded-lg transition-colors disabled:opacity-60 min-h-[44px]"
                >
                  {adding ? "Adding..." : "Add to Cart"}
                </button>
              </div>

              <div className="flex flex-wrap gap-x-5 gap-y-2 pt-1 border-t border-gray-200">
                {TRUST_POINTS.map(({ Icon, label }) => (
                  <span key={label} className="flex items-center gap-1.5 text-xs text-gray-500">
                    <Icon size={13} className="text-gray-400 shrink-0" />
                    {label}
                  </span>
                ))}
              </div>
            </div>
          ) : (
            <p className="text-sm text-gray-500">
              This product is currently unavailable.{" "}
              <Link to="/shop" className="text-action-main font-medium hover:underline">
                Browse similar items
              </Link>
            </p>
          )}
        </div>
      </div>

      {product.description && (
        <section className="mt-12 pt-10 border-t border-gray-100">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">About this product</h2>
          <p className="text-gray-600 leading-relaxed max-w-2xl whitespace-pre-line">
            {product.description}
          </p>
        </section>
      )}

      {related.length > 0 && (
        <section className="mt-12 pt-10 border-t border-gray-100" aria-labelledby="related-heading">
          <h2 id="related-heading" className="text-lg font-semibold text-gray-900 mb-6">
            You may also like
          </h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {related.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
