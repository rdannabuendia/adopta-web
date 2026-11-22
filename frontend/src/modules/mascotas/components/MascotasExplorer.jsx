import Alert from "../../../components/ui/Alert";
import Avatar from "../../../components/ui/Avatar";
import BaseButton from "../../../components/ui/BaseButton";
import Card from "../../../components/ui/Card";
import CardGrid from "../../../components/ui/CardGrid";
import EmptyState from "../../../components/ui/EmptyState";
import { PawIcon } from "../../../components/ui/Icons";
import InlineFilters from "../../../components/ui/InlineFilters";
import MediaCard from "../../../components/ui/MediaCard";
import Pagination from "../../../components/layout/Pagination";
import StatusBadge from "../../../components/ui/StatusBadge";
import { ESPECIES, EDADES, ESTADOS_MASCOTA, resumenMascota, TAMANOS } from "../../../constants/mascota";
import { useAsync } from "../../../hooks/useAsync";
import { useInstantFilters } from "../../../hooks/useInstantFilters";
import { mediaUrl } from "../../../utils/media";
import "../../../styles/Mascotas.css";

const PAGE_SIZE = 9;
const EMPTY = { especie: "", tamano: "", edad: "", estado: "" };

/**
 * fetcher(params) -> { items, pagina, totalPaginas, total }
 * fetchKey: cambia cuando cambia el origen de datos (ej. id de organización)
 * showOrganizacion: muestra la organización en la tarjeta (vista del admin)
 */
export default function MascotasExplorer({ fetcher, fetchKey, showOrganizacion = false, emptyAction }) {
  const f = useInstantFilters(EMPTY);

  const { data, loading, error } = useAsync(
    () => fetcher({ ...f.params, pagina: f.page, porPagina: PAGE_SIZE }),
    [fetchKey, f.params, f.page]
  );

  const filtros = [
    { name: "especie", label: "Especie", options: ESPECIES, value: f.filters.especie },
    { name: "tamano", label: "Tamaño", options: TAMANOS, value: f.filters.tamano },
    { name: "edad", label: "Edad", options: EDADES, value: f.filters.edad },
    { name: "estado", label: "Estado", options: ESTADOS_MASCOTA, value: f.filters.estado },
  ];

  const items = data?.items ?? [];

  return (
    <>
      <InlineFilters
        search={f.search}
        onSearchChange={f.setSearch}
        searchPlaceholder="Buscar por nombre o raza..."
        filters={filtros}
        onFilterChange={f.setFilter}
        onClear={f.clear}
        canClear={f.hasFilters}
      />

      {error && <Alert variant="error" className="mascotas-alert">{error}</Alert>}

      {loading && items.length === 0 ? (
        <p className="mascotas-state">Cargando…</p>
      ) : items.length === 0 ? (
        <Card padded>
          <EmptyState
            icon={<PawIcon width="32" height="32" />}
            title="No se encontraron mascotas"
            description={f.hasFilters ? "Prueba con otros criterios de búsqueda." : "Aún no hay mascotas registradas."}
            action={
              f.hasFilters
                ? <BaseButton variant="secondary" onClick={f.clear}>Limpiar filtros</BaseButton>
                : emptyAction
            }
          />
        </Card>
      ) : (
        <CardGrid className={loading ? "is-loading" : ""}>
          {items.map((m) => (
            <MediaCard
              key={m.idMascota}
              image={mediaUrl(m.fotoUrl)}
              imageAlt={`Foto de ${m.nombre}`}
              placeholder={<PawIcon width="44" height="44" />}
              badge={<StatusBadge status={m.estado} />}
              title={m.nombre}
              subtitle={resumenMascota(m)}
              meta={
                showOrganizacion && (
                  <span className="media-card__org">
                    <Avatar name={m.organizacion} src={mediaUrl(m.organizacionFotoUrl)} size="sm" />
                    {m.organizacion}
                  </span>
                )
              }
              footer={
                <BaseButton to={`/mascotas/${m.idMascota}`} variant="secondary" size="sm" fullWidth>
                  Ver detalle
                </BaseButton>
              }
            />
          ))}
        </CardGrid>
      )}

      {data && (
        <Pagination page={data.pagina} totalPages={data.totalPaginas} total={data.total} pageSize={PAGE_SIZE} onChange={f.setPage} />
      )}
    </>
  );
}