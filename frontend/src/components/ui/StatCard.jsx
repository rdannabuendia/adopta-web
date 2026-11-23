import { Link } from "react-router-dom";
import "../../styles/StatCard.css";

/**
 * Indicador: etiqueta, número grande y una línea de apoyo.
 * Con `to` toda la tarjeta es un enlace.
 */
export default function StatCard({ label, value, hint, to, className = "" }) {
  const classes = ["stat-card", to && "stat-card--link", className].filter(Boolean).join(" ");
  const body = (
    <>
      <span className="stat-card__label">{label}</span>
      <strong className="stat-card__value">{typeof value === "number" ? value.toLocaleString("es-MX") : value}</strong>
      {hint && <span className="stat-card__hint">{hint}</span>}
    </>
  );
  return to ? <Link to={to} className={classes}>{body}</Link> : <div className={classes}>{body}</div>;
}

/** Fila responsive de StatCard. */
export function StatGrid({ children, columns = 4 }) {
  return <div className={`stat-grid stat-grid--${columns}`}>{children}</div>;
}
