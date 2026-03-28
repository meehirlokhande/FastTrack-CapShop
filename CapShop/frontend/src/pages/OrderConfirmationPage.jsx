import { useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { useOrderStore } from "../app/orderStore";
import { formatCurrency } from "../utils/formatCurrency";
import { formatDateTime } from "../utils/formatDate";
import StatusBadge from "../components/ui/StatusBadge";
import Spinner from "../components/ui/Spinner";

export default function OrderConfirmationPage() {
  const { id } = useParams();
  const { currentOrder, loading, fetchOrderById } = useOrderStore();

  useEffect(() => {
    fetchOrderById(id);
  }, [id, fetchOrderById]);

  if (loading || !currentOrder) return <Spinner />;

  return (
    <div className="max-w-2xl mx-auto px-4 py-12">
      <div className="text-center mb-8">
        <div className="text-6xl mb-3">🎉</div>
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Order Confirmed!</h1>
        <p className="text-gray-500">
          Thank you for shopping with CapShop. Your order is on its way!
        </p>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        {/* Header */}
        <div className="px-6 py-4 bg-gray-50 flex items-center justify-between border-b">
          <div>
            <p className="text-xs text-gray-500 mb-0.5">Order ID</p>
            <p className="font-mono text-sm text-gray-800">{currentOrder.id}</p>
          </div>
          <StatusBadge status={currentOrder.status} />
        </div>

        {/* Items */}
        <div className="px-6 py-4 space-y-3">
          {currentOrder.items.map((item) => (
            <div key={item.productId} className="flex items-center gap-4">
              <img
                src={item.imageUrl || "https://placehold.co/56x56?text=N/A"}
                alt={item.productName}
                className="w-14 h-14 object-cover rounded-lg shrink-0"
              />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-800 truncate">
                  {item.productName}
                </p>
                <p className="text-xs text-gray-500">Qty: {item.quantity}</p>
              </div>
              <p className="text-sm font-semibold text-gray-900">
                {formatCurrency(item.subtotal)}
              </p>
            </div>
          ))}
        </div>

        {/* Summary */}
        <div className="px-6 py-4 border-t bg-gray-50 space-y-2 text-sm">
          <div className="flex justify-between text-gray-600">
            <span>Shipping to</span>
            <span>
              {currentOrder.shippingAddress}, {currentOrder.shippingCity} -{" "}
              {currentOrder.shippingPincode}
            </span>
          </div>
          <div className="flex justify-between text-gray-600">
            <span>Payment</span>
            <span>{currentOrder.paymentMethod ?? "—"}</span>
          </div>
          <div className="flex justify-between text-gray-600">
            <span>Ordered on</span>
            <span>{formatDateTime(currentOrder.createdAt)}</span>
          </div>
          <div className="flex justify-between font-bold text-gray-900 text-base pt-2 border-t">
            <span>Total</span>
            <span>{formatCurrency(currentOrder.totalAmount)}</span>
          </div>
        </div>
      </div>

      <div className="flex gap-4 mt-8 justify-center">
        <Link
          to="/orders"
          className="px-6 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50"
        >
          My Orders
        </Link>
        <Link
          to="/shop"
          className="px-6 py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700"
        >
          Continue Shopping
        </Link>
      </div>
    </div>
  );
}
