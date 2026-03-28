import { useEffect, useState } from "react";
import { adminApi } from "../../api/adminApi";
import { formatCurrency } from "../../utils/formatCurrency";
import { formatDate } from "../../utils/formatDate";
import StatusBadge from "../../components/ui/StatusBadge";
import Spinner from "../../components/ui/Spinner";
import toast from "react-hot-toast";

function toLocalISODate(date) {
  return date.toISOString().split("T")[0];
}

export default function AdminReportsPage() {
  const today = new Date();
  const thirtyDaysAgo = new Date(today);
  thirtyDaysAgo.setDate(today.getDate() - 30);

  const [from, setFrom] = useState(toLocalISODate(thirtyDaysAgo));
  const [to, setTo] = useState(toLocalISODate(today));
  const [salesData, setSalesData] = useState([]);
  const [statusSplit, setStatusSplit] = useState([]);
  const [loading, setLoading] = useState(false);

  const loadReports = async () => {
    setLoading(true);
    try {
      const [salesRes, splitRes] = await Promise.all([
        adminApi.getSalesReport(from, to),
        adminApi.getStatusSplit(),
      ]);
      setSalesData(salesRes.data);
      setStatusSplit(splitRes.data);
    } catch {
      toast.error("Failed to load reports.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadReports(); }, []);

  const totalRevenue = salesData.reduce((acc, d) => acc + d.revenue, 0);
  const totalOrders = salesData.reduce((acc, d) => acc + d.orderCount, 0);

  const exportCSV = () => {
    const header = "Date,Orders,Revenue\n";
    const rows = salesData
      .map((d) => `${formatDate(d.date)},${d.orderCount},${d.revenue}`)
      .join("\n");
    const blob = new Blob([header + rows], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `capshop-sales-${from}-to-${to}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div>
      <h1 className="text-xl font-bold text-gray-900 mb-6">Reports</h1>

      {/* Filters */}
      <div className="flex flex-wrap items-end gap-4 mb-6">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">From</label>
          <input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">To</label>
          <input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>
        <button
          onClick={loadReports}
          disabled={loading}
          className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-5 py-2 rounded-lg disabled:opacity-60"
        >
          {loading ? "Loading..." : "Apply"}
        </button>
        {salesData.length > 0 && (
          <button
            onClick={exportCSV}
            className="border border-gray-300 text-gray-700 hover:bg-gray-50 text-sm font-medium px-5 py-2 rounded-lg"
          >
            Export CSV
          </button>
        )}
      </div>

      {loading ? (
        <Spinner />
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Sales Table */}
          <div className="lg:col-span-2 bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
            <div className="px-5 py-4 border-b flex items-center justify-between">
              <h2 className="font-semibold text-gray-800">Sales Report</h2>
              <div className="text-sm text-gray-500">
                <span className="font-semibold text-gray-900">{totalOrders}</span> orders ·{" "}
                <span className="font-semibold text-gray-900">
                  {formatCurrency(totalRevenue)}
                </span>
              </div>
            </div>
            {salesData.length === 0 ? (
              <p className="p-6 text-gray-500 text-sm text-center">
                No data for the selected range.
              </p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
                    <tr>
                      {["Date", "Orders", "Revenue"].map((h) => (
                        <th key={h} className="px-4 py-3 text-left font-medium">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {salesData.map((row) => (
                      <tr key={row.date} className="hover:bg-gray-50">
                        <td className="px-4 py-3">{formatDate(row.date)}</td>
                        <td className="px-4 py-3">{row.orderCount}</td>
                        <td className="px-4 py-3 font-semibold">
                          {formatCurrency(row.revenue)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Status Split */}
          <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
            <div className="px-5 py-4 border-b">
              <h2 className="font-semibold text-gray-800">Order Status Split</h2>
            </div>
            <div className="p-5 space-y-3">
              {statusSplit.length === 0 ? (
                <p className="text-gray-500 text-sm text-center py-4">No data.</p>
              ) : (
                statusSplit.map((item) => {
                  const total = statusSplit.reduce((a, b) => a + b.count, 0);
                  const pct = total > 0 ? Math.round((item.count / total) * 100) : 0;
                  return (
                    <div key={item.status}>
                      <div className="flex items-center justify-between mb-1">
                        <StatusBadge status={item.status} />
                        <span className="text-sm font-semibold text-gray-800">
                          {item.count} ({pct}%)
                        </span>
                      </div>
                      <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-indigo-500 rounded-full"
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
