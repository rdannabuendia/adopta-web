import "../../styles/MediaCard.css";

export default function CardGrid({ children, className = "" }) {
  return <div className={["card-grid", className].filter(Boolean).join(" ")}>{children}</div>;
}