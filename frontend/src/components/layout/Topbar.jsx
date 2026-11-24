import { Link } from "react-router-dom";
import Avatar from "../ui/Avatar";
import DropdownMenu from "../ui/DropdownMenu";
import IconButton from "../ui/IconButton";
import NotificationBell from "./NotificationBell";
import { mediaUrl } from "../../utils/media";
import { ChevronDownIcon, LogoutIcon, MenuIcon, UserIcon } from "../ui/Icons";
import logo from "../../assets/img/adopta_logo.svg";

export default function Topbar({ user, title, menuOpen, onToggleMenu, onLogout }) {
  const firstName = user?.nombre?.trim().split(/\s+/)[0] || "Usuario";

  return (
    <header className="topbar">
      <IconButton
        label={menuOpen ? "Cerrar menú" : "Abrir menú"}
        className="topbar__menu-btn"
        onClick={onToggleMenu}
        aria-expanded={menuOpen}
        aria-controls="app-sidebar"
      >
        <MenuIcon />
      </IconButton>

      <Link to="/inicio" className="topbar__brand" aria-label="Ir al inicio">
        <img className="topbar__logo" src={logo} alt="Adopta+" />
      </Link>

      <p className="topbar__title">{title}</p>

      <NotificationBell />

      <DropdownMenu
        className="topbar__user"
        triggerClassName="user-trigger"
        trigger={({ open }) => (
          <>
            <Avatar name={firstName} src={mediaUrl(user?.fotoUrl)} />
            <span className="user-trigger__name">{firstName}</span>
            <ChevronDownIcon className={`user-trigger__chevron${open ? " is-open" : ""}`} />
          </>
        )}
        header={
          <div className="user-info">
            <strong>{user?.nombre}</strong>
            <span>{user?.email}</span>
            <small>{user?.rol}</small>
          </div>
        }
        items={[
          { label: "Ver perfil", icon: <UserIcon />, to: "/perfil" },
          { label: "Cerrar sesión", icon: <LogoutIcon />, onClick: onLogout },
        ]}
      />
    </header>
  );
}