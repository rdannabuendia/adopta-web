import { useMemo } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import DataTable from "../../../components/layout/DataTable";
import InlineFilters from "../../../components/ui/InlineFilters";
import Pagination from "../../../components/layout/Pagination";
import PageHeader from "../../../components/layout/PageHeader";
import StatusBadge from "../../../components/ui/StatusBadge";
import { ROLES } from "../../../components/layout/navigation";
import { ESTADOS_DEVOLUCION, tituloMascota } from "../../../constants/seguimiento";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { useInstantFilters } from "../../../hooks/useInstantFilters";
import { formatDate } from "../../../utils/format";
import { getUser } from "../../auth/services/authService";
import { listarDevoluciones } from "../services/seguimientoService";
import "../../../styles/Seguimiento.css";

const PAGE_SIZE = 10;
const EMPTY = { estado: "" };

function buildColumns(esAdmin) {
  const columns = [
    { key: "adoptante", header: "Adoptante", render: (s) => <strong>{s.adoptante}</strong> },
    { key: "mascota", header: "Mascota", render: (s) => tituloMascota(s.mascota, s.especie) },
    { key: "fechaSolicitud", header: "Fecha", render: (s) => formatDate(s.fechaSolicitud) },
    { key: "estado", header: "Estado", align: "center", render: (s) => <StatusBadge status={s.estado} /> },
    {
      key: "acciones",
      header: "Acciones",
      align: "center",
      render: (s) => (
        <BaseButton to={`/seguimiento/devoluciones/${s.idDevolucion}`} variant="secondary" size="xs">
          Ver detalle
        </BaseButton>
      ),
    },
  ];
  if (esAdmin) columns.splice(2, 0, { key: "organizacion", header: "Organización" });
  return columns;
}

export default function DevolucionesListaPage() {
  const esAdmin = getUser()?.rol === ROLES.ADMIN;
  const mensaje = useFlashMessage();
  const f = useInstantFilters(EMPTY);

  const { data, loading, error } = useAsync(
    () => listarDevoluciones({ ...f.params, pagina: f.page, tamano: PAGE_SIZE }),
    [f.params, f.page]
  );
  const columns = useMemo(() => buildColumns(esAdmin), [esAdmin]);

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Seguimiento de mascotas", to: "/seguimiento" },
          { label: "Solicitudes de devolución" },
        ]}
        title="Solicitudes de devolución"
        description="Revisa las solicitudes de personas que desean devolver una mascota adoptada."
      />

      {mensaje && <Alert variant="success" className="seg-alert">{mensaje}</Alert>}

      <InlineFilters
        search={f.search}
        onSearchChange={f.setSearch}
        searchPlaceholder="Buscar solicitud..."
        filters={[{ name: "estado", label: "Todas las solicitudes", options: ESTADOS_DEVOLUCION, value: f.filters.estado }]}
        onFilterChange={f.setFilter}
        onClear={f.clear}
        canClear={f.hasFilters}
      />

      {error && <Alert variant="error" className="seg-alert">{error}</Alert>}

      <DataTable
        caption="Solicitudes de devolución"
        columns={columns}
        rows={data?.items ?? []}
        rowKey="idDevolucion"
        loading={loading}
        empty={{
          title: "No hay solicitudes de devolución",
          description: f.hasFilters ? "Prueba con otros criterios de búsqueda." : "Cuando un adoptante pida devolver una mascota aparecerá aquí.",
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
