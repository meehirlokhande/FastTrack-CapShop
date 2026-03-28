import { Link, Outlet } from "react-router-dom";

export default function AuthLayout() {
  return (
    <div className="min-h-screen bg-gray-50">
      <div className="py-5 text-center bg-brand-primary border-b border-brand-secondary">
        <Link to="/" className="text-2xl font-bold text-action-main tracking-tight">
          CapShop
        </Link>
      </div>
      <Outlet />
    </div>
  );
}
