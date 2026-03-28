import { render, screen, fireEvent } from "@testing-library/react";
import StepAddress from "../../features/checkout/StepAddress";

const defaults = { shippingAddress: "", shippingCity: "", shippingPincode: "" };

describe("StepAddress", () => {
  it("renders all address fields", () => {
    render(<StepAddress defaultValues={defaults} onNext={() => {}} />);
    expect(screen.getByPlaceholderText("123, Main Street")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Mumbai")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("400001")).toBeInTheDocument();
  });

  it("calls onNext with address data on valid submit", () => {
    const onNext = jest.fn();
    render(<StepAddress defaultValues={defaults} onNext={onNext} />);

    fireEvent.change(screen.getByPlaceholderText("123, Main Street"), {
      target: { value: "42, Baker Street" },
    });
    fireEvent.change(screen.getByPlaceholderText("Mumbai"), {
      target: { value: "Delhi" },
    });
    fireEvent.change(screen.getByPlaceholderText("400001"), {
      target: { value: "110001" },
    });

    fireEvent.click(screen.getByText("Next: Delivery"));
    expect(onNext).toHaveBeenCalledWith({
      shippingAddress: "42, Baker Street",
      shippingCity: "Delhi",
      shippingPincode: "110001",
    });
  });

  it("does not call onNext when address is empty", () => {
    const onNext = jest.fn();
    render(<StepAddress defaultValues={defaults} onNext={onNext} />);
    fireEvent.click(screen.getByText("Next: Delivery"));
    expect(onNext).not.toHaveBeenCalled();
  });

  it("does not call onNext when pincode is invalid", () => {
    const onNext = jest.fn();
    render(<StepAddress defaultValues={defaults} onNext={onNext} />);
    fireEvent.change(screen.getByPlaceholderText("123, Main Street"), {
      target: { value: "Some Street" },
    });
    fireEvent.change(screen.getByPlaceholderText("Mumbai"), {
      target: { value: "Chennai" },
    });
    fireEvent.change(screen.getByPlaceholderText("400001"), {
      target: { value: "123" },
    });
    fireEvent.click(screen.getByText("Next: Delivery"));
    expect(onNext).not.toHaveBeenCalled();
  });
});
