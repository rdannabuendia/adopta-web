import { useEffect, useId, useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { ChevronDownIcon, LogoutIcon } from "../ui/Icons";
import { matchesPath } from "./navigation";
import BaseButton from "../ui/BaseButton";

function SidebarGroup({ item }) {
  const { pathname } = useLocation();
  const inside = item.children.some((c) => matchesPath(pathname, c.to));
  const [open, setOpen] = useState(inside);
  const listId = useId();
  const Icon = item.icon;

  // Si navegas (por cualquier vía) a una ruta del grupo, se abre solo
  useEffect(() => {
    if (inside) setOpen(true);
  }, [inside]);

  return (
    <li>
      <button
        type="button"
        className={`side-link${inside ? " is-active-group" : ""}`}
        aria-expanded={open}
        aria-controls={listId}
        onClick={() => setOpen((o) => !o)}
      >
        <span className="side-link__icon">
          <Icon />
        </span>
        <span className="side-link__label">{item.label}</span>
        <ChevronDownIcon
          className={`side-link__chevron${open ? " is-open" : ""}`}
        />
      </button>

      {open && (
        <ul id={listId} className="side-sub">
          {item.children.map((c) => (
            <li key={c.to}>
              <NavLink
                to={c.to}
                end={c.end}
                className={({ isActive }) =>
                  `side-sublink${isActive ? " is-active" : ""}`
                }
              >
                {c.label}
              </NavLink>
            </li>
          ))}
        </ul>
      )}
    </li>
  );
}

export default function Sidebar({ items, open, onLogout }) {
  return (
    <nav
      id="app-sidebar"
      className={`sidebar${open ? " is-open" : ""}`}
      aria-label="Navegación principal"
    >
      <p className="side-label">MENÚ PRINCIPAL</p>

      <ul className="side-list">
        {items.map((item) =>
          item.children ? (
            <SidebarGroup key={item.label} item={item} />
          ) : (
            <li key={item.to}>
              <NavLink
                to={item.to}
                className={({ isActive }) =>
                  `side-link${isActive ? " is-active" : ""}`
                }
              >
                <span className="side-link__icon">
                  <item.icon />
                </span>
                <span className="side-link__label">{item.label}</span>
              </NavLink>
            </li>
          ),
        )}
      </ul>

      <button type="button" className="side-logout" onClick={onLogout}>
        <BaseButton
          variant="secondary"
          size="lg"
          fullWidth
          icon={<LogoutIcon />}
          className="side-logout"
          onClick={onLogout}
        >
          Cerrar Sesión
        </BaseButton>
      </button>
    </nav>
  );
}
