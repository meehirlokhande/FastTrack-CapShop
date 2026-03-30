import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { catalogApi } from "../api/catalogApi";
import { useCartStore } from "../app/cartStore";
import { useAuthStore } from "../app/authStore";
import { formatCurrency } from "../utils/formatCurrency";
import StatusBadge from "../components/ui/StatusBadge";
import Spinner from "../components/ui/Spinner";
import toast from "react-hot-toast";

export default function ProductDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const token = useAuthStore((s) => s.token);
  const role = useAuthStore((s) => s.role);
  const addItem = useCartStore((s) => s.addItem);

  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [qty, setQty] = useState(1);
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    setLoading(true);
    catalogApi
      .getProductById(id)
      .then(({ data }) => setProduct(data))
      .catch(() => toast.error("Product not found."))
      .finally(() => setLoading(false));
  }, [id]);

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

  if (loading) return <Spinner />;
  if (!product) return null;

  const effectivePrice = product.discountPrice ?? product.price;
  const hasDiscount = product.discountPrice && product.discountPrice < product.price;
  const discount = hasDiscount
    ? Math.round(((product.price - product.discountPrice) / product.price) * 100)
    : 0;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <nav className="text-sm text-gray-500 mb-6">
        <Link to="/" className="hover:text-action-main">Home</Link>
        <span className="mx-2">/</span>
        <Link to="/shop" className="hover:text-action-main">Shop</Link>
        <span className="mx-2">/</span>
        <span className="text-gray-800">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
        {/* Image */}
        <div className="bg-gray-50 rounded-2xl overflow-hidden aspect-square">
          <img
            src={product.imageUrl || "https://placehold.co/600x600?text=No+Image"}
            alt={product.name}
            className="w-full h-full object-cover"
          />
        </div>

        {/* Info */}
        <div className="flex flex-col gap-4">
          <div>
            <p className="text-sm text-action-main font-medium mb-1">{product.categoryName}</p>
            <h1 className="text-2xl font-bold text-gray-900">{product.name}</h1>
          </div>

          <div className="flex items-center gap-3">
            <span className="text-3xl font-bold text-gray-900">
              {formatCurrency(effectivePrice)}
            </span>
            {hasDiscount && (
              <>
                <span className="text-lg text-gray-400 line-through">
                  {formatCurrency(product.price)}
                </span>
                <span className="bg-red-100 text-red-600 text-sm font-semibold px-2 py-0.5 rounded-full">
                  -{discount}%
                </span>
              </>
            )}
          </div>

          <div className="flex items-center gap-2">
            <StatusBadge status={product.status} />
            <span className="text-sm text-gray-500">
              {product.stock > 0 ? `${product.stock} in stock` : "Out of stock"}
            </span>
          </div>

          <p className="text-gray-600 leading-relaxed">{product.description}</p>

          {product.stock > 0 && (
            <div className="flex items-center gap-4 mt-2">
              <div className="flex items-center border border-gray-300 rounded-lg overflow-hidden">
                <button
                  onClick={() => setQty((q) => Math.max(1, q - 1))}
                  className="px-3 py-2 hover:bg-gray-100 text-gray-700 font-medium"
                >
                  -
                </button>
                <span className="px-4 py-2 text-sm font-semibold">{qty}</span>
                <button
                  onClick={() => setQty((q) => Math.min(product.stock, q + 1))}
                  className="px-3 py-2 hover:bg-gray-100 text-gray-700 font-medium"
                >
                  +
                </button>
              </div>
              <button
                onClick={handleAddToCart}
                disabled={adding}
                className="flex-1 bg-action-main hover:bg-action-hover text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-60"
              >
                {adding ? "Adding..." : "Add to Cart"}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
