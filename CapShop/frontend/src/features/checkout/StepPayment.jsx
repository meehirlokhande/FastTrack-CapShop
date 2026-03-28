import { useState } from "react";
import toast from "react-hot-toast";
import { cn } from "../../utils/cn";

const PAYMENT_OPTIONS = [
  { id: "UPI", label: "UPI", icon: "📱", desc: "Pay via UPI / QR code" },
  { id: "Card", label: "Card", icon: "💳", desc: "Credit / Debit card" },
  { id: "COD", label: "Cash on Delivery", icon: "💵", desc: "Pay when delivered" },
];

export default function StepPayment({ onNext, onBack, processing }) {
  const [selected, setSelected] = useState("");

  const handleNext = () => {
    if (!selected) { toast.error("Please select a payment method."); return; }
    onNext(selected);
  };

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-5">Payment Method</h2>

      <div className="space-y-3 mb-6">
        {PAYMENT_OPTIONS.map((opt) => (
          <button
            key={opt.id}
            onClick={() => setSelected(opt.id)}
            className={cn(
              "w-full flex items-center gap-4 p-4 rounded-xl border-2 text-left transition-colors",
              selected === opt.id
                ? "border-indigo-600 bg-indigo-50"
                : "border-gray-200 hover:border-indigo-300"
            )}
          >
            <span className="text-2xl">{opt.icon}</span>
            <div>
              <p className="font-semibold text-gray-800">{opt.label}</p>
              <p className="text-sm text-gray-500">{opt.desc}</p>
            </div>
            <div
              className={cn(
                "ml-auto w-5 h-5 rounded-full border-2 transition-colors shrink-0",
                selected === opt.id
                  ? "border-indigo-600 bg-indigo-600"
                  : "border-gray-300"
              )}
            />
          </button>
        ))}
      </div>

      <p className="text-xs text-gray-400 mb-5">
        Payment is simulated. No real transaction will occur.
      </p>

      <div className="flex gap-3 justify-between">
        <button
          onClick={onBack}
          disabled={processing}
          className="px-6 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50 disabled:opacity-50"
        >
          Back
        </button>
        <button
          onClick={handleNext}
          disabled={processing}
          className="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-6 py-2.5 rounded-lg disabled:opacity-60"
        >
          {processing ? "Processing..." : "Place Order"}
        </button>
      </div>
    </div>
  );
}
