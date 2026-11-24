import { Link } from "react-router-dom";
import "../../styles/Breadcrumbs.css";

/** items: [{ label, to? }]. El último (sin `to`) es la página actual. */
export default function Breadcrumbs({ items = [], className = "" }) {
  if (items.length === 0) return null;

  return (
    <nav className={["breadcrumbs", className].filter(Boolean).join(" ")} aria-label="Ruta de navegación">
      <ol>
        {items.map((b) => (
          <li key={b.label}>
            {b.to ? <Link to={b.to}>{b.label}</Link> : <span aria-current="page">{b.label}</span>}
          </li>
        ))}
      </ol>
    </nav>
  );
}