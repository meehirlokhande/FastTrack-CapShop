import { render, screen, fireEvent } from "@testing-library/react";
import StepPayment from "../../features/checkout/StepPayment";

describe("StepPayment", () => {
  it("renders all payment options", () => {
    render(<StepPayment onNext={() => {}} onBack={() => {}} processing={false} />);
    expect(screen.getByText("UPI")).toBeInTheDocument();
    expect(screen.getByText("Card")).toBeInTheDocument();
    expect(screen.getByText("Cash on Delivery")).toBeInTheDocument();
  });

  it("calls onBack when Back button clicked", () => {
    const onBack = jest.fn();
    render(<StepPayment onNext={() => {}} onBack={onBack} processing={false} />);
    fireEvent.click(screen.getByText("Back"));
    expect(onBack).toHaveBeenCalled();
  });

  it("does not call onNext when no payment selected", () => {
    const onNext = jest.fn();
    render(<StepPayment onNext={onNext} onBack={() => {}} processing={false} />);
    fireEvent.click(screen.getByText("Place Order"));
    expect(onNext).not.toHaveBeenCalled();
  });

  it("calls onNext with selected payment method", () => {
    const onNext = jest.fn();
    render(<StepPayment onNext={onNext} onBack={() => {}} processing={false} />);
    fireEvent.click(screen.getByText("UPI"));
    fireEvent.click(screen.getByText("Place Order"));
    expect(onNext).toHaveBeenCalledWith("UPI");
  });

  it("disables buttons when processing", () => {
    render(<StepPayment onNext={() => {}} onBack={() => {}} processing={true} />);
    expect(screen.getByText("Processing...")).toBeDisabled();
    expect(screen.getByText("Back")).toBeDisabled();
  });
});
