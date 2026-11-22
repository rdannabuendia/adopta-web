import { useId, useRef, useState } from "react";
import "../../styles/PhotoPicker.css";

const PERMITIDAS = ["jpg", "jpeg", "png"];
const nuevaKey = () => `n${Math.random().toString(36).slice(2)}`;

/**
 * items: [{ key, src, id? (foto ya guardada), file? (foto nueva) }]
 * principalKey: key de la foto principal
 * onChange({ items, principalKey })
 */
export default function PhotoPicker({
  items, principalKey, onChange, max = 6, maxSizeMB = 5, label = "Fotografías", required = false, error,
}) {
  const inputRef = useRef(null);
  const inputId = useId();
  const [dragging, setDragging] = useState(false);
  const [localError, setLocalError] = useState("");

  const agregar = (lista) => {
    const archivos = Array.from(lista ?? []);
    const libres = max - items.length;
    const nuevos = [];
    let msg = "";

    for (const file of archivos) {
      const ext = file.name.split(".").pop().toLowerCase();
      if (!PERMITIDAS.includes(ext)) { msg = "Solo se permiten imágenes JPG o PNG."; continue; }
      if (file.size > maxSizeMB * 1024 * 1024) { msg = `Cada foto debe pesar máximo ${maxSizeMB} MB.`; continue; }
      if (nuevos.length >= libres) { msg = `Máximo ${max} fotos.`; break; }
      nuevos.push({ key: nuevaKey(), file, src: URL.createObjectURL(file) });
    }

    setLocalError(msg);
    if (nuevos.length) onChange({ items: [...items, ...nuevos], principalKey: principalKey ?? nuevos[0].key });
    if (inputRef.current) inputRef.current.value = "";
  };

  const quitar = (key) => {
    const objetivo = items.find((i) => i.key === key);
    if (objetivo?.file) URL.revokeObjectURL(objetivo.src);
    const resto = items.filter((i) => i.key !== key);
    onChange({ items: resto, principalKey: principalKey === key ? resto[0]?.key ?? null : principalKey });
  };

  const onDrop = (e) => {
    e.preventDefault();
    setDragging(false);
    agregar(e.dataTransfer.files);
  };

  const mensaje = error || localError;

  return (
    <div className="photo-picker">
      <span className="photo-picker__label">
        {label}{required && <span className="photo-picker__req" aria-hidden="true"> *</span>}
      </span>

      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept=".jpg,.jpeg,.png"
        multiple
        hidden
        onChange={(e) => agregar(e.target.files)}
      />

      {items.length === 0 ? (
        <label
          htmlFor={inputId}
          className={`photo-picker__drop${dragging ? " is-dragging" : ""}`}
          onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
          onDragLeave={() => setDragging(false)}
          onDrop={onDrop}
        >
          <svg viewBox="0 0 24 24" width="30" height="30" fill="none" stroke="currentColor" strokeWidth="1.6" aria-hidden="true">
            <path d="M4 8h3l1.5-2h7L17 8h3v11H4z" />
            <circle cx="12" cy="13" r="3.5" />
          </svg>
          <strong>Arrastra y suelta tus fotos aquí</strong>
          <small>o haz clic para seleccionarlas (máximo {max} fotos · JPG o PNG)</small>
        </label>
      ) : (
        <>
          <ul className="photo-picker__grid" onDragOver={(e) => e.preventDefault()} onDrop={onDrop}>
            {items.map((it) => (
              <li key={it.key} className={`photo-picker__item${it.key === principalKey ? " is-principal" : ""}`}>
                <img src={it.src} alt="" />
                {it.key === principalKey ? (
                  <span className="photo-picker__tag">Principal</span>
                ) : (
                  <button
                    type="button"
                    className="photo-picker__make"
                    onClick={() => onChange({ items, principalKey: it.key })}
                  >
                    Hacer principal
                  </button>
                )}
                <button
                  type="button"
                  className="photo-picker__remove"
                  aria-label="Quitar foto"
                  onClick={() => quitar(it.key)}
                >
                  ×
                </button>
              </li>
            ))}

            {items.length < max && (
              <li>
                <label htmlFor={inputId} className="photo-picker__add">
                  <span aria-hidden="true">+</span>
                  Agregar
                </label>
              </li>
            )}
          </ul>
          <small className="photo-picker__count">{items.length} de {max} fotos</small>
        </>
      )}

      {mensaje && <p className="photo-picker__error" role="alert">{mensaje}</p>}
    </div>
  );
}