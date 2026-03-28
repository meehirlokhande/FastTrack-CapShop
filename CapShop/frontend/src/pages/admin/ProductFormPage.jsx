import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { adminApi } from "../../api/adminApi";
import { catalogApi } from "../../api/catalogApi";
import Spinner from "../../components/ui/Spinner";
import toast from "react-hot-toast";

const EMPTY = {
  name: "",
  description: "",
  price: "",
  discountPrice: "",
  stock: "",
  imageUrl: "",
  isFeatured: false,
  categoryId: "",
};

export default function ProductFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [form, setForm] = useState(EMPTY);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    catalogApi.getCategories().then(({ data }) => setCategories(data));
  }, []);

  useEffect(() => {
    if (!isEdit) return;
    adminApi
      .getProductById(id)
      .then(({ data }) =>
        setForm({
          name: data.name,
          description: data.description,
          price: String(data.price),
          discountPrice: data.discountPrice != null ? String(data.discountPrice) : "",
          stock: String(data.stock),
          imageUrl: data.imageUrl,
          isFeatured: data.isFeatured,
          categoryId: "",
        })
      )
      .catch(() => toast.error("Failed to load product."))
      .finally(() => setLoading(false));
  }, [id, isEdit]);

  const handleChange = (e) => {
    const { name, type, value, checked } = e.target;
    setForm((prev) => ({ ...prev, [name]: type === "checkbox" ? checked : value }));
  };

  const validate = () => {
    if (!form.name.trim()) return "Name is required.";
    if (!form.price || isNaN(Number(form.price))) return "Valid price is required.";
    if (!form.stock || isNaN(Number(form.stock))) return "Valid stock is required.";
    if (!form.categoryId) return "Category is required.";
    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const err = validate();
    if (err) { toast.error(err); return; }

    const payload = {
      name: form.name,
      description: form.description,
      price: parseFloat(form.price),
      discountPrice: form.discountPrice ? parseFloat(form.discountPrice) : null,
      stock: parseInt(form.stock, 10),
      imageUrl: form.imageUrl,
      isFeatured: form.isFeatured,
      categoryId: parseInt(form.categoryId, 10),
    };

    setSaving(true);
    try {
      if (isEdit) {
        await adminApi.updateProduct(id, payload);
        toast.success("Product updated.");
      } else {
        await adminApi.createProduct(payload);
        toast.success("Product created.");
      }
      navigate("/admin/products");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Failed to save product.");
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Spinner />;

  return (
    <div className="max-w-2xl">
      <h1 className="text-xl font-bold text-gray-900 mb-6">
        {isEdit ? "Edit Product" : "Add Product"}
      </h1>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 space-y-4">
        {[
          { label: "Product Name", name: "name", type: "text" },
          { label: "Image URL", name: "imageUrl", type: "url" },
          { label: "Price (₹)", name: "price", type: "number" },
          { label: "Discount Price (₹)", name: "discountPrice", type: "number" },
          { label: "Stock", name: "stock", type: "number" },
        ].map(({ label, name, type }) => (
          <div key={name}>
            <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
            <input
              type={type}
              name={name}
              value={form[name]}
              onChange={handleChange}
              className="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
        ))}

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
          <textarea
            name="description"
            value={form.description}
            onChange={handleChange}
            rows={3}
            className="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
          <select
            name="categoryId"
            value={form.categoryId}
            onChange={handleChange}
            className="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          >
            <option value="">Select category</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            name="isFeatured"
            checked={form.isFeatured}
            onChange={handleChange}
            className="accent-indigo-600 w-4 h-4"
          />
          <span className="text-sm text-gray-700">Featured product</span>
        </label>

        <div className="flex justify-end gap-3 pt-2">
          <button
            type="button"
            onClick={() => navigate("/admin/products")}
            className="px-5 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={saving}
            className="px-5 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-60"
          >
            {saving ? "Saving..." : isEdit ? "Update Product" : "Create Product"}
          </button>
        </div>
      </form>
    </div>
  );
}
