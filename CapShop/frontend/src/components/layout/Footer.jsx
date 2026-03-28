import { Link } from "react-router-dom";

export default function Footer() {
  return (
    <footer className="bg-brand-primary border-t border-brand-secondary/20 mt-auto pt-10 pb-6">
      <div className="max-w-7xl mx-auto px-6 lg:px-8">

        <div className="grid grid-cols-1 md:grid-cols-4 gap-10 mb-10">

          {/* Brand */}
          <div className="space-y-3">
            <Link to="/" className="text-xl font-black text-action-main tracking-tight uppercase">
              CapShop
            </Link>
            <p className="text-text-body text-sm leading-relaxed pt-1">
              A fast, reliable online store built for everyday shoppers. Browse, order, and track — all in one place.
            </p>
          </div>

          {/* Shop */}
          <div className="flex flex-col gap-2.5">
            <h4 className="text-text-header font-semibold text-xs uppercase tracking-widest mb-1">Browse</h4>
            <Link to="/shop" className="text-text-body hover:text-action-main text-sm transition-colors">All Products</Link>
            <Link to="/shop?category=Helmets" className="text-text-body hover:text-action-main text-sm transition-colors">Helmets</Link>
            <Link to="/shop?category=Electronics" className="text-text-body hover:text-action-main text-sm transition-colors">Electronics</Link>
            <Link to="/shop?sort=newest" className="text-text-body hover:text-action-main text-sm transition-colors">New Arrivals</Link>
          </div>

          {/* Account */}
          <div className="flex flex-col gap-2.5">
            <h4 className="text-text-header font-semibold text-xs uppercase tracking-widest mb-1">Account</h4>
            <Link to="/login" className="text-text-body hover:text-action-main text-sm transition-colors">Sign In</Link>
            <Link to="/signup" className="text-text-body hover:text-action-main text-sm transition-colors">Create Account</Link>
            <Link to="/orders" className="text-text-body hover:text-action-main text-sm transition-colors">My Orders</Link>
            <Link to="/cart" className="text-text-body hover:text-action-main text-sm transition-colors">View Cart</Link>
          </div>

          {/* Trust */}
          <div className="flex flex-col gap-4">
            <h4 className="text-text-header font-semibold text-xs uppercase tracking-widest mb-1">Why CapShop</h4>
            {[
              { label: "Secure Payments", icon: "M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" },
              { label: "Order Tracking", icon: "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" },
              { label: "Easy Returns", icon: "M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" },
            ].map(({ label, icon }) => (
              <div key={label} className="flex items-center gap-3">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-action-success shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={icon} />
                </svg>
                <span className="text-text-body text-xs">{label}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Bottom bar */}
        <div className="border-t border-brand-secondary/10 pt-6 flex flex-col md:flex-row justify-between items-center gap-3">
          <p className="text-text-muted text-xs">
            &copy; {new Date().getFullYear()} CapShop. All rights reserved.
          </p>
          <div className="flex items-center gap-5">
            <span className="text-[11px] text-text-muted uppercase tracking-wide">Privacy Policy</span>
            <span className="text-[11px] text-text-muted uppercase tracking-wide">Terms of Use</span>
          </div>
        </div>

      </div>
    </footer>
  );
}