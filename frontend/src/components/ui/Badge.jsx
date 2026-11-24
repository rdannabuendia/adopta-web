import "../../styles/Badge.css";

export default function Badge({ tone = "teal", size = "md", icon, className = "", children }) {
  const classes = ["badge", `badge--${tone}`, size === "sm" && "badge--sm", className].filter(Boolean).join(" ");
  return (
    <span className={classes}>
      {icon && <span className="badge__icon">{icon}</span>}
      {children}
    </span>
  );
}