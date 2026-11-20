import { forwardRef, useId } from "react";

const BaseCheckbox = forwardRef(function BaseCheckbox(
  { children, error, className = "", id, ...rest },
  ref
) {
  const autoId = useId();
  const inputId = id || autoId;

  const classes = ["base-check", error && "has-error", className].filter(Boolean).join(" ");

  return (
    <div className={classes}>
      <label className="base-check__row" htmlFor={inputId}>
        <input
          ref={ref}
          id={inputId}
          type="checkbox"
          className="base-check__input"
          aria-invalid={error ? true : undefined}
          {...rest}
        />
        <span>{children}</span>
      </label>

      {error && (
        <p className="base-input__message base-input__message--error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
});

export default BaseCheckbox;