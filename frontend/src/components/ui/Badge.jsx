import "../../styles/Badge.css";

export default function Badge({ tone = "teal", icon, className = "", children }) {
  return (
    <span className={["badge", `badge--${tone}`, className].filter(Boolean).join(" ")}>
      {icon && <span className="badge__icon">{icon}</span>}
      {children}
    </span>
  );
}