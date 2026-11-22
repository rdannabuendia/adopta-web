import { useEffect, useId, useRef } from "react";
import "../../styles/Modal.css";

export default function Modal({ open, title, onClose, children, footer, className = "" }) {
  const ref = useRef(null);
  const titleId = useId();

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      className={["modal", className].filter(Boolean).join(" ")}
      aria-labelledby={titleId}
      onClose={onClose}
      onClick={(e) => e.target === ref.current && onClose?.()}
    >
      {open && (
        <div className="modal__panel">
          <h2 id={titleId} className="modal__title">{title}</h2>
          <div className="modal__body">{children}</div>
          {footer && <div className="modal__footer">{footer}</div>}
        </div>
      )}
    </dialog>
  );
}