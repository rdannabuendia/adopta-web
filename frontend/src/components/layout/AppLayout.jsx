import { useEffect, useMemo, useState } from "react";
import { Outlet, useLocation, useNavigate } from "react-router-dom";
import { PawIcon } from "../ui/Icons";
import { getUser, logout } from "../../modules/auth/services/authService";
import { getPageTitle, getVisibleNav, ROLES } from "./navigation";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";
import PawDecor from "../ui/PawDecor";
import "../../styles/AppLayout.css";

export default function AppLayout() {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const user = getUser();
  const [menuOpen, setMenuOpen] = useState(false);

  const items = useMemo(() => getVisibleNav(user?.rol), [user?.rol]);

  useEffect(() => setMenuOpen(false), [pathname]);

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  const section = getPageTitle(pathname, items);
  const title =
    user?.rol === ROLES.ORG ? `${section} · ${user.nombre}` : section;

  return (
    <div className="app-shell">
      <Topbar
        user={user}
        title={title}
        menuOpen={menuOpen}
        onToggleMenu={() => setMenuOpen((o) => !o)}
        onLogout={handleLogout}
      />

      <Sidebar items={items} open={menuOpen} onLogout={handleLogout} />
      {menuOpen && (
        <div
          className="app-shell__backdrop"
          onClick={() => setMenuOpen(false)}
        />
      )}

      <main className="app-shell__main">
        <Outlet />
      </main>

      <PawDecor className="app-shell__deco app-shell__deco--l1" />
      <PawDecor className="app-shell__deco app-shell__deco--l2" />
      <PawDecor className="app-shell__deco app-shell__deco--r1" />
      <PawDecor className="app-shell__deco app-shell__deco--r2" />
    </div>
  );
}
