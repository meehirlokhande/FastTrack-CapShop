import { useEffect, useState } from "react";
import { adminApi } from "../../api/adminApi";
import { formatCurrency } from "../../utils/formatCurrency";
import { formatDate } from "../../utils/formatDate";
import StatusBadge from "../../components/ui/StatusBadge";
import Spinner from "../../components/ui/Spinner";
import toast from "react-hot-toast";

function KPICard({ label, value, color = "indigo" }) {
  const colorMap = {
    indigo: "bg-indigo-50 text-indigo-700",
    green: "bg-green-50 text-green-700",
    yellow: "bg-yellow-50 text-yellow-700",
    blue: "bg-blue-50 text-blue-700",
  };
  return (
    <div className={`rounded-xl p-5 ${colorMap[color]}`}>
      <p className="text-sm font-medium opacity-80">{label}</p>
      <p className="text-3xl font-bold mt-1">{value}</p>
    </div>
  );
}

export default function DashboardPage() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi
      .getDashboard()
      .then(({ data }) => setSummary(data))
      .catch(() => toast.error("Failed to load dashboard."))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;
  if (!summary) return null;

  return (
    <div>
      <h1 className="text-xl font-bold text-gray-900 mb-6">Dashboard</h1>

      {/* KPIs */}
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-4 mb-8">
        <KPICard label="Total Products" value={summary.totalProducts} color="indigo" />
        <KPICard label="Total Orders" value={summary.totalOrders} color="blue" />
        <KPICard
          label="Total Revenue"
          value={formatCurrency(summary.totalRevenue)}
          color="green"
        />
        <KPICard label="Pending Orders" value={summary.pendingOrders} color="yellow" />
        <KPICard label="Delivered" value={summary.deliveredOrders} color="green" />
      </div>

      {/* Recent Orders */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="px-5 py-4 border-b">
          <h2 className="font-semibold text-gray-800">Recent Orders</h2>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
              <tr>
                {["Order ID", "User", "City", "Items", "Amount", "Status", "Date"].map(
                  (h) => (
                    <th key={h} className="px-4 py-3 text-left font-medium">
                      {h}
                    </th>
                  )
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {summary.recentOrders.map((order) => (
                <tr key={order.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-xs text-gray-500">
                    {order.id.slice(0, 8)}...
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-gray-500">
                    {order.userId.slice(0, 8)}...
                  </td>
                  <td className="px-4 py-3">{order.shippingCity}</td>
                  <td className="px-4 py-3">{order.itemCount}</td>
                  <td className="px-4 py-3 font-semibold">
                    {formatCurrency(order.totalAmount)}
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={order.status} />
                  </td>
                  <td className="px-4 py-3 text-gray-500">{formatDate(order.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
