import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BackLink from "../../../components/ui/BackLink";
import Badge from "../../../components/ui/Badge";
import BaseButton from "../../../components/ui/BaseButton";
import Card from "../../../components/ui/Card";
import ConfirmDialog from "../../../components/ui/ConfirmDialog";
import DetailRows from "../../../components/ui/DetailRows";
import { CheckCircleIcon } from "../../../components/ui/Icons";
import PhotoGallery from "../../../components/ui/PhotoGallery";
import StatusBadge from "../../../components/ui/StatusBadge";
import PageHeader from "../../../components/layout/PageHeader";
import { ROLES } from "../../../components/layout/navigation";
import { resumenMascota } from "../../../constants/mascota";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { getUser } from "../../auth/services/authService";
import { cambiarEstadoMascota, obtenerMascota } from "../services/mascotasService";
import "../../../styles/Mascotas.css";

function Flag({ ok, yes, no }) {
  return (
    <li className={`pet-flag${ok ? " is-ok" : ""}`}>
      <CheckCircleIcon width="18" height="18" aria-hidden="true" />
      {ok ? yes : no}
    </li>
  );
}

export default function MascotaDetallePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const mensaje = useFlashMessage();
  const esOrg = getUser()?.rol === ROLES.ORG;

  const { data: m, loading, error } = useAsync(() => obtenerMascota(id), [id]);
  const [confirmar, setConfirmar] = useState(false);
  const [busy, setBusy] = useState(false);
  const [actionError, setActionError] = useState("");

  const volver = m && !esOrg ? `/mascotas/organizacion/${m.idOrganizacion}` : "/mascotas";

  const eliminar = async () => {
    setBusy(true);
    setActionError("");
    try {
      await cambiarEstadoMascota(id, { estado: "No disponible", motivo: "Eliminada por la organización" });
      navigate("/mascotas", { state: { mensaje: `${m.nombre} se eliminó de tus mascotas.` } });
    } catch (err) {
      setActionError(err.message);
      setBusy(false);
      setConfirmar(false);
    }
  };

  const filas = m && [
    { label: "Sexo", value: m.sexo === "Desconocido" ? null : m.sexo },
    { label: "Raza", value: m.raza },
    { label: "Color", value: m.color },
    { label: "Peso", value: m.peso != null ? `${m.peso} kg` : null },
    { label: "Estado de salud", value: m.estadoSalud },
    {
      label: "Organización",
      value: esOrg ? null : <Link to={`/mascotas/organizacion/${m.idOrganizacion}`}>{m.organizacion}</Link>,
    },
    { label: "Registrada el", value: formatDate(m.fechaRegistro) },
  ].filter((r) => r.value);

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Mascotas", to: "/mascotas" },
          { label: "Detalle mascota" },
        ]}
        title="Detalle mascota"
        description="Consulta la información general, salud y estado de adopción de la mascota."
      />

      {mensaje && <Alert variant="success" className="mascotas-alert">{mensaje}</Alert>}
      {error && <Alert variant="error" className="mascotas-alert">{error}</Alert>}
      {actionError && <Alert variant="error" className="mascotas-alert">{actionError}</Alert>}
      {loading && !m && <p className="mascotas-state">Cargando…</p>}

      {m && (
        <Card padded className="pet-detail">
          <BackLink to={volver} />

          <div className="pet-detail__grid">
            <PhotoGallery photos={m.fotos.map((f) => mediaUrl(f.url))} alt={`Foto de ${m.nombre}`} />

            <div className="pet-detail__info">
              <h2 className="pet-detail__name">{m.nombre}</h2>
              <p className="pet-detail__summary">{resumenMascota(m)}</p>
              <StatusBadge status={m.estado} />

              <ul className="pet-detail__flags">
                <Flag ok={m.vacunado} yes="Vacunas al día" no="Vacunas pendientes" />
                <Flag ok={m.esterilizado} yes="Esterilizado/a" no="Sin esterilizar" />
              </ul>

              <section>
                <h3>Descripción</h3>
                <p className="pet-detail__text">{m.descripcion}</p>
              </section>

              {m.caracteristicas.length > 0 && (
                <section>
                  <h3>Características</h3>
                  <div className="pet-detail__tags">
                    {m.caracteristicas.map((c) => <Badge key={c} tone="success">{c}</Badge>)}
                  </div>
                </section>
              )}

              {filas.length > 0 && (
                <section>
                  <h3>Datos</h3>
                  <DetailRows items={filas} />
                </section>
              )}
            </div>
          </div>

          {esOrg && (
            <div className="pet-detail__actions">
              <BaseButton to={`/mascotas/${m.idMascota}/editar`}>Editar información</BaseButton>
              {m.estadosDestino.length > 0 ? (
                <BaseButton variant="secondary" to={`/mascotas/${m.idMascota}/estatus`}>Cambiar estatus</BaseButton>
              ) : (
                <span className="pet-detail__hint">Su estatus cambia desde el proceso de adopción.</span>
              )}
              {m.estadosDestino.includes("No disponible") && (
                <BaseButton variant="secondary" onClick={() => setConfirmar(true)}>Eliminar</BaseButton>
              )}
            </div>
          )}
        </Card>
      )}

      <ConfirmDialog
        open={confirmar}
        title="Eliminar mascota"
        confirmLabel="Eliminar"
        busy={busy}
        onConfirm={eliminar}
        onClose={() => setConfirmar(false)}
      >
        <p>
          <strong>{m?.nombre}</strong> dejará de aparecer en tus mascotas y en el catálogo.
          Podrás volver a activarla filtrando por estado “No disponible”.
        </p>
      </ConfirmDialog>
    </>
  );
}