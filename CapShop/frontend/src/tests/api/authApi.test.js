jest.mock("../../app/axiosInstance", () => ({
  __esModule: true,
  default: { post: jest.fn(), get: jest.fn() },
}));

import api from "../../app/axiosInstance";
import { authApi } from "../../api/authApi";

describe("authApi", () => {
  afterEach(() => jest.clearAllMocks());

  it("calls login endpoint with credentials", async () => {
    api.post.mockResolvedValue({ data: { token: "tok", role: "Customer" } });
    const res = await authApi.login({ email: "a@b.com", password: "pass" });
    expect(api.post).toHaveBeenCalledWith("/auth/login", {
      email: "a@b.com",
      password: "pass",
    });
    expect(res.data.token).toBe("tok");
  });

  it("calls signup endpoint with payload", async () => {
    api.post.mockResolvedValue({ data: { message: "Created" } });
    await authApi.signup({
      fullName: "Test",
      email: "t@t.com",
      phoneNumber: "123",
      password: "pass",
    });
    expect(api.post).toHaveBeenCalledWith(
      "/auth/signup",
      expect.objectContaining({ fullName: "Test" })
    );
  });

  it("calls me endpoint", async () => {
    api.get.mockResolvedValue({
      data: { fullName: "Test", email: "t@t.com", role: "Customer" },
    });
    const res = await authApi.me();
    expect(api.get).toHaveBeenCalledWith("/auth/me");
    expect(res.data.role).toBe("Customer");
  });
});
