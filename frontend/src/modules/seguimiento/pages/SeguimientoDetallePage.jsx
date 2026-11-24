import { Link, useParams } from "react-router-dom";
import ActivityList from "../../../components/ui/ActivityList";
import Alert from "../../../components/ui/Alert";
import BackLink from "../../../components/ui/BackLink";
import Badge from "../../../components/ui/Badge";
import BaseButton from "../../../components/ui/BaseButton";
import EntityDetail from "../../../components/ui/EntityDetail";
import StatusBadge, { statusLabel } from "../../../components/ui/StatusBadge";
import PageHeader from "../../../components/layout/PageHeader";
import { ROLES } from "../../../components/layout/navigation";
import { resumenSeguimiento } from "../../../constants/seguimiento";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { formatDate } from "../../../utils/format";
import { getUser } from "../../auth/services/authService";
import ReporteCard from "../components/ReporteCard";
import { obtenerSeguimiento } from "../services/seguimientoService";
import "../../../styles/Seguimiento.css";

const actividadItems = (actividad) =>
  actividad.map((a, i) => ({
    id: `${a.tipo}-${a.fecha}-${i}`,
    text:
      a.tipo === "Reporte"
        ? "El adoptante envió un reporte de seguimiento"
        : `Estado actualizado a “${statusLabel(a.estado)}”`,
    detail: a.detalle,
    date: formatDate(a.fecha),
  }));

export default function SeguimientoDetallePage() {
  const { id } = useParams();
  const esAdmin = getUser()?.rol === ROLES.ADMIN;
  const mensaje = useFlashMessage();
  const { data: d, loading, error } = useAsync(() => obtenerSeguimiento(id), [id]);

  const rows = d && [
    { label: "Mascota", value: resumenSeguimiento(d) },
    { label: "Adoptante", value: d.adoptante },
    ...(esAdmin ? [{ label: "Organización", value: d.organizacion }] : []),
    { label: "Fecha de adopción", value: formatDate(d.fechaAdopcion) },
    { label: "Última actualización", value: formatDate(d.ultimaActualizacion) },
    { label: "Estado", value: <StatusBadge status={d.estadoAdaptacion} /> },
    {
      label: "Próxima revisión",
      value: (
        <>
          {formatDate(d.proximaRevision)}
          {d.revisionVencida && <Badge tone="danger" size="sm" className="seg-revision-vencida">Vencida</Badge>}
        </>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Seguimiento de mascotas", to: "/seguimiento" },
          { label: "Detalle" },
        ]}
        title="Detalle de seguimiento"
        description="Historial de adaptación y datos del adoptante."
        actions={<BackLink to="/seguimiento">Volver al listado</BackLink>}
      />

      {mensaje && <Alert variant="success" className="seg-alert">{mensaje}</Alert>}
      {error && <Alert variant="error" className="seg-alert">{error}</Alert>}
      {loading && !d && <p className="seg-state">Cargando…</p>}

      {d?.idDevolucionPendiente && (
        <Alert variant="info" className="seg-alert">
          El adoptante solicitó devolver a {d.mascota}.{" "}
          <Link to={`/seguimiento/devoluciones/${d.idDevolucionPendiente}`}>Revisar solicitud</Link>
        </Alert>
      )}

      {d && (
        <EntityDetail
          rows={rows}
          mainFooter={
            <section>
              <h2 className="seg-reports__title">Reportes del adoptante</h2>
              <p className="seg-reports__hint">Enviados desde la app móvil.</p>
              {d.reportes.length === 0 ? (
                <p className="seg-state">Aún no hay reportes de seguimiento.</p>
              ) : (
                d.reportes.map((r) => <ReporteCard key={r.idReporte} reporte={r} />)
              )}
            </section>
          }
          sideFooter={<ActivityList items={actividadItems(d.actividad)} />}
          actions={
            !esAdmin && (
              <BaseButton to={`/seguimiento/${d.idAdopcion}/actualizar`} fullWidth>
                Actualizar adaptación
              </BaseButton>
            )
          }
        />
      )}
    </>
  );
}
