import { NavLink } from "react-router-dom";
import { REPORTES } from "../../../constants/reportes";
import "../../../styles/Reportes.css";

/** Navegación entre reportes dentro de la página (sirve también en móvil, donde el sidebar está oculto). */
export default function ReportesNav() {
  return (
    <nav className="reportes-nav" aria-label="Reportes">
      {REPORTES.map((r) => (
        <NavLink key={r.to} to={r.to} end={r.end} className={({ isActive }) => `reportes-nav__link${isActive ? " is-active" : ""}`}>
          {r.label}
        </NavLink>
      ))}
      <NavLink to="/reportes/exportar" className={({ isActive }) => `reportes-nav__link reportes-nav__link--export${isActive ? " is-active" : ""}`}>
        Generar y exportar
      </NavLink>
    </nav>
  );
}
