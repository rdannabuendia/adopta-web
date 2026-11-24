import { Link } from "react-router-dom";
import "../../styles/LinkCard.css";

/** Tarjeta clicable: icono + título + descripción. `icon` es un componente (no un elemento). */
export default function LinkCard({ to, icon: Icon, title, description, className = "" }) {
  return (
    <Link to={to} className={["link-card", className].filter(Boolean).join(" ")}>
      {Icon && (
        <span className="link-card__icon">
          <Icon />
        </span>
      )}
      <strong className="link-card__title">{title}</strong>
      {description && <span className="link-card__desc">{description}</span>}
    </Link>
  );
}