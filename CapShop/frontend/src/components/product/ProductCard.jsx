import { Link } from "react-router-dom";
import { formatCurrency } from "../../utils/formatCurrency";

export default function ProductCard({ product }) {
  const { id, name, price, discountPrice, imageUrl, categoryName, stock } = product;
  const hasDiscount = discountPrice && discountPrice < price;
  const discount = hasDiscount
    ? Math.round(((price - discountPrice) / price) * 100)
    : 0;

  return (
    <Link
      to={`/shop/${id}`}
      className="group bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow"
    >
      <div className="relative aspect-square overflow-hidden bg-gray-50">
        <img
          src={imageUrl || "https://placehold.co/400x400?text=No+Image"}
          alt={name}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
        {hasDiscount && (
          <span className="absolute top-2 left-2 bg-red-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
            -{discount}%
          </span>
        )}
        {stock === 0 && (
          <div className="absolute inset-0 bg-black/40 flex items-center justify-center">
            <span className="text-white font-semibold text-sm">Out of Stock</span>
          </div>
        )}
      </div>
      <div className="p-4">
        <p className="text-xs text-action-main font-medium mb-1">{categoryName}</p>
        <h3 className="text-sm font-semibold text-gray-800 line-clamp-2 mb-2">{name}</h3>
        <div className="flex items-center gap-2">
          <span className="text-base font-bold text-gray-900">
            {formatCurrency(hasDiscount ? discountPrice : price)}
          </span>
          {hasDiscount && (
            <span className="text-sm text-gray-400 line-through">
              {formatCurrency(price)}
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
