import { Link, useLocation } from "react-router-dom";

const linkClass = (active) =>
  `text-sm font-semibold pb-2 border-b-2 transition-colors ${
    active
      ? "text-indigo-600 border-indigo-600"
      : "text-gray-500 border-transparent hover:text-gray-800"
  }`;

export default function AccountSubNav() {
  const { pathname } = useLocation();
  return (
    <div className="flex gap-8 border-b border-gray-200 mb-8">
      <Link to="/account/profile" className={linkClass(pathname === "/account/profile")}>
        Profile
      </Link>
      <Link to="/account/security" className={linkClass(pathname === "/account/security")}>
        Security
      </Link>
    </div>
  );
}
