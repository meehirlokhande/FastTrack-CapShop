export default function ProductFilters({ filters, categories, onChange }) {
  const sortOptions = [
    { value: "", label: "Default" },
    { value: "price_asc", label: "Price: Low to High" },
    { value: "price_desc", label: "Price: High to Low" },
    { value: "name_asc", label: "Name: A-Z" },
  ];

  return (
    <aside className="w-full lg:w-64 shrink-0 space-y-6">
      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Category</h3>
        <div className="space-y-2">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="radio"
              name="category"
              value=""
              checked={!filters.category}
              onChange={() => onChange({ category: "" })}
              className="accent-indigo-600"
            />
            <span className="text-sm text-gray-700">All</span>
          </label>
          {categories.map((cat) => (
            <label key={cat.id} className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="category"
                value={cat.name}
                checked={filters.category === cat.name}
                onChange={() => onChange({ category: cat.name })}
                className="accent-indigo-600"
              />
              <span className="text-sm text-gray-700">{cat.name}</span>
            </label>
          ))}
        </div>
      </div>

      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Price Range</h3>
        <div className="flex gap-2">
          <input
            type="number"
            placeholder="Min"
            value={filters.minPrice ?? ""}
            onChange={(e) =>
              onChange({ minPrice: e.target.value || undefined })
            }
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
          <input
            type="number"
            placeholder="Max"
            value={filters.maxPrice ?? ""}
            onChange={(e) =>
              onChange({ maxPrice: e.target.value || undefined })
            }
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>
      </div>

      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Sort By</h3>
        <select
          value={filters.sort ?? ""}
          onChange={(e) => onChange({ sort: e.target.value || undefined })}
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
        >
          {sortOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>

      <button
        onClick={() => onChange({ category: "", minPrice: undefined, maxPrice: undefined, sort: undefined })}
        className="w-full text-sm text-indigo-600 hover:text-indigo-700 font-medium"
      >
        Clear Filters
      </button>
    </aside>
  );
}
