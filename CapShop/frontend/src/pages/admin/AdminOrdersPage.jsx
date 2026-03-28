import { useEffect, useState } from "react";
import { adminApi } from "../../api/adminApi";
import { formatCurrency } from "../../utils/formatCurrency";
import { formatDate } from "../../utils/formatDate";
import StatusBadge from "../../components/ui/StatusBadge";
import Spinner from "../../components/ui/Spinner";
import toast from "react-hot-toast";

const STATUS_TRANSITIONS = {
  Pending: ["Paid", "Cancelled"],
  Paid: ["Packed", "Cancelled"],
  Packed: ["Shipped"],
  Shipped: ["Delivered"],
  Delivered: [],
  Cancelled: [],
};

export default function AdminOrdersPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(null);

  const load = () =>
    adminApi
      .getOrders()
      .then(({ data }) => setOrders(data))
      .catch(() => toast.error("Failed to load orders."))
      .finally(() => setLoading(false));

  useEffect(() => { load(); }, []);

  const handleStatusChange = async (orderId, status) => {
    setUpdating(orderId);
    try {
      await adminApi.updateOrderStatus(orderId, { status });
      toast.success(`Status updated to ${status}.`);
      load();
    } catch {
      toast.error("Failed to update status.");
    } finally {
      setUpdating(null);
    }
  };

  if (loading) return <Spinner />;

  return (
    <div>
      <h1 className="text-xl font-bold text-gray-900 mb-6">Orders</h1>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
              <tr>
                {["Order ID", "City", "Items", "Amount", "Paid On", "Status", "Update Status"].map(
                  (h) => (
                    <th key={h} className="px-4 py-3 text-left font-medium whitespace-nowrap">
                      {h}
                    </th>
                  )
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {orders.map((order) => {
                const nextStatuses = STATUS_TRANSITIONS[order.status] ?? [];
                return (
                  <tr key={order.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-mono text-xs text-gray-500">
                      {order.id.slice(0, 8)}...
                    </td>
                    <td className="px-4 py-3">{order.shippingCity}</td>
                    <td className="px-4 py-3">{order.itemCount}</td>
                    <td className="px-4 py-3 font-semibold">
                      {formatCurrency(order.totalAmount)}
                    </td>
                    <td className="px-4 py-3 text-gray-500">
                      {order.paidAt ? formatDate(order.paidAt) : "—"}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={order.status} />
                    </td>
                    <td className="px-4 py-3">
                      {nextStatuses.length > 0 ? (
                        <select
                          disabled={updating === order.id}
                          defaultValue=""
                          onChange={(e) => {
                            if (e.target.value) handleStatusChange(order.id, e.target.value);
                          }}
                          className="text-xs border border-gray-300 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-2 focus:ring-indigo-400 disabled:opacity-50"
                        >
                          <option value="" disabled>
                            Move to...
                          </option>
                          {nextStatuses.map((s) => (
                            <option key={s} value={s}>
                              {s}
                            </option>
                          ))}
                        </select>
                      ) : (
                        <span className="text-xs text-gray-400">—</span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
