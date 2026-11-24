import { useEffect, useRef, useState } from "react";
import Avatar from "./Avatar";
import BaseButton from "./BaseButton";
import { UploadCloudIcon } from "./Icons";
import "../../styles/AvatarUpload.css";

export default function AvatarUpload({
  name,
  currentUrl,
  file = null,
  onChange,
  onRemove,
  maxSizeMB = 2,
  disabled = false,
}) {
  const inputRef = useRef(null);
  const [preview, setPreview] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!file) {
      setPreview(null);
      return;
    }
    const url = URL.createObjectURL(file);
    setPreview(url);
    return () => URL.revokeObjectURL(url);
  }, [file]);

  const handlePick = (e) => {
    const picked = e.target.files?.[0];
    e.target.value = ""; // permite volver a elegir el mismo archivo
    if (!picked) return;

    if (!["image/jpeg", "image/png"].includes(picked.type)) return setError("Usa una imagen JPG o PNG.");
    if (picked.size > maxSizeMB * 1024 * 1024) return setError(`La imagen supera ${maxSizeMB} MB.`);

    setError("");
    onChange?.(picked);
  };

  const src = preview ?? currentUrl;

  return (
    <div className="avatar-upload">
      <Avatar name={name} src={src} size="xl" />

      <input ref={inputRef} type="file" accept="image/jpeg,image/png" hidden tabIndex={-1} onChange={handlePick} />

      <BaseButton
        variant="secondary"
        size="sm"
        icon={<UploadCloudIcon width={16} height={16} />}
        onClick={() => inputRef.current?.click()}
        disabled={disabled}
      >
        Cambiar Fotografía
      </BaseButton>

      {src && onRemove && (
        <BaseButton variant="secondary" size="xs" onClick={onRemove} disabled={disabled}>
          Quitar foto
        </BaseButton>
      )}

      {error && <p className="base-input__message base-input__message--error" role="alert">{error}</p>}
    </div>
  );
}