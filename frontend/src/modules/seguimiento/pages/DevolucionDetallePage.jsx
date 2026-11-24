import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import Badge from "../../../components/ui/Badge";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import ConfirmDialog from "../../../components/ui/ConfirmDialog";
import ProfileCard from "../../../components/ui/ProfileCard";
import PageHeader from "../../../components/layout/PageHeader";
import { ROLES } from "../../../components/layout/navigation";
import { textoEstadoDevolucion } from "../../../constants/seguimiento";
import { useAsync } from "../../../hooks/useAsync";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { getUser } from "../../auth/services/authService";
import { aceptarDevolucion, obtenerDevolucion } from "../services/seguimientoService";
import "../../../styles/Seguimiento.css";

const LISTA = "/seguimiento/devoluciones";

export default function DevolucionDetallePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const esAdmin = getUser()?.rol === ROLES.ADMIN;

  const { data: d, loading, error: loadError } = useAsync(() => obtenerDevolucion(id), [id]);

  const [confirmando, setConfirmando] = useState(false);
  const [observaciones, setObservaciones] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const pendiente = d?.estado === "Pendiente";

  const handleAceptar = async () => {
    setError("");
    setSaving(true);
    try {
      const res = await aceptarDevolucion(id, { observaciones: observaciones.trim() });
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
          { label: "Seguimiento de mascotas", to: "/seguimiento" },
          { label: "Solicitudes de devolución", to: LISTA },
          { label: "Detalle" },
        ]}
        title="Detalle de solicitud de devolución"
        description={
          esAdmin
            ? "Datos de la persona adoptante, la mascota y el motivo de la solicitud."
            : "Revisa los datos de la persona adoptante, la mascota y el motivo antes de resolver."
        }
      />

      {loadError && <Alert variant="error" className="seg-alert">{loadError}</Alert>}
      {error && <Alert variant="error" className="seg-alert">{error}</Alert>}
      {loading && !d && <p className="seg-state">Cargando…</p>}

      {d && (
        <Card padded>
          <h2 className="dev-detail__title">
            Solicitud {d.codigo} · {textoEstadoDevolucion(d.estado)}
          </h2>

          <div className="dev-detail__people">
            <ProfileCard
              title="Persona adoptante"
              name={d.adoptante.nombre}
              src={mediaUrl(d.adoptante.fotoUrl)}
              lines={[
                { label: "Correo", value: d.adoptante.correo },
                { label: "Teléfono", value: d.adoptante.telefono },
                { label: "Dirección", value: d.adoptante.direccion },
              ]}
            />
            <ProfileCard
              title="Mascota adoptada"
              name={d.mascota.nombre}
              src={mediaUrl(d.mascota.fotoUrl)}
              lines={[
                { label: "Especie", value: d.mascota.especie },
                { label: "Raza", value: d.mascota.raza },
                { label: "Edad", value: d.mascota.edadAproximada },
              ]}
            />
          </div>

          <div className="dev-detail__fields">
            <div>
              {d.motivos.length > 0 && (
                <div className="dev-detail__motivos">
                  {d.motivos.map((m) => <Badge key={m} tone="teal" size="sm">{m}</Badge>)}
                </div>
              )}
              <BaseTextarea label="Motivo de devolución" rows={3} value={d.explicacion} readOnly />
            </div>
            <div>
              <BaseInput label="Fecha de solicitud" value={formatDate(d.fechaSolicitud)} readOnly />
              {esAdmin && <BaseInput label="Organización" value={d.organizacion} readOnly className="dev-detail__org" />}
            </div>
          </div>

          {!pendiente && (
            <Alert variant="success" className="dev-detail__resolution">
              Solicitud aceptada el {formatDate(d.fechaRevision)}.
              {d.observacionesOrganizacion && ` Observaciones: ${d.observacionesOrganizacion}`}
            </Alert>
          )}

          <div className="dev-detail__actions">
            {pendiente && !esAdmin && (
              <BaseButton onClick={() => setConfirmando(true)}>Aceptar solicitud</BaseButton>
            )}
            <BaseButton variant="secondary" to={LISTA}>Volver a solicitudes</BaseButton>
          </div>
        </Card>
      )}

      <ConfirmDialog
        open={confirmando}
        title="Aceptar solicitud de devolución"
        confirmLabel="Aceptar solicitud"
        busy={saving}
        onConfirm={handleAceptar}
        onClose={() => setConfirmando(false)}
      >
        <p className="dev-confirm__text">
          La mascota volverá a estar disponible para adopción y se avisará a la persona adoptante. Esta acción no se puede deshacer.
        </p>
        <BaseTextarea
          label="Observaciones (opcional)"
          rows={3}
          maxLength={500}
          showCount
          value={observaciones}
          onChange={(e) => setObservaciones(e.target.value)}
        />
      </ConfirmDialog>
    </>
  );
}
