import "../../styles/Timeline.css";

/**
 * Línea de tiempo vertical (historial de un trámite).
 *  items: [{ id, label, date?, detail?, current? }]   date ya formateada
 *  El ítem con current=true (por defecto el último) se pinta relleno.
 */
export default function Timeline({ items = [], emptyText = "Sin movimientos registrados.", className = "" }) {
  if (items.length === 0) return <p className="timeline__empty">{emptyText}</p>;

  const ultimo = items.length - 1;
  return (
    <ol className={["timeline", className].filter(Boolean).join(" ")}>
      {items.map((item, i) => {
        const current = item.current ?? i === ultimo;
        return (
          <li key={item.id} className={["timeline__item", current && "is-current"].filter(Boolean).join(" ")}>
            <span className="timeline__dot" aria-hidden="true" />
            <div className="timeline__content">
              <strong>{item.label}</strong>
              {item.date && <time>{item.date}</time>}
              {item.detail && <p>{item.detail}</p>}
            </div>
          </li>
        );
      })}
    </ol>
  );
}
