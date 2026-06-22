import { NavLink, Route, Routes } from "react-router-dom";
import { CatalogPage } from "../features/catalog/CatalogPage";
import { BasketPage } from "../features/basket/BasketPage";
import { OrdersPage } from "../features/orders/OrdersPage";
import { AdminPage } from "../features/admin/AdminPage";
import { AccountPage } from "../features/identity/AccountPage";
import { LoginPage } from "../features/identity/LoginPage";
import { RegisterPage } from "../features/identity/RegisterPage";
import { PaymentsPage } from "../features/payments/PaymentsPage";
import { ComparePage } from "../features/compare/ComparePage";
import { setAccessToken } from "./session";

export function App() {
  return (
    <div className="shell">
      <header className="hero">
        <div>
          <p className="eyebrow">Forward Engineered from eShopOnWeb</p>
          <h1>Platform App</h1>
          <p className="lede">
            React 18 frontend aligned to preserved catalog and identity contracts from the
            authoritative M6 architecture artifacts.
          </p>
        </div>
        <nav className="nav">
          <NavLink to="/">Catalog</NavLink>
          <NavLink to="/basket">Basket</NavLink>
          <NavLink to="/orders">Orders</NavLink>
          <NavLink to="/account">Account</NavLink>
          <NavLink to="/admin">Admin</NavLink>
          <NavLink to="/payments">Payments</NavLink>
          <NavLink to="/register">Register</NavLink>
          <NavLink to="/compare">Compare</NavLink>
          <NavLink to="/login">Login</NavLink>
          <button className="nav-ghost" onClick={() => setAccessToken(null)}>
            Logout
          </button>
        </nav>
      </header>

      <main className="main">
        <Routes>
          <Route path="/" element={<CatalogPage />} />
          <Route path="/basket" element={<BasketPage />} />
          <Route path="/orders" element={<OrdersPage />} />
          <Route path="/account" element={<AccountPage />} />
          <Route path="/admin" element={<AdminPage />} />
          <Route path="/payments" element={<PaymentsPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/compare" element={<ComparePage />} />
          <Route path="/login" element={<LoginPage />} />
        </Routes>
      </main>
    </div>
  );
}
