import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { authApi } from "../api/authApi";
import { useAuthStore } from "../app/authStore";
import AccountSubNav from "../components/account/AccountSubNav";
import Spinner from "../components/ui/Spinner";
import toast from "react-hot-toast";
import { gatewayAssetUrl } from "../utils/gatewayAssetUrl";

export default function AccountProfilePage() {
  const fetchUser = useAuthStore((s) => s.fetchUser);

  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState(null);

  const [fullName, setFullName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [savingProfile, setSavingProfile] = useState(false);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [savingPassword, setSavingPassword] = useState(false);

  const [uploadingAvatar, setUploadingAvatar] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const { data } = await authApi.getProfile();
      setProfile(data);
      setFullName(data.fullName ?? "");
      setPhoneNumber(data.phoneNumber ?? "");
    } catch {
      toast.error("Failed to load profile.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleSaveProfile = async (e) => {
    e.preventDefault();
    setSavingProfile(true);
    try {
      const { data } = await authApi.updateProfile({ fullName, phoneNumber });
      setProfile(data);
      await fetchUser();
      toast.success("Profile saved.");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Could not save profile.");
    } finally {
      setSavingProfile(false);
    }
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();
    if (newPassword.length < 6) {
      toast.error("New password must be at least 6 characters.");
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error("New passwords do not match.");
      return;
    }
    setSavingPassword(true);
    try {
      await authApi.changePassword({ currentPassword, newPassword });
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      toast.success("Password updated.");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Could not change password.");
    } finally {
      setSavingPassword(false);
    }
  };

  const handleAvatarChange = async (e) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      toast.error("Image must be 2 MB or smaller.");
      return;
    }
    const formData = new FormData();
    formData.append("file", file);
    setUploadingAvatar(true);
    try {
      const { data } = await authApi.uploadAvatar(formData);
      setProfile(data);
      await fetchUser();
      toast.success("Photo updated.");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Could not upload photo.");
    } finally {
      setUploadingAvatar(false);
    }
  };

  const handleRemoveAvatar = async () => {
    if (!window.confirm("Remove your profile photo?")) return;
    setUploadingAvatar(true);
    try {
      const { data } = await authApi.removeAvatar();
      setProfile(data);
      await fetchUser();
      toast.success("Photo removed.");
    } catch (err) {
      toast.error(err.response?.data?.message ?? "Could not remove photo.");
    } finally {
      setUploadingAvatar(false);
    }
  };

  if (loading || !profile) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-10 flex justify-center">
        <Spinner />
      </div>
    );
  }

  const avatarSrc = gatewayAssetUrl(profile.profilePictureUrl);

  return (
    <div className="max-w-2xl mx-auto px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900 mb-1">Account</h1>
      <p className="text-sm text-gray-500 mb-6">
        Manage your profile and sign-in details.{" "}
        <Link to="/orders" className="text-indigo-600 hover:underline font-medium">
          View orders
        </Link>
      </p>

      <AccountSubNav />

      {/* Avatar */}
      <section className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
        <h2 className="text-base font-semibold text-gray-900 mb-4">Profile photo</h2>
        <div className="flex flex-col sm:flex-row sm:items-center gap-6">
          <div className="shrink-0">
            {avatarSrc ? (
              <img
                src={avatarSrc}
                alt=""
                className="w-24 h-24 rounded-full object-cover border border-gray-200"
              />
            ) : (
              <div className="w-24 h-24 rounded-full bg-indigo-100 text-indigo-700 flex items-center justify-center text-2xl font-bold border border-indigo-200">
                {(profile.fullName || profile.email || "?").charAt(0).toUpperCase()}
              </div>
            )}
          </div>
          <div className="flex flex-wrap items-center gap-3">
            <label className="cursor-pointer">
              <span className="inline-block bg-indigo-600 text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-indigo-700 disabled:opacity-50">
                {uploadingAvatar ? "Working…" : "Upload photo"}
              </span>
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp,image/gif"
                className="hidden"
                disabled={uploadingAvatar}
                onChange={handleAvatarChange}
              />
            </label>
            {profile.profilePictureUrl && (
              <button
                type="button"
                onClick={handleRemoveAvatar}
                disabled={uploadingAvatar}
                className="text-sm text-red-600 hover:text-red-700 font-medium disabled:opacity-50"
              >
                Remove photo
              </button>
            )}
          </div>
        </div>
        <p className="text-xs text-gray-500 mt-3">JPEG, PNG, WebP, or GIF. Max 2 MB.</p>
      </section>

      {/* Profile form */}
      <form onSubmit={handleSaveProfile} className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
        <h2 className="text-base font-semibold text-gray-900 mb-4">Personal information</h2>
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Full name</label>
            <input
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              required
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Phone</label>
            <input
              type="tel"
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Email</label>
            <input
              type="email"
              value={profile.email}
              disabled
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-600 cursor-not-allowed"
            />
            <p className="text-xs text-gray-400 mt-1">Email is used to sign in and cannot be changed here.</p>
          </div>
        </div>
        <button
          type="submit"
          disabled={savingProfile}
          className="mt-6 bg-indigo-600 text-white text-sm font-medium px-5 py-2 rounded-lg hover:bg-indigo-700 disabled:opacity-60"
        >
          {savingProfile ? "Saving…" : "Save changes"}
        </button>
      </form>

      {/* Password */}
      <form onSubmit={handleChangePassword} className="bg-white border border-gray-200 rounded-xl p-6">
        <h2 className="text-base font-semibold text-gray-900 mb-4">Change password</h2>
        <div className="space-y-4 max-w-md">
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Current password</label>
            <input
              type="password"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              autoComplete="current-password"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">New password</label>
            <input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              autoComplete="new-password"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Confirm new password</label>
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              autoComplete="new-password"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
            />
          </div>
        </div>
        <button
          type="submit"
          disabled={savingPassword}
          className="mt-6 bg-white border border-gray-300 text-gray-800 text-sm font-medium px-5 py-2 rounded-lg hover:bg-gray-50 disabled:opacity-60"
        >
          {savingPassword ? "Updating…" : "Update password"}
        </button>
      </form>
    </div>
  );
}
