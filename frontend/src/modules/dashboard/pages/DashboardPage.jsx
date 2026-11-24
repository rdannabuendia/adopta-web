import { ROLES } from "../../../components/layout/navigation";
import { getUser } from "../../auth/services/authService";
import banner from "../../../assets/img/banner_inicio.jpg";
import "../../../styles/Dashboard.css";

const SUBTITLES = {
  [ROLES.ADMIN]: "Administra organizaciones, mascotas y reportes de Adopta+ desde un solo lugar.",
  [ROLES.ORG]: "Gestiona tus mascotas, solicitudes y seguimientos para que cada una encuentre un hogar.",
};

function formatToday() {
  const text = new Date().toLocaleDateString("es-MX", {
    weekday: "long",
    day: "numeric",
    month: "long",
  });
  return text.charAt(0).toUpperCase() + text.slice(1);
}

export default function DashboardPage() {
  const user = getUser();
  const isOrg = user?.rol === ROLES.ORG;

  // Para organizaciones `nombre` es el nombre del refugio: se muestra completo.
  const displayName = isOrg
    ? user?.nombre?.trim()
    : user?.nombre?.trim().split(/\s+/)[0];

  return (
    <section className="dash-hero" aria-labelledby="dash-title">
      <img className="dash-hero__img" src={banner} alt="" aria-hidden="true" draggable="false" />

      <div className="dash-hero__content">
        
        <span className="dash-hero__date">{formatToday()}</span>

        <h1 id="dash-title" className="dash-hero__title">
          ¡Hola{displayName ? `, ${displayName}` : ""}!
        </h1>

        <p className="dash-hero__subtitle">
          {SUBTITLES[user?.rol] ?? "Bienvenido a Adopta+. Ellos también merecen una oportunidad."}
        </p>

        {user?.rol && <span className="dash-hero__role">{user.rol}</span>}
      </div>
    </section>
  );
}