import { useMemo } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import DataTable from "../../../components/layout/DataTable";
import EntityCell from "../../../components/ui/EntityCell";
import FilterPanel from "../../../components/ui/FilterPanel";
import ListToolbar from "../../../components/ui/ListToolbar";
import Pagination from "../../../components/layout/Pagination";
import PageHeader from "../../../components/layout/PageHeader";
import StatusBadge, { statusLabel } from "../../../components/ui/StatusBadge";
import { ROLES } from "../../../components/layout/navigation";
import { tituloMascota } from "../../../constants/seguimiento";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { useListFilters } from "../../../hooks/useListFilters";
import { formatDate } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { getUser } from "../../auth/services/authService";
import { listarEstadosAdaptacion, listarSeguimientos } from "../services/seguimientoService";
import "../../../styles/Seguimiento.css";

const PAGE_SIZE = 10;
const EMPTY_FILTERS = { buscar: "", idEstadoAdaptacion: "", desde: "", hasta: "" };

function buildColumns(esAdmin) {
  const columns = [
    {
      key: "mascota",
      header: "Mascota",
      render: (s) => <EntityCell name={tituloMascota(s.mascota, s.especie)} imageUrl={mediaUrl(s.fotoUrl)} />,
    },
    { key: "adoptante", header: "Adoptante" },
    { key: "fechaAdopcion", header: "Fecha de adopción", render: (s) => formatDate(s.fechaAdopcion) },
    {
      key: "estado",
      header: "Estado",
      align: "center",
      render: (s) => <StatusBadge status={s.estadoAdaptacion} />,
    },
    {
      key: "acciones",
      header: "Acciones",
      align: "center",
      render: (s) => (
        <BaseButton to={`/seguimiento/${s.idAdopcion}`} variant="secondary" size="xs">
          Ver detalle
        </BaseButton>
      ),
    },
  ];

  // El admin supervisa varias organizaciones: necesita saber de cuál es cada una
  if (esAdmin) columns.splice(2, 0, { key: "organizacion", header: "Organización" });
  return columns;
}

export default function SeguimientoListaPage() {
  const esAdmin = getUser()?.rol === ROLES.ADMIN;
  const mensaje = useFlashMessage();
  const f = useListFilters(EMPTY_FILTERS);

  const { data, loading, error } = useAsync(
    () => listarSeguimientos({ ...f.filters, pagina: f.page, tamano: PAGE_SIZE }),
    [f.filters, f.page]
  );
  const { data: estados } = useAsync(listarEstadosAdaptacion, []);

  const opcionesEstado = useMemo(
    () => (estados ?? []).map((e) => ({ value: String(e.idEstadoAdaptacion), label: statusLabel(e.nombre) })),
    [estados]
  );
  const columns = useMemo(() => buildColumns(esAdmin), [esAdmin]);

  const modoFiltros = f.showFilters;

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio", to: "/inicio" }, { label: "Seguimiento de mascotas" }]}
        title={modoFiltros ? "Filtrar seguimientos" : "Mascotas adoptadas"}
        description={
          modoFiltros
            ? "Localiza seguimientos por mascota, estado o fecha."
            : esAdmin
              ? "Consulta las mascotas adoptadas en todas las organizaciones."
              : "Consulta las mascotas adoptadas por tu organización."
        }
      />

      {mensaje && <Alert variant="success" className="seg-alert">{mensaje}</Alert>}

      {modoFiltros ? (
        <FilterPanel onApply={f.apply} onClear={f.clear} onClose={f.closeFilters}>
          <BaseInput
            label="Nombre de mascota"
            name="buscar"
            placeholder="Escribe para buscar"
            value={f.draft.buscar}
            onChange={f.handleDraftChange}
          />
          <BaseSelect
            label="Estado de adaptación"
            name="idEstadoAdaptacion"
            placeholder="Todos"
            options={opcionesEstado}
            value={f.draft.idEstadoAdaptacion}
            onChange={f.handleDraftChange}
          />
          <BaseInput label="Fecha desde" type="date" name="desde" max={f.draft.hasta || undefined} value={f.draft.desde} onChange={f.handleDraftChange} />
          <BaseInput label="Fecha hasta" type="date" name="hasta" min={f.draft.desde || undefined} value={f.draft.hasta} onChange={f.handleDraftChange} />
        </FilterPanel>
      ) : (
        <ListToolbar
          search={f.search}
          onSearchChange={f.setSearch}
          searchPlaceholder="Buscar mascota..."
          onToggleFilters={f.openFilters}
          activeFilters={f.activeCount}
          filterLabel="Filtro"
        />
      )}

      {error && <Alert variant="error" className="seg-alert">{error}</Alert>}

      <DataTable
        caption="Mascotas adoptadas en seguimiento"
        columns={columns}
        rows={data?.items ?? []}
        rowKey="idAdopcion"
        loading={loading}
        empty={{
          title: "No se encontraron seguimientos",
          description: f.hasFilters
            ? "Prueba con otros criterios de búsqueda."
            : "Cuando una mascota sea adoptada aparecerá aquí.",
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
