import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { adminApi } from "../../api/adminApi";
import { formatCurrency } from "../../utils/formatCurrency";
import StatusBadge from "../../components/ui/StatusBadge";
import ConfirmDialog from "../../components/ui/ConfirmDialog";
import Spinner from "../../components/ui/Spinner";
import toast from "react-hot-toast";

export default function AdminProductsPage() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [stockTarget, setStockTarget] = useState(null);
  const [stockValue, setStockValue] = useState("");

  const load = () =>
    adminApi
      .getProducts()
      .then(({ data }) => setProducts(data))
      .catch(() => toast.error("Failed to load products."))
      .finally(() => setLoading(false));

  useEffect(() => { load(); }, []);

  const handleDelete = async () => {
    try {
      await adminApi.deleteProduct(deleteTarget);
      toast.success("Product deleted.");
      load();
    } catch {
      toast.error("Failed to delete product.");
    }
  };

  const handleToggleStatus = async (id) => {
    try {
      await adminApi.toggleProductStatus(id);
      toast.success("Status updated.");
      load();
    } catch {
      toast.error("Failed to update status.");
    }
  };

  const handleUpdateStock = async () => {
    const qty = parseInt(stockValue, 10);
    if (isNaN(qty) || qty < 0) { toast.error("Enter a valid stock value."); return; }
    try {
      await adminApi.updateStock(stockTarget, { stock: qty });
      toast.success("Stock updated.");
      setStockTarget(null);
      setStockValue("");
      load();
    } catch {
      toast.error("Failed to update stock.");
    }
  };

  if (loading) return <Spinner />;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold text-gray-900">Products</h1>
        <Link
          to="/admin/products/new"
          className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg"
        >
          + Add Product
        </Link>
      </div>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
              <tr>
                {["Product", "Category", "Price", "Stock", "Status", "Actions"].map((h) => (
                  <th key={h} className="px-4 py-3 text-left font-medium whitespace-nowrap">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {products.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <img
                        src={p.imageUrl || "https://placehold.co/40x40?text=N/A"}
                        alt={p.name}
                        className="w-10 h-10 object-cover rounded-lg shrink-0"
                      />
                      <span className="font-medium text-gray-800 line-clamp-1">{p.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{p.categoryName}</td>
                  <td className="px-4 py-3 font-medium">
                    {formatCurrency(p.discountPrice ?? p.price)}
                    {p.discountPrice && (
                      <span className="text-xs text-gray-400 line-through ml-1">
                        {formatCurrency(p.price)}
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => { setStockTarget(p.id); setStockValue(String(p.stock)); }}
                      className="text-gray-800 hover:text-indigo-600 font-medium"
                    >
                      {p.stock}
                    </button>
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={p.status} />
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Link
                        to={`/admin/products/${p.id}`}
                        className="text-xs text-indigo-600 hover:underline"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => handleToggleStatus(p.id)}
                        className="text-xs text-yellow-600 hover:underline"
                      >
                        Toggle
                      </button>
                      <button
                        onClick={() => setDeleteTarget(p.id)}
                        className="text-xs text-red-500 hover:underline"
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Stock edit inline modal */}
      {stockTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="absolute inset-0 bg-black/40" onClick={() => setStockTarget(null)} />
          <div className="relative bg-white rounded-xl p-6 w-80 z-10 shadow-xl">
            <h3 className="font-semibold text-gray-900 mb-4">Update Stock</h3>
            <input
              type="number"
              value={stockValue}
              onChange={(e) => setStockValue(e.target.value)}
              min={0}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setStockTarget(null)}
                className="text-sm text-gray-600 border border-gray-300 px-4 py-2 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleUpdateStock}
                className="text-sm bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700"
              >
                Save
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Product"
        message="This will permanently delete the product. Are you sure?"
        confirmLabel="Delete"
        danger
      />
    </div>
  );
}
