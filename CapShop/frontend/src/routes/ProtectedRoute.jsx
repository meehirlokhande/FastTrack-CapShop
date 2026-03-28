import { Navigate, Outlet } from "react-router-dom";
import { useAuthStore } from "../app/authStore";
import Spinner from "../components/ui/Spinner";

export default function ProtectedRoute() {
  const token = useAuthStore((s) => s.token);
  const isLoading = useAuthStore((s) => s.isLoading);

  if (isLoading) return <Spinner />;
  if (!token) return <Navigate to="/login" replace />;
  return <Outlet />;
}
