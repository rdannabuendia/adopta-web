import { useState } from "react";
import "../../styles/TagInput.css";

export default function TagInput({
  label, value = [], onChange, suggestions = [], max = 8, maxLength = 30, hint, error,
}) {
  const [adding, setAdding] = useState(false);
  const [text, setText] = useState("");

  const agregar = (raw) => {
    const t = raw.replace(/,/g, " ").trim();
    if (!t || value.length >= max) return;
    if (value.some((v) => v.toLowerCase() === t.toLowerCase())) return;
    onChange([...value, t]);
    setText("");
  };

  const cerrar = () => {
    agregar(text);
    setText("");
    setAdding(false);
  };

  const onKeyDown = (e) => {
    if (e.key === "Enter") { e.preventDefault(); agregar(text); }
    else if (e.key === "Escape") { setText(""); setAdding(false); }
  };

  const libres = suggestions.filter((s) => !value.some((v) => v.toLowerCase() === s.toLowerCase()));

  return (
    <div className="tag-input">
      {label && <span className="tag-input__label">{label}</span>}

      <div className="tag-input__list">
        {value.map((t) => (
          <span key={t} className="tag-input__tag">
            {t}
            <button type="button" aria-label={`Quitar ${t}`} onClick={() => onChange(value.filter((v) => v !== t))}>×</button>
          </span>
        ))}

        {value.length < max && (adding ? (
          <input
            autoFocus
            className="tag-input__field"
            value={text}
            maxLength={maxLength}
            placeholder="Escribe y presiona Enter"
            onChange={(e) => setText(e.target.value)}
            onKeyDown={onKeyDown}
            onBlur={cerrar}
          />
        ) : (
          <button type="button" className="tag-input__add" onClick={() => setAdding(true)}>
            Agregar <span aria-hidden="true">+</span>
          </button>
        ))}
      </div>

      {libres.length > 0 && value.length < max && (
        <div className="tag-input__suggestions">
          <small>Sugeridas:</small>
          {libres.map((s) => (
            <button key={s} type="button" onClick={() => agregar(s)}>{s}</button>
          ))}
        </div>
      )}

      {error ? <p className="tag-input__msg tag-input__msg--error" role="alert">{error}</p>
        : hint && <p className="tag-input__msg">{hint}</p>}
    </div>
  );
}