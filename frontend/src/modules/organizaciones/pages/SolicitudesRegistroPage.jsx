import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import DataTable from "../../../components/layout/DataTable";
import EntityCell from "../../../components/ui/EntityCell";
import FilterPanel from "../../../components/ui/FilterPanel";
import ListToolbar from "../../../components/ui/ListToolbar";
import Pagination from "../../../components/layout/Pagination";
import StatusBadge from "../../../components/ui/StatusBadge";
import PageHeader from "../../../components/layout/PageHeader";
import { TIPOS_ORGANIZACION } from "../../../constants/organizacion";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { useListFilters } from "../../../hooks/useListFilters";
import { formatDate } from "../../../utils/format";
import { listarSolicitudes } from "../services/organizacionesService";
import "../../../styles/Organizaciones.css";

const PAGE_SIZE = 10;
const EMPTY_FILTERS = { buscar: "", tipo: "", desde: "", hasta: "" };

const COLUMNS = [
  { key: "organizacion", header: "Organización", render: (s) => <EntityCell name={s.nombre} /> },
  { key: "tipoOrganizacion", header: "Tipo" },
  { key: "fechaSolicitud", header: "Fecha de solicitud", render: (s) => formatDate(s.fechaSolicitud) },
  { key: "totalDocumentos", header: "Documentos", align: "center" },
  { key: "estado", header: "Estado", align: "center", render: (s) => <StatusBadge status={s.estado} /> },
  {
    key: "acciones",
    header: "Acciones",
    align: "center",
    render: (s) => (
      <BaseButton to={`/organizaciones/solicitudes/${s.idOrganizacion}`} variant="secondary" size="xs">
        Ver detalle
      </BaseButton>
    ),
  },
];

export default function SolicitudesRegistroPage() {
  const mensaje = useFlashMessage();
  const f = useListFilters(EMPTY_FILTERS);

  const { data, loading, error } = useAsync(
    () => listarSolicitudes({ ...f.filters, pagina: f.page, tamano: PAGE_SIZE }),
    [f.filters, f.page]
  );

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Organizaciones", to: "/organizaciones" },
          { label: "Solicitudes de registro" },
        ]}
        title="Solicitudes de registro"
        description="Revisa los documentos y aprueba o rechaza a las organizaciones nuevas."
      />

      {mensaje && <Alert variant="success" className="org-alert">{mensaje}</Alert>}

      {f.showFilters ? (
        <FilterPanel onApply={f.apply} onClear={f.clear} onClose={f.closeFilters}>
          <BaseInput label="Nombre de organización" name="buscar" placeholder="Escribe para buscar" value={f.draft.buscar} onChange={f.handleDraftChange} />
          <BaseSelect label="Tipo de organización" name="tipo" placeholder="Todos" options={TIPOS_ORGANIZACION} value={f.draft.tipo} onChange={f.handleDraftChange} />
          <BaseInput label="Solicitadas desde" name="desde" type="date" value={f.draft.desde} onChange={f.handleDraftChange} />
          <BaseInput label="Solicitadas hasta" name="hasta" type="date" value={f.draft.hasta} onChange={f.handleDraftChange} />
        </FilterPanel>
      ) : (
        <ListToolbar
          search={f.search}
          onSearchChange={f.setSearch}
          searchPlaceholder="Buscar solicitud..."
          onToggleFilters={f.openFilters}
          activeFilters={f.activeCount}
        />
      )}

      {error && <Alert variant="error" className="org-alert">{error}</Alert>}

      <DataTable
        caption="Solicitudes de registro pendientes"
        columns={COLUMNS}
        rows={data?.items ?? []}
        rowKey="idOrganizacion"
        loading={loading}
        empty={{
          title: "No hay solicitudes pendientes",
          description: f.hasFilters ? "Prueba con otros criterios de búsqueda." : "Cuando una organización se registre aparecerá aquí.",
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