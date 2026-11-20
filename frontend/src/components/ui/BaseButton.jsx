import { Link } from "react-router-dom";
import "../../styles/BaseButton.css";

/**
 * Props
 *  variant:   "primary" | "secondary"
 *  size:      "xs" | "sm" | "md" | "lg"
 *  to:        ruta interna (react-router) → <Link>
 *  href:      enlace o ancla → <a>
 *  icon:      elemento a la izquierda
 *  iconRight: elemento a la derecha (flecha, enviar…)
 *  loading:   muestra spinner y bloquea el botón
 *  disabled:  deshabilita
 *  fullWidth: ocupa todo el ancho del contenedor
 */

export default function BaseButton({
  variant = "primary",
  size = "md",
  to,
  href,
  icon,
  iconRight,
  loading = false,
  disabled = false,
  fullWidth = false,
  className = "",
  children,
  type = "button",
  ...rest
}) {
  const isDisabled = disabled || loading;

  const classes = [
    "base-btn",
    `base-btn--${variant}`,
    `base-btn--${size}`,
    fullWidth && "base-btn--full",
    loading && "is-loading",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  const content = (
    <>
      {loading ? (
        <span className="base-btn__spinner" aria-hidden="true" />
      ) : (
        icon && <span className="base-btn__icon">{icon}</span>
      )}
      <span className="base-btn__label">{children}</span>
      {iconRight && !loading && (
        <span className="base-btn__icon base-btn__icon--right">{iconRight}</span>
      )}
    </>
  );

  const linkProps = {
    className: classes,
    "aria-disabled": isDisabled || undefined,
    "aria-busy": loading || undefined,
    tabIndex: isDisabled ? -1 : undefined,
    ...rest,
  };

  if (to) return <Link to={to} {...linkProps}>{content}</Link>;
  if (href) return <a href={href} {...linkProps}>{content}</a>;

  return (
    <button
      type={type}
      className={classes}
      disabled={isDisabled}
      aria-busy={loading || undefined}
      {...rest}
    >
      {content}
    </button>
  );
}