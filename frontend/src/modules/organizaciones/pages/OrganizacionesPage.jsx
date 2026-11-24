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
import { mediaUrl } from "../../../utils/media";
import { listarOrganizaciones, obtenerCiudades } from "../services/organizacionesService";
import "../../../styles/Organizaciones.css";

const PAGE_SIZE = 10;
const EMPTY_FILTERS = { buscar: "", estado: "", tipo: "", ciudad: "" };
const ESTADOS = ["Activa", "Inactiva"];

const COLUMNS = [
  { key: "organizacion", header: "Organización", render: (o) => <EntityCell name={o.nombre} imageUrl={mediaUrl(o.fotoUrl)} /> },
  { key: "ubicacion", header: "Ubicación" },
  { key: "totalMascotas", header: "Mascotas" },
  { key: "estado", header: "Estado", align: "center", render: (o) => <StatusBadge status={o.estado} /> },
  {
    key: "acciones",
    header: "Acciones",
    align: "center",
    render: (o) => (
      <BaseButton to={`/organizaciones/${o.idOrganizacion}`} variant="secondary" size="xs">
        Ver detalle
      </BaseButton>
    ),
  },
];

export default function OrganizacionesPage() {
  const mensaje = useFlashMessage();
  const f = useListFilters(EMPTY_FILTERS);

  const { data, loading, error } = useAsync(
    () => listarOrganizaciones({ ...f.filters, pagina: f.page, tamano: PAGE_SIZE }),
    [f.filters, f.page]
  );
  const { data: ciudades } = useAsync(obtenerCiudades, []);

  const modoFiltros = f.showFilters;

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio", to: "/inicio" }, { label: "Organizaciones" }]}
        title={modoFiltros ? "Filtrar y buscar organizaciones" : "Lista de organizaciones"}
        description={
          modoFiltros
            ? "Encuentra organizaciones por nombre, ubicación o estado."
            : "Consulta y administra las organizaciones registradas."
        }
      />

      {mensaje && <Alert variant="success" className="org-alert">{mensaje}</Alert>}

      {modoFiltros ? (
        <FilterPanel onApply={f.apply} onClear={f.clear} onClose={f.closeFilters}>
          <BaseInput
            label="Nombre de organización"
            name="buscar"
            placeholder="Escribe para buscar"
            value={f.draft.buscar}
            onChange={f.handleDraftChange}
          />
          <BaseSelect label="Estado" name="estado" placeholder="Todos" options={ESTADOS} value={f.draft.estado} onChange={f.handleDraftChange} />
          <BaseSelect label="Tipo de organización" name="tipo" placeholder="Todos" options={TIPOS_ORGANIZACION} value={f.draft.tipo} onChange={f.handleDraftChange} />
          <BaseSelect label="Ciudad" name="ciudad" placeholder="Todas" options={ciudades ?? []} value={f.draft.ciudad} onChange={f.handleDraftChange} />
        </FilterPanel>
      ) : (
        <ListToolbar
          search={f.search}
          onSearchChange={f.setSearch}
          searchPlaceholder="Buscar organización..."
          onToggleFilters={f.openFilters}
          activeFilters={f.activeCount}
        />
      )}

      {error && <Alert variant="error" className="org-alert">{error}</Alert>}

      <DataTable
        caption="Organizaciones registradas"
        columns={COLUMNS}
        rows={data?.items ?? []}
        rowKey="idOrganizacion"
        loading={loading}
        empty={{
          title: "No se encontraron organizaciones",
          description: f.hasFilters ? "Prueba con otros criterios de búsqueda." : "Aún no hay organizaciones registradas.",
          action: f.hasFilters && (
            <BaseButton variant="secondary" onClick={f.clear}>Limpiar filtros</BaseButton>
          ),
        }}
        footer={
          data && (
            <Pagination
              page={data.pagina}
              totalPages={data.totalPaginas}
              total={data.total}
              pageSize={PAGE_SIZE}
              onChange={f.setPage}
            />
          )
        }
      />
    </>
  );
}