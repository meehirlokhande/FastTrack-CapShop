import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "../app/authStore";
import toast from "react-hot-toast";

const METHOD_LABEL = {
  Authenticator: "Enter the 6-digit code from your authenticator app.",
  Email: "Enter the 6-digit code sent to your email.",
  Sms: "Enter the 6-digit code sent to your phone.",
};

export default function LoginPage() {
  const { login, verifyTwoFactor, resendTwoFactor, cancelTwoFactor, pendingTwoFactor } =
    useAuthStore();
  const navigate = useNavigate();

  const [form, setForm] = useState({ email: "", password: "" });
  const [otp, setOtp] = useState("");
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);

  const handleChange = (e) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  const handleLogin = async (e) => {
    e.preventDefault();
    if (!form.email || !form.password) {
      toast.error("Email and password are required.");
      return;
    }
    setLoading(true);
    try {
      const result = await login(form.email, form.password);
      if (!result.requiresTwoFactor) {
        toast.success("Logged in successfully.");
        navigate(result.role === "Admin" ? "/admin" : "/");
      }
      // If requires2FA, the store sets pendingTwoFactor — component re-renders to OTP step
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Invalid credentials.");
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async (e) => {
    e.preventDefault();
    if (!otp || otp.length !== 6) {
      toast.error("Enter the 6-digit code.");
      return;
    }
    setLoading(true);
    try {
      const role = await verifyTwoFactor(otp);
      toast.success("Logged in successfully.");
      navigate(role === "Admin" ? "/admin" : "/");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Invalid or expired code.");
      setOtp("");
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    setResending(true);
    try {
      await resendTwoFactor();
      toast.success("Code resent.");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Failed to resend code.");
    } finally {
      setResending(false);
    }
  };

  const handleCancel = () => {
    cancelTwoFactor();
    setOtp("");
  };

  // OTP verification step
  if (pendingTwoFactor) {
    const method = pendingTwoFactor.method;
    const showResend = method === "Email" || method === "Sms";

    return (
      <div className="min-h-[80vh] flex items-center justify-center px-4">
        <div className="w-full max-w-md bg-white rounded-2xl shadow-md p-8">
          <h2 className="text-2xl font-bold text-gray-900 mb-1">Two-Factor Verification</h2>
          <p className="text-sm text-gray-500 mb-6">
            {METHOD_LABEL[method] ?? "Enter your verification code."}
          </p>

          <form onSubmit={handleVerifyOtp} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Verification Code
              </label>
              <input
                type="text"
                inputMode="numeric"
                maxLength={6}
                value={otp}
                onChange={(e) => setOtp(e.target.value.replace(/\D/g, ""))}
                placeholder="000000"
                className="w-full border border-gray-300 rounded-lg px-4 py-2.5 text-sm text-center tracking-[0.4em] font-mono focus:outline-none focus:ring-2 focus:ring-action-main/50"
                autoFocus
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-action-main hover:bg-action-hover text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-60"
            >
              {loading ? "Verifying..." : "Verify"}
            </button>
          </form>

          <div className="mt-4 flex items-center justify-between text-sm">
            <button
              onClick={handleCancel}
              className="text-gray-500 hover:text-gray-700"
            >
              Back to login
            </button>
            {showResend && (
              <button
                onClick={handleResend}
                disabled={resending}
                className="text-action-main hover:text-action-hover font-medium disabled:opacity-50"
              >
                {resending ? "Sending..." : "Resend code"}
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }

  // Standard login step
  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-md p-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-1">Welcome back</h2>
        <p className="text-sm text-gray-500 mb-6">Sign in to your account</p>

        <form onSubmit={handleLogin} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              placeholder="you@example.com"
              className="w-full border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-action-main/50"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              placeholder="••••••••"
              className="w-full border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-action-main/50"
            />
          </div>
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-action-main hover:bg-action-hover text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-60"
          >
            {loading ? "Signing in..." : "Sign In"}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-500">
          Don&apos;t have an account?{" "}
          <Link to="/signup" className="text-action-main font-medium hover:underline">
            Sign Up
          </Link>
        </p>
      </div>
    </div>
  );
}
