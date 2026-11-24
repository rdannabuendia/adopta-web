import "../../styles/EmptyState.css";

/** icon: elemento opcional · action: normalmente un <BaseButton> */
export default function EmptyState({ icon, title, description, action, className = "" }) {
  return (
    <div className={["empty-state", className].filter(Boolean).join(" ")}>
      {icon && <span className="empty-state__icon">{icon}</span>}
      <strong className="empty-state__title">{title}</strong>
      {description && <p className="empty-state__desc">{description}</p>}
      {action && <div className="empty-state__action">{action}</div>}
    </div>
  );
}