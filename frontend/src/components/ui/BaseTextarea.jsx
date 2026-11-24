import { forwardRef, useId } from "react";
import "../../styles/BaseInput.css";
import "../../styles/BaseTextarea.css";

/** Props como BaseInput + rows y showCount (requiere maxLength y value) */
const BaseTextarea = forwardRef(function BaseTextarea(
  { label, error, hint, showCount = false, fullWidth = true, rows = 4, className = "", id, ...rest },
  ref
) {
  const autoId = useId();
  const textId = id || autoId;
  const messageId = error || hint ? `${textId}-msg` : undefined;
  const length = typeof rest.value === "string" ? rest.value.length : 0;

  const classes = ["base-input", fullWidth && "base-input--full", error && "has-error", className]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classes}>
      {label && (
        <label htmlFor={textId} className="base-input__label">
          {label}
          {rest.required && <span className="base-input__req" aria-hidden="true">*</span>}
        </label>
      )}

      <div className="base-input__field base-input__field--area">
        <textarea
          ref={ref}
          id={textId}
          rows={rows}
          className="base-input__control base-input__control--area"
          aria-invalid={error ? true : undefined}
          aria-describedby={messageId}
          {...rest}
        />
      </div>

      {(error || hint || showCount) && (
        <div className="base-input__meta">
          {error ? (
            <p id={messageId} className="base-input__message base-input__message--error" role="alert">
              {error}
            </p>
          ) : hint ? (
            <p id={messageId} className="base-input__message">{hint}</p>
          ) : (
            <span />
          )}
          {showCount && rest.maxLength && (
            <span className="base-input__count">{length}/{rest.maxLength}</span>
          )}
        </div>
      )}
    </div>
  );
});

export default BaseTextarea;