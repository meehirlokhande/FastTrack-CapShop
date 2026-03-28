import { Navigate, Outlet } from "react-router-dom";
import { useAuthStore } from "../app/authStore";
import Spinner from "../components/ui/Spinner";

export default function AdminRoute() {
  const token = useAuthStore((s) => s.token);
  const role = useAuthStore((s) => s.role);
  const isLoading = useAuthStore((s) => s.isLoading);

  if (isLoading) return <Spinner />;
  if (!token) return <Navigate to="/login" replace />;
  if (role !== "Admin") return <Navigate to="/" replace />;
  return <Outlet />;
}
