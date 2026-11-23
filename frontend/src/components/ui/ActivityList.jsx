import Card from "./Card";
import "../../styles/ActivityList.css";

/**
 * Lista cronológica corta ("Actividad reciente").
 *  items: [{ id, text, detail?, date? }]   date ya formateada
 */
export default function ActivityList({ title = "Actividad reciente", items = [], emptyText = "Sin actividad registrada.", className = "" }) {
  return (
    <Card padded className={["activity-list", className].filter(Boolean).join(" ")}>
      <h2 className="activity-list__title">{title}</h2>
      {items.length === 0 ? (
        <p className="activity-list__empty">{emptyText}</p>
      ) : (
        <ul className="activity-list__items">
          {items.map((i) => (
            <li key={i.id}>
              <span>{i.text}</span>
              {i.detail && <em className="activity-list__detail">{i.detail}</em>}
              {i.date && <time className="activity-list__date">{i.date}</time>}
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}
