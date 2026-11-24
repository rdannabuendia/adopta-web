import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import ChoiceCards from "../../../components/ui/ChoiceCard";
import ConfirmDialog from "../../../components/ui/ConfirmDialog";
import { CheckCircleIcon, CloseIcon } from "../../../components/ui/Icons";
import ProfileCard from "../../../components/ui/ProfileCard";
import PageHeader from "../../../components/layout/PageHeader";
import { DICTAMENES, etiquetaTamano, MAX_TEXTO, MIN_TEXTO } from "../../../constants/solicitud";
import { useAsync } from "../../../hooks/useAsync";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { obtenerSolicitud, resolverSolicitud } from "../services/solicitudesService";
import "../../../styles/Solicitudes.css";

const LISTA = "/solicitudes";

const ICONOS = {
  Aceptada: <CheckCircleIcon width={22} height={22} />,
  Rechazada: <CloseIcon width={22} height={22} />,
};
const OPCIONES = DICTAMENES.map((o) => ({ ...o, icon: ICONOS[o.value] }));

/** Qué va a pasar según el dictamen elegido (lo que antes no se veía antes de confirmar). */
const CONSECUENCIAS = {
  Aceptada: (mascota) => [
    `${mascota} pasará a estatus “Adoptada” y se registrará la adopción.`,
    "Las demás solicitudes abiertas para esta mascota se cerrarán automáticamente.",
    "El adoptante recibirá una notificación.",
  ],
  Rechazada: (mascota) => [
    "El adoptante recibirá una notificación con el motivo.",
    `${mascota} volverá a estar disponible si nadie más la está solicitando.`,
  ],
};

export default function SolicitudAdopcionResolverPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const detalle = `${LISTA}/${id}`;

  const { data: d, loading, error: loadError } = useAsync(() => obtenerSolicitud(id), [id]);

  const [decision, setDecision] = useState("");
  const [observaciones, setObservaciones] = useState("");
  const [errors, setErrors] = useState({});
  const [confirmando, setConfirmando] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const rechaza = decision === "Rechazada";

  const elegir = (valor) => {
    setDecision(valor);
    setErrors({});
  };

  const validar = () => {
    const er = {};
    if (!decision) er.decision = "Selecciona el dictamen de la solicitud.";
    else if (rechaza && observaciones.trim().length < MIN_TEXTO)
      er.observaciones = `Explica al adoptante el motivo del rechazo (mínimo ${MIN_TEXTO} caracteres).`;
    setErrors(er);
    return Object.keys(er).length === 0;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    setError("");
    if (validar()) setConfirmando(true);
  };

  const confirmar = async () => {
    setSaving(true);
    try {
      const res = await resolverSolicitud(id, { decision, observaciones: observaciones.trim() });
      navigate(LISTA, { state: { mensaje: res.mensaje } });
    } catch (err) {
      setConfirmando(false);
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Adopción", to: LISTA },
          { label: "Detalle Solicitud", to: detalle },
          { label: "Resolución Solicitud" },
        ]}
        title="Resolución de la Solicitud"
        description="Selecciona el dictamen final para esta solicitud y añade las observaciones correspondientes."
      />

      {loadError && <Alert variant="error" className="sol-alert">{loadError}</Alert>}
      {error && <Alert variant="error" className="sol-alert">{error}</Alert>}
      {loading && !d && <p className="sol-state">Cargando…</p>}

      {d?.esFinal && (
        <Alert variant="info" className="sol-alert">
          Esta solicitud ya fue resuelta. <Link to={detalle}>Ver detalle</Link>
        </Alert>
      )}

      {d && !d.esFinal && (
        <div className="sol-resolve">
          <div className="sol-resolve__people">
            <ProfileCard
              title="Mascota"
              name={d.mascota.nombre}
              src={mediaUrl(d.mascota.fotoUrl)}
              lines={[
                { label: "Especie", value: d.mascota.especie },
                { label: "Tamaño", value: etiquetaTamano(d.mascota.tamano) },
                { label: "Edad", value: d.mascota.edadAproximada },
              ]}
            />
            <ProfileCard
              title="Adoptante"
              name={d.adoptante.nombre}
              src={mediaUrl(d.adoptante.fotoUrl)}
              lines={[
                { label: "Teléfono", value: d.adoptante.telefono },
                { label: "Correo", value: d.adoptante.correo },
              ]}
            />
            <Card padded>
              <h3 className="sol-section-title">Solicitud del {formatDate(d.fechaSolicitud)}</h3>
              {d.motivoAdopcion ? (
                <blockquote className="sol-quote">“{d.motivoAdopcion}”</blockquote>
              ) : (
                <p className="sol-state">El adoptante no dejó un mensaje.</p>
              )}
              <Link to={detalle} className="sol-resolve__more">Ver detalle completo y documentos</Link>
            </Card>
          </div>

          <Card padded className="sol-resolve__form">
            <form onSubmit={handleSubmit} noValidate>
              <ChoiceCards
                legend="Dictamen final"
                name="decision"
                options={OPCIONES}
                value={decision}
                onChange={elegir}
                error={errors.decision}
                disabled={saving}
              />

              <BaseTextarea
                label={rechaza ? "Motivo del rechazo" : "Observaciones (opcional)"}
                required={rechaza}
                placeholder={
                  rechaza
                    ? "Explica al adoptante por qué no se acepta la solicitud…"
                    : "Ej. Coordinar la entrega de la mascota esta semana…"
                }
                hint="El adoptante recibirá este mensaje junto con la notificación."
                rows={5}
                maxLength={MAX_TEXTO}
                showCount
                value={observaciones}
                onChange={(e) => setObservaciones(e.target.value)}
                error={errors.observaciones}
                className="sol-resolve__notes"
              />

              {decision && (
                <Alert variant={rechaza ? "error" : "success"} className="sol-resolve__effects">
                  <strong>Al confirmar:</strong>
                  <ul>
                    {CONSECUENCIAS[decision](d.mascota.nombre).map((t) => <li key={t}>{t}</li>)}
                  </ul>
                </Alert>
              )}

              <div className="sol-form-actions">
                <BaseButton type="submit" disabled={!decision}>Confirmar resolución</BaseButton>
                <BaseButton variant="secondary" to={detalle} disabled={saving}>Cancelar</BaseButton>
              </div>
            </form>
          </Card>
        </div>
      )}

      <ConfirmDialog
        open={confirmando}
        title={rechaza ? "¿Rechazar la solicitud?" : "¿Aceptar la solicitud?"}
        confirmLabel={rechaza ? "Sí, rechazar" : "Sí, aceptar"}
        busy={saving}
        onConfirm={confirmar}
        onClose={() => setConfirmando(false)}
      >
        <p>
          {rechaza
            ? `Se rechazará la solicitud de ${d?.adoptante.nombre} para adoptar a ${d?.mascota.nombre}.`
            : `${d?.mascota.nombre} será entregada en adopción a ${d?.adoptante.nombre}.`}{" "}
          Esta acción no se puede deshacer.
        </p>
      </ConfirmDialog>
    </>
  );
}
