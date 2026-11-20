import { forwardRef, useId } from "react";

/**
 * Props
 *  label, error, hint, fullWidth: igual que BaseInput
 *  options:     ["Refugio", ...] o [{ value, label }]
 *  placeholder: texto de la opción vacía
 *  ...rest:     props nativas de <select> (name, value, onChange, required…)
 */
const BaseSelect = forwardRef(function BaseSelect(
  {
    label,
    options = [],
    placeholder = "Selecciona una opción",
    error,
    hint,
    fullWidth = true,
    className = "",
    id,
    ...rest
  },
  ref
) {
  const autoId = useId();
  const selectId = id || autoId;
  const messageId = error || hint ? `${selectId}-msg` : undefined;

  const classes = [
    "base-input",
    fullWidth && "base-input--full",
    error && "has-error",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classes}>
      {label && (
        <label htmlFor={selectId} className="base-input__label">
          {label}
          {rest.required && (
            <span className="base-input__req" aria-hidden="true">*</span>
          )}
        </label>
      )}

      <div className="base-input__field">
        <select
          ref={ref}
          id={selectId}
          className="base-input__control"
          aria-invalid={error ? true : undefined}
          aria-describedby={messageId}
          {...rest}
        >
          <option value="">{placeholder}</option>
          {options.map((o) => {
            const opt = typeof o === "string" ? { value: o, label: o } : o;
            return (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            );
          })}
        </select>
      </div>

      {error ? (
        <p id={messageId} className="base-input__message base-input__message--error" role="alert">
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

export default BaseSelect;