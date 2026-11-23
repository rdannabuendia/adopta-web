import { useId } from "react";
import "../../styles/ChoiceCards.css";

/**
 * Selector de una opción entre pocas, con tarjetas grandes (radio accesible).
 *  options: [{ value, label, description?, tone?: "success" | "danger" | "teal", icon? }]
 *  value / onChange(value)
 */
export default function ChoiceCards({ legend, options = [], value, onChange, name, error, disabled = false, className = "" }) {
  const autoId = useId();
  const group = name ?? autoId;

  return (
    <fieldset className={["choice-cards", error && "has-error", className].filter(Boolean).join(" ")} disabled={disabled}>
      {legend && <legend className="choice-cards__legend">{legend}</legend>}

      <div className="choice-cards__grid">
        {options.map((o) => (
          <label key={o.value} className={`choice-card choice-card--${o.tone ?? "teal"}${value === o.value ? " is-selected" : ""}`}>
            <input
              type="radio"
              name={group}
              value={o.value}
              checked={value === o.value}
              onChange={() => onChange?.(o.value)}
            />
            {o.icon && <span className="choice-card__icon" aria-hidden="true">{o.icon}</span>}
            <span className="choice-card__text">
              <strong>{o.label}</strong>
              {o.description && <small>{o.description}</small>}
            </span>
          </label>
        ))}
      </div>

      {error && <p className="choice-cards__error" role="alert">{error}</p>}
    </fieldset>
  );
}
