import { forwardRef, useId } from "react";
import "../../styles/BaseInput.css";

/**
 * Props
 *  label:     texto de la etiqueta (arriba del campo)
 *  icon:      elemento a la izquierda (correo, candado…)
 *  iconRight: elemento a la derecha (ojo de contraseña, etc.)
 *  error:     mensaje de error (marca el campo en rojo)
 *  hint:      texto de ayuda bajo el campo
 *  fullWidth: ocupa todo el ancho del contenedor (true por defecto)
 *  ...rest:   cualquier prop nativa de <input> (type, name, value, onChange…)
 */
const BaseInput = forwardRef(function BaseInput(
  {
    label,
    icon,
    iconRight,
    error,
    hint,
    fullWidth = true,
    className = "",
    id,
    type = "text",
    ...rest
  },
  ref,
) {
  const autoId = useId();
  const inputId = id || autoId;
  const messageId = error || hint ? `${inputId}-msg` : undefined;

  const classes = [
    "base-input",
    fullWidth && "base-input--full",
    error && "has-error",
    rest.disabled && "is-disabled",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classes}>
      {label && (
        <label htmlFor={inputId} className="base-input__label">
          {label}
          {rest.required && (
            <span className="base-input__req" aria-hidden="true">
              *
            </span>
          )}
        </label>
      )}

      <div className="base-input__field">
        {icon && <span className="base-input__icon">{icon}</span>}

        <input
          ref={ref}
          id={inputId}
          type={type}
          className="base-input__control"
          aria-invalid={error ? true : undefined}
          aria-describedby={messageId}
          {...rest}
        />

        {iconRight && (
          <span className="base-input__icon base-input__icon--right">
            {iconRight}
          </span>
        )}
      </div>

      {error ? (
        <p
          id={messageId}
          className="base-input__message base-input__message--error"
          role="alert"
        >
          {error}
        </p>
      ) : (
        hint && (
          <p id={messageId} className="base-input__message">
            {hint}
          </p>
        )
      )}
    </div>
  );
});

export default BaseInput;
