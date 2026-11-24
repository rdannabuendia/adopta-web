import "../../styles/InfoItem.css";

export default function InfoItem({ label, children, className = "" }) {
  const vacio = children === null || children === undefined || children === "";
  return (
    <div className={["info-item", className].filter(Boolean).join(" ")}>
      <dt className="info-item__label">{label}</dt>
      <dd className="info-item__value">
        {vacio ? <span className="info-item__empty">No especificado</span> : children}
      </dd>
    </div>
  );
}