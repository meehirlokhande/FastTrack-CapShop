import { render, screen, fireEvent } from "@testing-library/react";
import Pagination from "../../components/ui/Pagination";

describe("Pagination", () => {
  it("renders nothing when totalPages <= 1", () => {
    const { container } = render(
      <Pagination page={1} totalPages={1} onPageChange={() => {}} />
    );
    expect(container.firstChild).toBeNull();
  });

  it("renders page buttons", () => {
    render(<Pagination page={2} totalPages={4} onPageChange={() => {}} />);
    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("Prev")).toBeInTheDocument();
    expect(screen.getByText("Next")).toBeInTheDocument();
  });

  it("disables Prev on first page", () => {
    render(<Pagination page={1} totalPages={3} onPageChange={() => {}} />);
    expect(screen.getByText("Prev")).toBeDisabled();
  });

  it("disables Next on last page", () => {
    render(<Pagination page={3} totalPages={3} onPageChange={() => {}} />);
    expect(screen.getByText("Next")).toBeDisabled();
  });

  it("calls onPageChange with correct page when clicking a page button", () => {
    const onChange = jest.fn();
    render(<Pagination page={1} totalPages={3} onPageChange={onChange} />);
    fireEvent.click(screen.getByText("3"));
    expect(onChange).toHaveBeenCalledWith(3);
  });

  it("calls onPageChange when clicking Next", () => {
    const onChange = jest.fn();
    render(<Pagination page={2} totalPages={4} onPageChange={onChange} />);
    fireEvent.click(screen.getByText("Next"));
    expect(onChange).toHaveBeenCalledWith(3);
  });
});
