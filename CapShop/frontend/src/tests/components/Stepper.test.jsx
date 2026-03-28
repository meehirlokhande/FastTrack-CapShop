import { render, screen } from "@testing-library/react";
import Stepper from "../../components/ui/Stepper";

const STEPS = ["Address", "Delivery", "Payment", "Review"];

describe("Stepper", () => {
  it("renders all step labels", () => {
    render(<Stepper steps={STEPS} currentStep={1} />);
    STEPS.forEach((step) => expect(screen.getByText(step)).toBeInTheDocument());
  });

  it("marks completed steps correctly at step 3", () => {
    render(<Stepper steps={STEPS} currentStep={3} />);
    // Step 1 and 2 should show checkmarks (no number), step 3 is active
    expect(screen.getByText("Payment")).toBeInTheDocument();
    expect(screen.queryByText("1")).not.toBeInTheDocument();
    expect(screen.queryByText("2")).not.toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
  });

  it("shows step number for current and future steps", () => {
    render(<Stepper steps={STEPS} currentStep={1} />);
    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
  });
});
