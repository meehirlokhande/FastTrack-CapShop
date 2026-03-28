import { formatCurrency } from "../../utils/formatCurrency";

export default function StepDelivery({ items, total, onNext, onBack }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-5">Order Summary</h2>

      <div className="space-y-3 mb-5">
        {items.map((item) => (
          <div key={item.id} className="flex items-center gap-4">
            <img
              src={item.imageUrl || "https://placehold.co/56x56?text=N/A"}
              alt={item.productName}
              className="w-14 h-14 object-cover rounded-lg shrink-0"
            />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-800 truncate">{item.productName}</p>
              <p className="text-xs text-gray-500">Qty: {item.quantity}</p>
            </div>
            <p className="text-sm font-semibold text-gray-900 shrink-0">
              {formatCurrency(item.subtotal)}
            </p>
          </div>
        ))}
      </div>

      <div className="border-t pt-4 mb-4">
        <div className="flex justify-between text-sm text-gray-600 mb-1">
          <span>Subtotal</span>
          <span>{formatCurrency(total)}</span>
        </div>
        <div className="flex justify-between text-sm text-gray-600">
          <span>Delivery</span>
          <span className="text-green-600">FREE</span>
        </div>
        <div className="flex justify-between font-bold text-gray-900 mt-3 text-base">
          <span>Total</span>
          <span>{formatCurrency(total)}</span>
        </div>
      </div>

      <div className="flex gap-3 justify-between mt-6">
        <button
          onClick={onBack}
          className="px-6 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50"
        >
          Back
        </button>
        <button
          onClick={onNext}
          className="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-6 py-2.5 rounded-lg"
        >
          Next: Payment
        </button>
      </div>
    </div>
  );
}
