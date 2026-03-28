import { useState } from "react";
import toast from "react-hot-toast";

export default function StepAddress({ defaultValues, onNext }) {
  const [form, setForm] = useState(defaultValues);

  const handleChange = (e) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  const handleNext = () => {
    if (!form.shippingAddress.trim()) {
      toast.error("Street address is required."); return;
    }
    if (!form.shippingCity.trim()) {
      toast.error("City is required."); return;
    }
    if (!/^\d{6}$/.test(form.shippingPincode)) {
      toast.error("Enter a valid 6-digit pincode."); return;
    }
    onNext(form);
  };

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-5">Shipping Address</h2>
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Street Address
          </label>
          <input
            name="shippingAddress"
            value={form.shippingAddress}
            onChange={handleChange}
            placeholder="123, Main Street"
            className="w-full border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">City</label>
            <input
              name="shippingCity"
              value={form.shippingCity}
              onChange={handleChange}
              placeholder="Mumbai"
              className="w-full border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Pincode
            </label>
            <input
              name="shippingPincode"
              value={form.shippingPincode}
              onChange={handleChange}
              placeholder="400001"
              maxLength={6}
              className="w-full border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
        </div>
      </div>
      <div className="mt-6 flex justify-end">
        <button
          onClick={handleNext}
          className="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-6 py-2.5 rounded-lg"
        >
          Next: Delivery
        </button>
      </div>
    </div>
  );
}
