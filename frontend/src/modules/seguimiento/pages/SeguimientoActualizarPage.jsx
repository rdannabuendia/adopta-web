import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import Avatar from "../../../components/ui/Avatar";
import BackLink from "../../../components/ui/BackLink";
import BaseButton from "../../../components/ui/BaseButton";
import BaseSelect from "../../../components/ui/BaseSelect";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import StatusBadge, { statusLabel } from "../../../components/ui/StatusBadge";
import PageHeader from "../../../components/layout/PageHeader";
import { resumenSeguimiento } from "../../../constants/seguimiento";
import { useAsync } from "../../../hooks/useAsync";
import { mediaUrl } from "../../../utils/media";
import { actualizarAdaptacion, listarEstadosAdaptacion, obtenerSeguimiento } from "../services/seguimientoService";
import "../../../styles/Seguimiento.css";

/** Solo organización: registra cómo va la adaptación (estado + observaciones). */
export default function SeguimientoActualizarPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const volver = `/seguimiento/${id}`;

  const { data: d, error: loadError } = useAsync(() => obtenerSeguimiento(id), [id]);
  const { data: estados } = useAsync(listarEstadosAdaptacion, []);

  const [estado, setEstado] = useState("");
  const [observaciones, setObservaciones] = useState("");
  const [fieldError, setFieldError] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  // Arranca con el estado actual: basta con agregar una observación si no cambia
  useEffect(() => {
    if (d) setEstado(String(d.idEstadoAdaptacion));
  }, [d]);

  const opciones = useMemo(
    () => (estados ?? []).map((e) => ({ value: String(e.idEstadoAdaptacion), label: statusLabel(e.nombre) })),
    [estados]
  );

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!estado) return setFieldError("Selecciona el estado de adaptación.");

    setSaving(true);
    try {
      const res = await actualizarAdaptacion(id, {
        idEstadoAdaptacion: Number(estado),
        observaciones: observaciones.trim(),
      });
      navigate(volver, { state: { mensaje: res.mensaje } });
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Seguimiento de mascotas", to: "/seguimiento" },
          { label: "Detalle", to: volver },
          { label: "Actualizar adaptación" },
        ]}
        title="Actualizar adaptación"
        description="Registra cómo se está adaptando la mascota a su nuevo hogar. Se avisará al adoptante."
      />

      {loadError && <Alert variant="error" className="seg-alert">{loadError}</Alert>}
      {error && <Alert variant="error" className="seg-alert">{error}</Alert>}

      {d && (
        <Card padded className="seg-update">
          <BackLink to={volver} />

          <div className="seg-update__pet">
            <Avatar name={d.mascota} src={mediaUrl(d.fotoUrl)} size="lg" />
            <div>
              <h2>{d.mascota}</h2>
              <p>{resumenSeguimiento(d)} · Adoptada por {d.adoptante}</p>
            </div>
          </div>

          <div className="seg-update__current">
            Estado actual: <StatusBadge status={d.estadoAdaptacion} />
          </div>

          <form className="seg-update__form" onSubmit={handleSubmit} noValidate>
            <BaseSelect
              label="Estado de adaptación"
              name="idEstadoAdaptacion"
              placeholder="Selecciona una opción…"
              options={opciones}
              value={estado}
              onChange={(e) => { setEstado(e.target.value); setFieldError(""); }}
              error={fieldError}
              required
            />

            <BaseTextarea
              label="Observaciones (opcional)"
              placeholder="Ej. Ya come con normalidad y duerme tranquila…"
              rows={5}
              maxLength={1000}
              showCount
              value={observaciones}
              onChange={(e) => setObservaciones(e.target.value)}
            />

            <div className="seg-update__actions">
              <BaseButton type="submit" loading={saving}>Guardar actualización</BaseButton>
              <BaseButton variant="secondary" to={volver} disabled={saving}>Cancelar</BaseButton>
            </div>
          </form>
        </Card>
      )}
    </>
  );
}
