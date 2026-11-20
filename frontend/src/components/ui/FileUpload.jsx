import { useId, useRef, useState } from "react";
import { CloseIcon, FileTextIcon, UploadCloudIcon } from "./Icons";
import "../../styles/FileUpload.css";

function formatSize(bytes) {
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * Props
 *  label:     texto sobre la zona
 *  required:  muestra asterisco
 *  value:     File | null
 *  onChange:  (file | null) => void   (solo se llama con archivos válidos)
 *  error:     mensaje externo (ej. "obligatorio")
 *  accept:    extensiones permitidas
 *  maxSizeMB: tamaño máximo
 *  hint:      texto pequeño inferior
 */
export default function FileUpload({
  label,
  required = false,
  value = null,
  onChange,
  error,
  accept = ".pdf,.jpg,.jpeg,.png",
  maxSizeMB = 5,
  hint = "PDF, JPG o PNG (máx. 5 MB)",
  disabled = false,
  className = "",
}) {
  const inputRef = useRef(null);
  const id = useId();
  const [dragging, setDragging] = useState(false);
  const [localError, setLocalError] = useState("");

  const allowed = accept.split(",").map((a) => a.trim().replace(".", "").toLowerCase());

  const validate = (file) => {
    const ext = file.name.split(".").pop().toLowerCase();
    if (!allowed.includes(ext)) return "Formato no permitido. Usa PDF, JPG o PNG.";
    if (file.size === 0) return "El archivo está vacío.";
    if (file.size > maxSizeMB * 1024 * 1024) return `El archivo supera ${maxSizeMB} MB.`;
    return "";
  };

  const pick = (file) => {
    if (!file) return;
    const msg = validate(file);
    setLocalError(msg);
    if (!msg) onChange?.(file);
  };

  const open = () => !disabled && inputRef.current?.click();

  const remove = () => {
    if (inputRef.current) inputRef.current.value = "";
    setLocalError("");
    onChange?.(null);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setDragging(false);
    if (!disabled) pick(e.dataTransfer.files?.[0]);
  };

  const message = error || localError;
  const classes = [
    "file-upload",
    message && "has-error",
    dragging && "is-dragging",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={classes}>
      {label && (
        <span className="file-upload__label" id={`${id}-label`}>
          {label}
          {required && <span className="base-input__req" aria-hidden="true">*</span>}
        </span>
      )}

      <div
        className="file-upload__zone"
        onDragOver={(e) => {
          e.preventDefault();
          if (!disabled) setDragging(true);
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={handleDrop}
      >
        {value ? (
          <div className="file-upload__file">
            <FileTextIcon />
            <span className="file-upload__name" title={value.name}>
              {value.name}
            </span>
            <span className="file-upload__size">{formatSize(value.size)}</span>
            <button
              type="button"
              className="file-upload__remove"
              onClick={remove}
              disabled={disabled}
              aria-label={`Quitar ${value.name}`}
            >
              <CloseIcon />
            </button>
          </div>
        ) : (
          <button
            type="button"
            className="file-upload__empty"
            onClick={open}
            disabled={disabled}
            aria-labelledby={`${id}-label`}
          >
            <span className="file-upload__cloud">
              <UploadCloudIcon />
            </span>
            <strong>Haz clic para subir</strong>
            <span>o arrastra el archivo</span>
            <small>{hint}</small>
          </button>
        )}

        <input
          ref={inputRef}
          type="file"
          accept={accept}
          hidden
          tabIndex={-1}
          onChange={(e) => {
            pick(e.target.files?.[0]);
            e.target.value = ""; // permite volver a elegir el mismo archivo
          }}
        />
      </div>

      {message && (
        <p className="base-input__message base-input__message--error" role="alert">
          {message}
        </p>
      )}
    </div>
  );
}