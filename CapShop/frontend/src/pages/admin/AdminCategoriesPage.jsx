import { useEffect, useState } from "react";
import { adminApi } from "../../api/adminApi";
import ConfirmDialog from "../../components/ui/ConfirmDialog";
import Spinner from "../../components/ui/Spinner";
import toast from "react-hot-toast";

const EMPTY_FORM = { name: "", description: "" };

export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const load = () =>
    adminApi
      .getAdminCategories()
      .then(({ data }) => setCategories(data))
      .catch(() => toast.error("Failed to load categories."))
      .finally(() => setLoading(false));

  useEffect(() => {
    load();
  }, []);

  const handleChange = (e) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!form.name.trim()) {
      toast.error("Category name is required.");
      return;
    }
    setSaving(true);
    try {
      await adminApi.createCategory({
        name: form.name.trim(),
        description: form.description.trim(),
      });
      toast.success("Category created.");
      setForm(EMPTY_FORM);
      load();
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Failed to create category.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    try {
      await adminApi.deleteCategory(deleteTarget);
      toast.success("Category deleted.");
      load();
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Failed to delete category.");
    }
  };

  return (
    <div className="max-w-4xl">
      <h1 className="text-xl font-bold text-gray-900 mb-6">Categories</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Create form */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 h-fit">
          <h2 className="font-semibold text-gray-800 mb-4">Add Category</h2>
          <form onSubmit={handleCreate} className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Name <span className="text-red-500">*</span>
              </label>
              <input
                name="name"
                value={form.name}
                onChange={handleChange}
                placeholder="e.g. Sports"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description
              </label>
              <textarea
                name="description"
                value={form.description}
                onChange={handleChange}
                rows={3}
                placeholder="Optional description"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none"
              />
            </div>
            <button
              type="submit"
              disabled={saving}
              className="w-full bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2 rounded-lg disabled:opacity-60"
            >
              {saving ? "Creating..." : "Create Category"}
            </button>
          </form>
        </div>

        {/* Category list */}
        <div className="lg:col-span-2">
          {loading ? (
            <Spinner />
          ) : (
            <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
              {categories.length === 0 ? (
                <p className="p-8 text-center text-gray-500 text-sm">
                  No categories yet.
                </p>
              ) : (
                <table className="w-full text-sm">
                  <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
                    <tr>
                      {["Name", "Description", "Products", ""].map((h) => (
                        <th
                          key={h}
                          className="px-4 py-3 text-left font-medium whitespace-nowrap"
                        >
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {categories.map((cat) => (
                      <tr key={cat.id} className="hover:bg-gray-50">
                        <td className="px-4 py-3 font-medium text-gray-800">
                          {cat.name}
                        </td>
                        <td className="px-4 py-3 text-gray-500 max-w-xs truncate">
                          {cat.description || "—"}
                        </td>
                        <td className="px-4 py-3">
                          <span
                            className={`text-xs font-semibold px-2 py-0.5 rounded-full ${
                              cat.productCount > 0
                                ? "bg-indigo-50 text-indigo-700"
                                : "bg-gray-100 text-gray-500"
                            }`}
                          >
                            {cat.productCount}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <button
                            onClick={() => setDeleteTarget(cat.id)}
                            disabled={cat.productCount > 0}
                            title={
                              cat.productCount > 0
                                ? "Reassign or delete all products first"
                                : "Delete category"
                            }
                            className="text-xs text-red-500 hover:text-red-600 disabled:opacity-30 disabled:cursor-not-allowed"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </div>
      </div>

      <ConfirmDialog
        isOpen={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Category"
        message="This will permanently delete the category. Are you sure?"
        confirmLabel="Delete"
        danger
      />
    </div>
  );
}
