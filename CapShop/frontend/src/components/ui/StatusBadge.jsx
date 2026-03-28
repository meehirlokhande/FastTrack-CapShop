const colorMap = {
  Pending: "bg-yellow-100 text-yellow-800",
  Paid: "bg-blue-100 text-blue-800",
  Packed: "bg-purple-100 text-purple-800",
  Shipped: "bg-orange-100 text-orange-800",
  Delivered: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
  Active: "bg-green-100 text-green-800",
  Inactive: "bg-gray-100 text-gray-600",
};

export default function StatusBadge({ status }) {
  const color = colorMap[status] ?? "bg-gray-100 text-gray-700";
  return (
    <span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-semibold ${color}`}>
      {status}
    </span>
  );
}
