import "../../styles/DetailRows.css";

export default function DetailRows({ items, className = "" }) {
  return (
    <dl className={["detail-rows", className].filter(Boolean).join(" ")}>
      {items.map(({ label, value }) => (
        <div className="detail-rows__row" key={label}>
          <dt>{label}</dt>
          <dd>{value === null || value === undefined || value === "" ? <span className="detail-rows__empty">No especificado</span> : value}</dd>
        </div>
      ))}
    </dl>
  );
}