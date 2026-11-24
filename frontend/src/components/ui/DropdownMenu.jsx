import { useEffect, useId, useRef, useState } from "react";
import { Link } from "react-router-dom";
import "../../styles/DropdownMenu.css";

/**
 * Props
 *  trigger:          ({ open }) => contenido del botón que abre el menú
 *  triggerClassName: clases para estilizar el botón
 *  header:           nodo opcional sobre las opciones (info del usuario…)
 *  items:            [{ label, icon, to, onClick, danger }]  (con `to` es link, sin `to` es botón)
 *  align:            "right" | "left"
 */
export default function DropdownMenu({
  trigger,
  triggerClassName = "",
  header,
  items = [],
  align = "right",
  className = "",
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);
  const menuId = useId();

  useEffect(() => {
    if (!open) return;
    const onDown = (e) => {
      if (!ref.current?.contains(e.target)) setOpen(false);
    };
    const onKey = (e) => e.key === "Escape" && setOpen(false);
    document.addEventListener("mousedown", onDown);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDown);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  return (
    <div className={["dropdown", className].filter(Boolean).join(" ")} ref={ref}>
      <button
        type="button"
        className={["dropdown__trigger", triggerClassName].filter(Boolean).join(" ")}
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={open ? menuId : undefined}
        onClick={() => setOpen((o) => !o)}
      >
        {trigger({ open })}
      </button>

      {open && (
        <div id={menuId} role="menu" className={`dropdown__menu dropdown__menu--${align}`}>
          {header && <div className="dropdown__header">{header}</div>}

          {items.map(({ label, icon, to, onClick, danger }) => {
            const cls = `dropdown__item${danger ? " is-danger" : ""}`;
            const content = (
              <>
                {icon && <span className="dropdown__icon">{icon}</span>}
                {label}
              </>
            );
            const handleClick = () => {
              setOpen(false);
              onClick?.();
            };

            return to ? (
              <Link key={label} to={to} role="menuitem" className={cls} onClick={handleClick}>
                {content}
              </Link>
            ) : (
              <button key={label} type="button" role="menuitem" className={cls} onClick={handleClick}>
                {content}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}