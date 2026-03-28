import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useOrderStore } from "../app/orderStore";
import { formatCurrency } from "../utils/formatCurrency";
import { formatDateTime } from "../utils/formatDate";
import StatusBadge from "../components/ui/StatusBadge";
import ConfirmDialog from "../components/ui/ConfirmDialog";
import Spinner from "../components/ui/Spinner";
import toast from "react-hot-toast";

const CANCELLABLE_STATUSES = ["Pending", "Paid"];

export default function OrderDetailPage() {
  const { id } = useParams();
  const { currentOrder, loading, fetchOrderById, cancelOrder } = useOrderStore();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    fetchOrderById(id);
  }, [id, fetchOrderById]);

  const handleCancel = async () => {
    setCancelling(true);
    try {
      await cancelOrder(id);
      toast.success("Order cancelled.");
      fetchOrderById(id);
    } catch {
      toast.error("Failed to cancel order.");
    } finally {
      setCancelling(false);
    }
  };

  if (loading || !currentOrder) return <Spinner />;

  const canCancel = CANCELLABLE_STATUSES.includes(currentOrder.status);

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <Link to="/orders" className="text-sm text-indigo-600 hover:underline">
          ← My Orders
        </Link>
      </div>

      <div className="flex items-start justify-between mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-900">Order Detail</h1>
          <p className="font-mono text-xs text-gray-400 mt-1">{currentOrder.id}</p>
        </div>
        <div className="flex items-center gap-3">
          <StatusBadge status={currentOrder.status} />
          {canCancel && (
            <button
              onClick={() => setConfirmOpen(true)}
              disabled={cancelling}
              className="text-sm text-red-600 border border-red-300 rounded-lg px-3 py-1.5 hover:bg-red-50 disabled:opacity-50"
            >
              Cancel Order
            </button>
          )}
        </div>
      </div>

      {/* Items */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden mb-5">
        <div className="px-5 py-3 border-b bg-gray-50">
          <h2 className="font-semibold text-gray-800 text-sm">Items</h2>
        </div>
        <div className="px-5 py-4 space-y-3">
          {currentOrder.items.map((item) => (
            <div key={item.productId} className="flex items-center gap-4">
              <img
                src={item.imageUrl || "https://placehold.co/56x56?text=N/A"}
                alt={item.productName}
                className="w-14 h-14 object-cover rounded-lg shrink-0"
              />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-800 truncate">{item.productName}</p>
                <p className="text-xs text-gray-500">
                  {formatCurrency(item.price)} × {item.quantity}
                </p>
              </div>
              <p className="font-semibold text-gray-900 text-sm">
                {formatCurrency(item.subtotal)}
              </p>
            </div>
          ))}
        </div>
      </div>

      {/* Info */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="px-5 py-3 border-b bg-gray-50">
          <h2 className="font-semibold text-gray-800 text-sm">Details</h2>
        </div>
        <div className="px-5 py-4 space-y-3 text-sm">
          {[
            ["Shipping Address", `${currentOrder.shippingAddress}, ${currentOrder.shippingCity} - ${currentOrder.shippingPincode}`],
            ["Payment Method", currentOrder.paymentMethod ?? "—"],
            ["Placed On", formatDateTime(currentOrder.createdAt)],
            ["Paid On", currentOrder.paidAt ? formatDateTime(currentOrder.paidAt) : "—"],
          ].map(([label, value]) => (
            <div key={label} className="flex justify-between gap-4">
              <span className="text-gray-500">{label}</span>
              <span className="text-gray-800 text-right">{value}</span>
            </div>
          ))}
          <div className="flex justify-between pt-3 border-t font-bold text-gray-900">
            <span>Total</span>
            <span>{formatCurrency(currentOrder.totalAmount)}</span>
          </div>
        </div>
      </div>

      <ConfirmDialog
        isOpen={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        onConfirm={handleCancel}
        title="Cancel Order"
        message="Are you sure you want to cancel this order? This cannot be undone."
        confirmLabel="Cancel Order"
        danger
      />
    </div>
  );
}
