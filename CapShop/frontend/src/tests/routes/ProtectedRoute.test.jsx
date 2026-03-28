import { render, screen } from "@testing-library/react";
import { MemoryRouter, Routes, Route } from "react-router-dom";

// Mock the auth store before importing the component
jest.mock("../../app/authStore");
import { useAuthStore } from "../../app/authStore";
import ProtectedRoute from "../../routes/ProtectedRoute";

function renderWithRouter(token, isLoading = false) {
  useAuthStore.mockImplementation((selector) =>
    selector({ token, isLoading })
  );

  return render(
    <MemoryRouter initialEntries={["/protected"]}>
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route path="/protected" element={<div>Protected Content</div>} />
        </Route>
        <Route path="/login" element={<div>Login Page</div>} />
      </Routes>
    </MemoryRouter>
  );
}

describe("ProtectedRoute", () => {
  it("renders protected content when token exists", () => {
    renderWithRouter("valid-token");
    expect(screen.getByText("Protected Content")).toBeInTheDocument();
  });

  it("redirects to login when no token", () => {
    renderWithRouter(null);
    expect(screen.getByText("Login Page")).toBeInTheDocument();
    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
  });

  it("shows spinner while loading", () => {
    useAuthStore.mockImplementation((selector) =>
      selector({ token: null, isLoading: true })
    );

    render(
      <MemoryRouter initialEntries={["/protected"]}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/protected" element={<div>Protected Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
  });
});
