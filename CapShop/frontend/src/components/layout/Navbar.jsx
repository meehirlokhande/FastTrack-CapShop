import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "../../app/authStore";
import { gatewayAssetUrl } from "../../utils/gatewayAssetUrl";
import { useCartStore } from "../../app/cartStore";
import { useProductStore } from "../../app/productStore";

export default function Navbar() {
  const { user, role, token, logout } = useAuthStore();
  const itemCount = useCartStore((s) => s.itemCount);
  const { categories, fetchCategories } = useProductStore();
  const navigate = useNavigate();

  const [query, setQuery] = useState("");
  const [category, setCategory] = useState("");

  useEffect(() => {
    if (categories.length === 0) fetchCategories();
  }, [fetchCategories, categories.length]);

  const handleSearch = () => {
    const params = new URLSearchParams();
    if (query.trim()) params.set("query", query.trim());
    if (category) params.set("category", category);
    navigate(`/shop?${params.toString()}`);
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") handleSearch();
  };

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
   
    <nav className="bg-brand-primary/90 backdrop-blur-lg border-b border-brand-secondary/20 sticky top-0 z-50 py-2">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center gap-6 h-16">

          <Link to="/" className="text-2xl font-black text-action-main tracking-tighter shrink-0 uppercase">
            Cap<span className="text-text-header">Shop</span>
          </Link>

          
          <div className="flex flex-1 h-11 bg-ui-bg border border-brand-secondary/30 rounded-full overflow-hidden focus-within:ring-2 ring-action-main/50 transition-all">
            <select
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              className="bg-brand-secondary/10 text-text-body text-xs px-4 border-r border-brand-secondary/20 focus:outline-none cursor-pointer hover:bg-brand-secondary/20 shrink-0 appearance-none font-bold"
            >
              <option value="">All</option>
              {categories.map((c) => (
                <option key={c.id} value={c.name}>{c.name}</option>
              ))}
            </select>

            <input
              type="text"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Search the stealth collection..."
              className="flex-1 px-4 bg-transparent text-sm text-text-header placeholder:text-text-muted focus:outline-none"
            />

            <button
              onClick={handleSearch}
              className="bg-action-main hover:bg-action-hover px-5 transition-all shrink-0 flex items-center justify-center group"
              aria-label="Search"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-text-header group-hover:scale-110 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M21 21l-4.35-4.35M17 11A6 6 0 1 1 5 11a6 6 0 0 1 12 0z" />
              </svg>
            </button>
          </div>

          {/* RIGHT CONTROLS */}
          <div className="flex items-center gap-6 shrink-0">
            {token && role !== "Admin" && (
              <Link to="/orders" className="hidden lg:block text-text-body hover:text-action-main font-bold text-xs uppercase tracking-widest transition-colors">
                Orders
              </Link>
            )}

            {role === "Admin" && (
              <Link to="/admin" className="text-action-success font-bold text-xs uppercase tracking-widest border border-action-success/30 px-3 py-1 rounded-md">
                Admin
              </Link>
            )}

            {/* CART with Purple Glow */}
            {token && role !== "Admin" && (
              <Link to="/cart" className="relative text-text-header hover:text-action-main transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-7 w-7" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
                {itemCount > 0 && (
                  <span className="absolute -top-1 -right-1 bg-action-main text-text-header text-[10px] font-black rounded-full h-5 w-5 flex items-center justify-center shadow-[0_0_10px_rgba(127,90,240,0.6)]">
                    {itemCount}
                  </span>
                )}
              </Link>
            )}

            {token ? (
              <div className="flex items-center gap-4 border-l border-brand-secondary/20 pl-4">
                <div className="hidden sm:flex items-center gap-3">
                  <Link to="/account/profile" className="shrink-0">
                    {gatewayAssetUrl(user?.profilePictureUrl) ? (
                      <img
                        src={gatewayAssetUrl(user.profilePictureUrl)}
                        alt=""
                        className="h-9 w-9 rounded-full object-cover border border-brand-secondary/30"
                      />
                    ) : (
                      <div className="h-9 w-9 rounded-full bg-action-main/20 text-action-main flex items-center justify-center text-xs font-bold border border-brand-secondary/30">
                        {(user?.fullName || user?.email || "?").charAt(0).toUpperCase()}
                      </div>
                    )}
                  </Link>
                  <div className="flex flex-col items-end leading-none">
                    <Link to="/account/profile" className="text-[10px] text-text-muted font-bold uppercase hover:text-action-main transition-colors">
                      Account
                    </Link>
                    <span className="text-sm text-text-header font-medium">{user?.fullName?.split(" ")[0]}</span>
                  </div>
                </div>
                <button
                  onClick={handleLogout}
                  className="text-xs font-bold text-red-400 hover:text-red-300 transition-colors uppercase tracking-tighter"
                >
                  Sign Out
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-4">
                <Link to="/login" className="text-xs font-bold text-text-header hover:text-action-main uppercase tracking-widest">
                  Login
                </Link>
                <Link to="/signup" className="text-xs font-black bg-action-main text-text-header px-5 py-2.5 rounded-full uppercase shadow-[0_5px_15px_rgba(127,90,240,0.4)] hover:shadow-action-main/60 transition-all">
                  Sign Up
                </Link>
              </div>
            )}
          </div>

        </div>
      </div>
    </nav>
  );
}