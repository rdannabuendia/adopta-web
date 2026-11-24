import Avatar from "./Avatar";
import Card from "./Card";
import "../../styles/ProfileCard.css";

/**
 * Tarjeta con avatar + datos clave (persona adoptante, mascota adoptada…).
 *  title:  encabezado pequeño de la tarjeta
 *  name:   nombre destacado
 *  src:    foto (si no hay, se muestra la inicial)
 *  lines:  [{ label, value }]  -> "Correo: maria@email.com"
 */
export default function ProfileCard({ title, name, src, lines = [], className = "" }) {
  return (
    <Card padded className={["profile-card", className].filter(Boolean).join(" ")}>
      {title && <h3 className="profile-card__title">{title}</h3>}
      <div className="profile-card__body">
        <Avatar name={name} src={src} size="xl" />
        <div className="profile-card__info">
          <strong>{name}</strong>
          {lines.map((l) => (
            <span key={l.label}>
              {l.label}: {l.value || "No especificado"}
            </span>
          ))}
        </div>
      </div>
    </Card>
  );
}
