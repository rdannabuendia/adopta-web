import { useEffect, useId, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { infoNotificacion } from "../../constants/notificaciones";
import { useNotificaciones } from "../../hooks/useNotificaciones";
import { tiempoRelativo } from "../../utils/format";
import { BellIcon, CheckAllIcon } from "../ui/Icons";
import "../../styles/NotificationBell.css";

/** Campanita del topbar: indicador de no leídas y panel con las últimas notificaciones. */
export default function NotificationBell() {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);
  const panelId = useId();
  const navigate = useNavigate();
  const { items, noLeidas, loading, error, leer, leerTodas, recargar } = useNotificaciones();

  useEffect(() => {
    if (!open) return;
    const onDown = (e) => !ref.current?.contains(e.target) && setOpen(false);
    const onKey = (e) => e.key === "Escape" && setOpen(false);
    document.addEventListener("mousedown", onDown);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDown);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const toggle = () => {
    if (!open) recargar();
    setOpen((o) => !o);
  };

  const abrir = (n) => {
    leer(n);
    const destino = infoNotificacion(n).destino(n);
    if (destino) {
      setOpen(false);
      navigate(destino);
    }
  };

  const etiqueta = noLeidas > 0 ? `Notificaciones, ${noLeidas} sin leer` : "Notificaciones";

  return (
    <div className="bell" ref={ref}>
      <button
        type="button"
        className={`bell__btn${open ? " is-open" : ""}`}
        aria-label={etiqueta}
        aria-haspopup="true"
        aria-expanded={open}
        aria-controls={open ? panelId : undefined}
        onClick={toggle}
      >
        <BellIcon />
        {noLeidas > 0 && (
          <span className="bell__badge" aria-hidden="true">
            {noLeidas > 9 ? "9+" : noLeidas}
          </span>
        )}
      </button>

      {open && (
        <div id={panelId} className="bell__panel" role="region" aria-label="Notificaciones">
          <div className="bell__head">
            <strong>Notificaciones</strong>
            <button type="button" className="bell__all" onClick={leerTodas} disabled={noLeidas === 0}>
              <CheckAllIcon /> Marcar todas como leídas
            </button>
          </div>

          <div className="bell__list">
            {loading && <p className="bell__empty">Cargando…</p>}
            {!loading && error && items.length === 0 && (
              <p className="bell__empty">No se pudieron cargar las notificaciones.</p>
            )}
            {!loading && !error && items.length === 0 && (
              <p className="bell__empty">Estás al día. No tienes notificaciones.</p>
            )}

            {items.map((n) => (
              <button
                key={n.idNotificacion}
                type="button"
                className={`bell__item bell__item--${infoNotificacion(n).tono}${n.leida ? "" : " is-unread"}`}
                onClick={() => abrir(n)}
              >
                <span className="bell__dot" aria-hidden="true" />
                <span className="bell__body">
                  <span className="bell__title">{n.titulo}</span>
                  <span className="bell__msg">{n.mensaje}</span>
                  <span className="bell__time">{tiempoRelativo(n.fecha)}</span>
                </span>
                {!n.leida && <span className="sr-only">Sin leer</span>}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
