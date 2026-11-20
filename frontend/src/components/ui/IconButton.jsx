import "../../styles/IconButton.css";

/**
 * Props
 *  label:   nombre accesible (obligatorio, no hay texto visible)
 *  variant: "ghost" | "soft"
 *  size:    "sm" | "md" | "lg"
 *  ...rest: props nativas de <button> (onClick, disabled…)
 */
export default function IconButton({
  label,
  variant = "ghost",
  size = "md",
  className = "",
  type = "button",
  children,
  ...rest
}) {
  const classes = ["icon-btn", `icon-btn--${variant}`, `icon-btn--${size}`, className]
    .filter(Boolean)
    .join(" ");

  return (
    <button type={type} className={classes} aria-label={label} {...rest}>
      {children}
    </button>
  );
}