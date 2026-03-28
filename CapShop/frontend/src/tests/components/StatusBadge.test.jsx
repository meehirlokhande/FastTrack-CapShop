import { render, screen } from "@testing-library/react";
import StatusBadge from "../../components/ui/StatusBadge";

describe("StatusBadge", () => {
  const statuses = ["Pending", "Paid", "Packed", "Shipped", "Delivered", "Cancelled", "Active", "Inactive"];

  statuses.forEach((status) => {
    it(`renders ${status} badge`, () => {
      render(<StatusBadge status={status} />);
      expect(screen.getByText(status)).toBeInTheDocument();
    });
  });

  it("renders unknown status without crashing", () => {
    render(<StatusBadge status="Unknown" />);
    expect(screen.getByText("Unknown")).toBeInTheDocument();
  });
});
