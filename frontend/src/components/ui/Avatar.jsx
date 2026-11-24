import "../../styles/Avatar.css";

/** Foto (src) o, si no hay, la inicial del nombre. size: "sm" | "md" | "lg" */
export default function Avatar({ name = "", src, size = "md", className = "" }) {
  const initial = name.trim()[0]?.toUpperCase() ?? "?";

  return (
    <span className={["avatar", `avatar--${size}`, className].filter(Boolean).join(" ")} aria-hidden="true">
      {src ? <img src={src} alt="" /> : initial}
    </span>
  );
}