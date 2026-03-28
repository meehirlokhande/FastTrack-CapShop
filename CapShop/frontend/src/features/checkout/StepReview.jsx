export default function StepReview({ address, paymentMethod, paymentSuccess, order, onPlaceOrder }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 text-center">
      <div className="text-5xl mb-4">{paymentSuccess ? "✅" : "❌"}</div>
      <h2 className="text-xl font-bold text-gray-900 mb-2">
        {paymentSuccess ? "Payment Successful!" : "Payment Failed"}
      </h2>
      <p className="text-gray-500 mb-6">
        {paymentSuccess
          ? "Your order has been placed successfully."
          : "Something went wrong with your payment."}
      </p>

      {paymentSuccess && order && (
        <div className="text-left bg-gray-50 rounded-xl p-4 mb-6 space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-500">Order ID</span>
            <span className="font-mono text-gray-800 text-xs">{order.id}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Shipping to</span>
            <span className="text-gray-800">
              {address.shippingCity}, {address.shippingPincode}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Payment</span>
            <span className="text-gray-800">{paymentMethod}</span>
          </div>
        </div>
      )}

      {paymentSuccess && (
        <button
          onClick={onPlaceOrder}
          className="bg-indigo-600 hover:bg-indigo-700 text-white font-medium px-8 py-2.5 rounded-lg"
        >
          View Order
        </button>
      )}
    </div>
  );
}
