import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { QRCode } from "react-qr-code";
import { authApi } from "../api/authApi";
import toast from "react-hot-toast";
import Spinner from "../components/ui/Spinner";
import AccountSubNav from "../components/account/AccountSubNav";

const METHODS = [
  {
    id: "Authenticator",
    label: "Authenticator App",
    description: "Use Google Authenticator, Microsoft Authenticator, or any TOTP app.",
  },
  {
    id: "Email",
    label: "Email OTP",
    description: "Receive a 6-digit code on your registered email address.",
  },
  {
    id: "Sms",
    label: "SMS OTP",
    description: "Receive a 6-digit code on your registered phone number.",
  },
];

export default function AccountSecurityPage() {
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(true);

  // totp setup state
  const [totpSetup, setTotpSetup] = useState(null); // { secret, qrCodeUri }
  const [totpCode, setTotpCode] = useState("");

  // email/sms setup state
  const [otpMethodPending, setOtpMethodPending] = useState(null); // "Email" | "Sms"
  const [otpCode, setOtpCode] = useState("");

  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    loadStatus();
  }, []);

  const loadStatus = async () => {
    setLoading(true);
    try {
      const { data } = await authApi.getTwoFactorStatus();
      setStatus(data);
    } catch {
      toast.error("Failed to load security settings.");
    } finally {
      setLoading(false);
    }
  };

  const handleSetup = async (methodId) => {
    setSubmitting(true);
    try {
      if (methodId === "Authenticator") {
        const { data } = await authApi.setupTotp();
        setTotpSetup(data);
        setTotpCode("");
      } else if (methodId === "Email") {
        await authApi.setupEmail();
        setOtpMethodPending("Email");
        setOtpCode("");
        toast.success("Code sent to your email.");
      } else if (methodId === "Sms") {
        await authApi.setupSms();
        setOtpMethodPending("Sms");
        setOtpCode("");
        toast.success("Code sent to your phone.");
      }
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Failed to start setup.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleConfirmTotp = async (e) => {
    e.preventDefault();
    if (totpCode.length !== 6) { toast.error("Enter the 6-digit code."); return; }
    setSubmitting(true);
    try {
      await authApi.confirmTotp(totpCode);
      toast.success("Authenticator app enabled.");
      setTotpSetup(null);
      setTotpCode("");
      await loadStatus();
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Invalid code.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleConfirmOtp = async (e) => {
    e.preventDefault();
    if (otpCode.length !== 6) { toast.error("Enter the 6-digit code."); return; }
    setSubmitting(true);
    try {
      if (otpMethodPending === "Email") {
        await authApi.confirmEmail(otpCode);
        toast.success("Email OTP enabled.");
      } else {
        await authApi.confirmSms(otpCode);
        toast.success("SMS OTP enabled.");
      }
      setOtpMethodPending(null);
      setOtpCode("");
      await loadStatus();
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Invalid code.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDisable = async () => {
    if (!window.confirm("Disable two-factor authentication? Your account will be less secure.")) return;
    setSubmitting(true);
    try {
      await authApi.disableTwoFactor();
      toast.success("Two-factor authentication disabled.");
      setTotpSetup(null);
      setOtpMethodPending(null);
      await loadStatus();
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Failed to disable 2FA.");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div className="flex justify-center py-20"><Spinner /></div>;

  return (
    <div className="max-w-2xl mx-auto px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900 mb-1">Account</h1>
      <p className="text-sm text-gray-500 mb-6">
        Security and two-factor authentication.{" "}
        <Link to="/account/profile" className="text-indigo-600 hover:underline font-medium">
          Edit profile
        </Link>
      </p>

      <AccountSubNav />

      <h2 className="text-lg font-semibold text-gray-900 mb-1">Security Settings</h2>
      <p className="text-sm text-gray-500 mb-8">Manage two-factor authentication for your account.</p>

      {/* Current Status */}
      <div className={`rounded-xl border p-5 mb-8 flex items-center justify-between ${
        status?.enabled ? "bg-green-50 border-green-200" : "bg-gray-50 border-gray-200"
      }`}>
        <div>
          <p className="font-semibold text-gray-900">
            Two-Factor Authentication: {status?.enabled ? (
              <span className="text-green-700">Enabled</span>
            ) : (
              <span className="text-gray-500">Disabled</span>
            )}
          </p>
          {status?.enabled && (
            <p className="text-sm text-gray-500 mt-0.5">Method: {status.method}</p>
          )}
        </div>
        {status?.enabled && (
          <button
            onClick={handleDisable}
            disabled={submitting}
            className="text-sm text-red-600 hover:text-red-700 font-medium disabled:opacity-50"
          >
            Disable
          </button>
        )}
      </div>

      {/* TOTP QR Setup */}
      {totpSetup && (
        <div className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
          <h3 className="font-semibold text-gray-900 mb-2">Scan QR Code</h3>
          <p className="text-sm text-gray-500 mb-4">
            Open your authenticator app and scan this QR code, then enter the 6-digit code below.
          </p>
          <div className="flex justify-center mb-4 p-4 bg-white border border-gray-100 rounded-lg">
            <QRCode value={totpSetup.qrCodeUri} size={180} />
          </div>
          <p className="text-xs text-gray-400 mb-4 text-center break-all">
            Manual key: <span className="font-mono">{totpSetup.secret}</span>
          </p>
          <form onSubmit={handleConfirmTotp} className="flex gap-3">
            <input
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={totpCode}
              onChange={(e) => setTotpCode(e.target.value.replace(/\D/g, ""))}
              placeholder="000000"
              className="flex-1 border border-gray-300 rounded-lg px-4 py-2 text-sm font-mono text-center tracking-widest focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
            <button
              type="submit"
              disabled={submitting}
              className="bg-indigo-600 text-white px-5 py-2 rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-60"
            >
              {submitting ? "Verifying..." : "Confirm"}
            </button>
            <button
              type="button"
              onClick={() => setTotpSetup(null)}
              className="text-gray-400 hover:text-gray-600 text-sm"
            >
              Cancel
            </button>
          </form>
        </div>
      )}

      {/* Email/SMS OTP Confirm */}
      {otpMethodPending && (
        <div className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
          <h3 className="font-semibold text-gray-900 mb-2">Enter Verification Code</h3>
          <p className="text-sm text-gray-500 mb-4">
            {otpMethodPending === "Email"
              ? "Enter the code sent to your email."
              : "Enter the code sent to your phone."}
          </p>
          <form onSubmit={handleConfirmOtp} className="flex gap-3">
            <input
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={otpCode}
              onChange={(e) => setOtpCode(e.target.value.replace(/\D/g, ""))}
              placeholder="000000"
              className="flex-1 border border-gray-300 rounded-lg px-4 py-2 text-sm font-mono text-center tracking-widest focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
            <button
              type="submit"
              disabled={submitting}
              className="bg-indigo-600 text-white px-5 py-2 rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-60"
            >
              {submitting ? "Verifying..." : "Confirm"}
            </button>
            <button
              type="button"
              onClick={() => setOtpMethodPending(null)}
              className="text-gray-400 hover:text-gray-600 text-sm"
            >
              Cancel
            </button>
          </form>
        </div>
      )}

      {/* Method Selection — show only if 2FA not enabled and no setup in progress */}
      {!status?.enabled && !totpSetup && !otpMethodPending && (
        <div>
          <h2 className="text-base font-semibold text-gray-800 mb-4">Choose a method to enable</h2>
          <div className="space-y-3">
            {METHODS.map((m) => (
              <div
                key={m.id}
                className="flex items-center justify-between border border-gray-200 rounded-xl p-5 hover:border-indigo-300 transition-colors"
              >
                <div>
                  <p className="font-medium text-gray-900">{m.label}</p>
                  <p className="text-sm text-gray-500 mt-0.5">{m.description}</p>
                </div>
                <button
                  onClick={() => handleSetup(m.id)}
                  disabled={submitting}
                  className="ml-4 text-sm font-medium text-indigo-600 hover:text-indigo-700 disabled:opacity-50 whitespace-nowrap"
                >
                  Enable
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
