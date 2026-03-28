import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useCartStore } from "../app/cartStore";
import { formatCurrency } from "../utils/formatCurrency";
import ConfirmDialog from "../components/ui/ConfirmDialog";
import Spinner from "../components/ui/Spinner";
import EmptyState from "../components/ui/EmptyState";
import toast from "react-hot-toast";

export default function CartPage() {
  const { items, total, loading, fetchCart, updateQuantity, removeItem } = useCartStore();
  const navigate = useNavigate();
  const [removing, setRemoving] = useState(null);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [targetItem, setTargetItem] = useState(null);

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  const handleQuantityChange = async (itemId, qty) => {
    try {
      await updateQuantity(itemId, qty);
    } catch {
      toast.error("Failed to update quantity.");
    }
  };

  const handleRemove = async () => {
    setRemoving(targetItem);
    try {
      await removeItem(targetItem);
    } catch {
      toast.error("Failed to remove item.");
    } finally {
      setRemoving(null);
    }
  };

  if (loading) return <Spinner />;

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Your Cart</h1>

      {items.length === 0 ? (
        <EmptyState
          title="Your cart is empty"
          description="Looks like you haven't added anything yet."
          action={
            <Link
              to="/shop"
              className="bg-indigo-600 text-white px-6 py-2.5 rounded-lg font-medium hover:bg-indigo-700"
            >
              Browse Products
            </Link>
          }
        />
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Items */}
          <div className="lg:col-span-2 space-y-4">
            {items.map((item) => (
              <div
                key={item.id}
                className="flex gap-4 bg-white p-4 rounded-xl border border-gray-100 shadow-sm"
              >
                <img
                  src={item.imageUrl || "https://placehold.co/80x80?text=N/A"}
                  alt={item.productName}
                  className="w-20 h-20 object-cover rounded-lg shrink-0"
                />
                <div className="flex-1 min-w-0">
                  <p className="font-semibold text-gray-800 truncate">{item.productName}</p>
                  <p className="text-indigo-600 font-medium mt-1">
                    {formatCurrency(item.price)}
                  </p>
                  <div className="flex items-center gap-3 mt-2">
                    <div className="flex items-center border border-gray-300 rounded-lg overflow-hidden">
                      <button
                        onClick={() =>
                          handleQuantityChange(item.id, item.quantity - 1)
                        }
                        disabled={item.quantity <= 1}
                        className="px-2.5 py-1 hover:bg-gray-100 disabled:opacity-40"
                      >
                        -
                      </button>
                      <span className="px-3 py-1 text-sm font-semibold">
                        {item.quantity}
                      </span>
                      <button
                        onClick={() =>
                          handleQuantityChange(item.id, item.quantity + 1)
                        }
                        className="px-2.5 py-1 hover:bg-gray-100"
                      >
                        +
                      </button>
                    </div>
                    <button
                      onClick={() => {
                        setTargetItem(item.id);
                        setConfirmOpen(true);
                      }}
                      disabled={removing === item.id}
                      className="text-sm text-red-500 hover:text-red-600 font-medium"
                    >
                      Remove
                    </button>
                  </div>
                </div>
                <div className="text-right shrink-0">
                  <p className="font-bold text-gray-900">
                    {formatCurrency(item.subtotal)}
                  </p>
                </div>
              </div>
            ))}
          </div>

          {/* Summary */}
          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 h-fit sticky top-24">
            <h2 className="font-semibold text-gray-900 mb-4">Order Summary</h2>
            <div className="space-y-2 text-sm text-gray-600 mb-4">
              <div className="flex justify-between">
                <span>Subtotal ({items.length} items)</span>
                <span>{formatCurrency(total)}</span>
              </div>
              <div className="flex justify-between">
                <span>Delivery</span>
                <span className="text-green-600">FREE</span>
              </div>
            </div>
            <div className="border-t pt-4 flex justify-between font-bold text-gray-900 mb-5">
              <span>Total</span>
              <span>{formatCurrency(total)}</span>
            </div>
            <button
              onClick={() => navigate("/checkout")}
              className="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2.5 rounded-lg transition-colors"
            >
              Proceed to Checkout
            </button>
          </div>
        </div>
      )}

      <ConfirmDialog
        isOpen={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        onConfirm={handleRemove}
        title="Remove item"
        message="Are you sure you want to remove this item from your cart?"
        confirmLabel="Remove"
        danger
      />
    </div>
  );
}
