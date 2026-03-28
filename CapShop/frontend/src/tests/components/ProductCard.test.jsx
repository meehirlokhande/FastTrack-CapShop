import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import ProductCard from "../../components/product/ProductCard";

const baseProduct = {
  id: "abc-123",
  name: "Test Headphones",
  price: 2000,
  discountPrice: null,
  imageUrl: "",
  categoryName: "Electronics",
  stock: 10,
};

function renderCard(props = {}) {
  return render(
    <MemoryRouter>
      <ProductCard product={{ ...baseProduct, ...props }} />
    </MemoryRouter>
  );
}

describe("ProductCard", () => {
  it("renders product name and category", () => {
    renderCard();
    expect(screen.getByText("Test Headphones")).toBeInTheDocument();
    expect(screen.getByText("Electronics")).toBeInTheDocument();
  });

  it("shows regular price when no discount", () => {
    renderCard();
    expect(screen.getByText("₹2,000")).toBeInTheDocument();
  });

  it("shows discount price and strikethrough when discounted", () => {
    renderCard({ discountPrice: 1500 });
    expect(screen.getByText("₹1,500")).toBeInTheDocument();
    expect(screen.getByText("₹2,000")).toBeInTheDocument();
  });

  it("shows discount percentage badge", () => {
    renderCard({ discountPrice: 1000 });
    expect(screen.getByText("-50%")).toBeInTheDocument();
  });

  it("shows out of stock overlay when stock is 0", () => {
    renderCard({ stock: 0 });
    expect(screen.getByText("Out of Stock")).toBeInTheDocument();
  });

  it("links to correct product detail page", () => {
    renderCard();
    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/shop/abc-123");
  });
});
