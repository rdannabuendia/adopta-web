import { useState } from "react";
import BaseInput from "./BaseInput";
import { EyeIcon, EyeOffIcon, LockIcon } from "./Icons";

/**
 * Input de contraseña con botón mostrar/ocultar.
 * Acepta las mismas props que BaseInput (menos type / icon / iconRight).
 */
export default function PasswordInput(props) {
  const [visible, setVisible] = useState(false);

  return (
    <BaseInput
      {...props}
      type={visible ? "text" : "password"}
      icon={<LockIcon />}
      iconRight={
        <button
          type="button"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? "Ocultar contraseña" : "Mostrar contraseña"}
          aria-pressed={visible}
        >
          {visible ? <EyeIcon /> : <EyeOffIcon />}
        </button>
      }
    />
  );
}
