import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useOrderStore } from "../app/orderStore";
import { formatCurrency } from "../utils/formatCurrency";
import { formatDate } from "../utils/formatDate";
import StatusBadge from "../components/ui/StatusBadge";
import Spinner from "../components/ui/Spinner";
import EmptyState from "../components/ui/EmptyState";

export default function OrdersPage() {
  const { orders, loading, fetchMyOrders } = useOrderStore();

  useEffect(() => {
    fetchMyOrders();
  }, [fetchMyOrders]);

  if (loading) return <Spinner />;

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">My Orders</h1>

      {orders.length === 0 ? (
        <EmptyState
          title="No orders yet"
          description="Start shopping to see your orders here."
          action={
            <Link
              to="/shop"
              className="bg-action-main text-white px-6 py-2.5 rounded-lg font-medium hover:bg-action-hover"
            >
              Shop Now
            </Link>
          }
        />
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <Link
              key={order.id}
              to={`/orders/${order.id}`}
              className="block bg-white rounded-xl border border-gray-100 shadow-sm p-5 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="font-mono text-xs text-gray-400 mb-1">{order.id}</p>
                  <p className="font-semibold text-gray-800">
                    {order.itemCount} {order.itemCount === 1 ? "item" : "items"}
                  </p>
                  <p className="text-sm text-gray-500 mt-1">{formatDate(order.createdAt)}</p>
                </div>
                <div className="text-right">
                  <p className="font-bold text-gray-900 text-lg">
                    {formatCurrency(order.totalAmount)}
                  </p>
                  <div className="mt-1">
                    <StatusBadge status={order.status} />
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
