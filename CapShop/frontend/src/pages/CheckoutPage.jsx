import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useCartStore } from "../app/cartStore";
import { useOrderStore } from "../app/orderStore";
import Stepper from "../components/ui/Stepper";
import StepAddress from "../features/checkout/StepAddress";
import StepDelivery from "../features/checkout/StepDelivery";
import StepPayment from "../features/checkout/StepPayment";
import StepReview from "../features/checkout/StepReview";
import toast from "react-hot-toast";

const STEPS = ["Address", "Delivery", "Payment", "Review"];

export default function CheckoutPage() {
  const navigate = useNavigate();
  const { items, total, fetchCart, clearLocal } = useCartStore();
  const { checkout, simulatePayment } = useOrderStore();

  const [step, setStep] = useState(1);
  const [address, setAddress] = useState({
    shippingAddress: "",
    shippingCity: "",
    shippingPincode: "",
  });
  const [paymentMethod, setPaymentMethod] = useState("");
  const [paymentSuccess, setPaymentSuccess] = useState(false);
  const [placedOrder, setPlacedOrder] = useState(null);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  const handleAddressNext = (data) => {
    setAddress(data);
    setStep(2);
  };

  const handleDeliveryNext = () => setStep(3);

  const handlePaymentNext = async (method) => {
    setPaymentMethod(method);
    setProcessing(true);
    try {
      // Place order first (checkout), then simulate payment
      const order = await checkout(address);
      const payResult = await simulatePayment(order.id, method);

      if (payResult.status === "Failed") {
        toast.error("Payment failed. Please try again.");
        setProcessing(false);
        return;
      }

      setPlacedOrder(order);
      setPaymentSuccess(true);
      setStep(4);
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Checkout failed.");
    } finally {
      setProcessing(false);
    }
  };

  const handlePlaceOrder = () => {
    clearLocal();
    navigate(`/orders/${placedOrder.id}/confirmation`);
  };

  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900 mb-8 text-center">Checkout</h1>
      <Stepper steps={STEPS} currentStep={step} />

      {step === 1 && (
        <StepAddress defaultValues={address} onNext={handleAddressNext} />
      )}
      {step === 2 && (
        <StepDelivery
          items={items}
          total={total}
          onNext={handleDeliveryNext}
          onBack={() => setStep(1)}
        />
      )}
      {step === 3 && (
        <StepPayment
          onNext={handlePaymentNext}
          onBack={() => setStep(2)}
          processing={processing}
        />
      )}
      {step === 4 && (
        <StepReview
          address={address}
          paymentMethod={paymentMethod}
          paymentSuccess={paymentSuccess}
          order={placedOrder}
          onPlaceOrder={handlePlaceOrder}
        />
      )}
    </div>
  );
}
