import { useMemo } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import DataTable from "../../../components/layout/DataTable";
import EntityCell from "../../../components/ui/EntityCell";
import InlineFilters from "../../../components/ui/InlineFilters";
import Pagination from "../../../components/layout/Pagination";
import PageHeader from "../../../components/layout/PageHeader";
import StatusBadge, { statusLabel } from "../../../components/ui/StatusBadge";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { useInstantFilters } from "../../../hooks/useInstantFilters";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { listarEstadosSolicitud, listarSolicitudes } from "../services/solicitudesService";
import "../../../styles/Solicitudes.css";

const PAGE_SIZE = 10;
const EMPTY = { idEstadoSolicitud: "", fecha: "" };

const COLUMNS = [
  {
    key: "adoptante",
    header: "Solicitante",
    render: (s) => <EntityCell name={s.adoptante} imageUrl={mediaUrl(s.adoptanteFotoUrl)} />,
  },
  { key: "mascota", header: "Mascota" },
  { key: "fechaSolicitud", header: "Fecha", render: (s) => formatDate(s.fechaSolicitud) },
  { key: "estado", header: "Estatus", align: "center", render: (s) => <StatusBadge status={s.estado} /> },
  {
    key: "acciones",
    header: "Acciones",
    align: "center",
    render: (s) => (
      <BaseButton to={`/solicitudes/${s.idSolicitud}`} variant="secondary" size="xs">
        Detalle
      </BaseButton>
    ),
  },
];

export default function SolicitudesAdopcionListaPage() {
  const mensaje = useFlashMessage();
  const f = useInstantFilters(EMPTY);

  const { data, loading, error } = useAsync(
    () => listarSolicitudes({ ...f.params, pagina: f.page, tamano: PAGE_SIZE }),
    [f.params, f.page]
  );
  const { data: estados } = useAsync(listarEstadosSolicitud, []);

  const opcionesEstado = useMemo(
    () => (estados ?? []).map((e) => ({ value: String(e.idEstadoSolicitud), label: statusLabel(e.nombre) })),
    [estados]
  );

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio", to: "/inicio" }, { label: "Adopción" }]}
        title="Solicitudes de Adopción"
        description="Consulta, filtra y gestiona los trámites de adopción en curso."
      />

      {mensaje && <Alert variant="success" className="sol-alert">{mensaje}</Alert>}
      {error && <Alert variant="error" className="sol-alert">{error}</Alert>}

      <InlineFilters
        search={f.search}
        onSearchChange={f.setSearch}
        searchPlaceholder="Buscar solicitud..."
        filters={[
          { name: "idEstadoSolicitud", label: "Estatus", options: opcionesEstado, value: f.filters.idEstadoSolicitud },
          { name: "fecha", label: "Seleccione una fecha", type: "date", value: f.filters.fecha },
        ]}
        onFilterChange={f.setFilter}
        onClear={f.clear}
        canClear={f.hasFilters}
      />

      <DataTable
        caption="Solicitudes de adopción de la organización"
        columns={COLUMNS}
        rows={data?.items ?? []}
        rowKey="idSolicitud"
        loading={loading}
        empty={{
          title: "No se encontraron solicitudes",
          description: f.hasFilters
            ? "Prueba con otros criterios de búsqueda."
            : "Cuando alguien solicite adoptar a una de tus mascotas aparecerá aquí.",
          action: f.hasFilters && <BaseButton variant="secondary" onClick={f.clear}>Limpiar filtros</BaseButton>,
        }}
        footer={
          data && (
            <Pagination page={data.pagina} totalPages={data.totalPaginas} total={data.total} pageSize={PAGE_SIZE} onChange={f.setPage} />
          )
        }
      />
    </>
  );
}
