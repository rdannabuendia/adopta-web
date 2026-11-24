import Alert from "../../../components/ui/Alert";
import Avatar from "../../../components/ui/Avatar";
import BaseButton from "../../../components/ui/BaseButton";
import Card from "../../../components/ui/Card";
import CardGrid from "../../../components/ui/CardGrid";
import EmptyState from "../../../components/ui/EmptyState";
import InlineFilters from "../../../components/ui/InlineFilters";
import MediaCard from "../../../components/ui/MediaCard";
import Pagination from "../../../components/layout/Pagination";
import StatusBadge from "../../../components/ui/StatusBadge";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import { useInstantFilters } from "../../../hooks/useInstantFilters";
import { plural } from "../../../utils/format";
import { mediaUrl } from "../../../utils/media";
import { listarOrganizacionesConMascotas } from "../services/mascotasService";
import "../../../styles/Mascotas.css";

const PAGE_SIZE = 12;

export default function OrganizacionesMascotasPage() {
  const f = useInstantFilters({});
  const { data, loading, error } = useAsync(
    () => listarOrganizacionesConMascotas({ ...f.params, pagina: f.page, tamano: PAGE_SIZE }),
    [f.params, f.page]
  );
  const items = data?.items ?? [];

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio", to: "/inicio" }, { label: "Mascotas" }]}
        title="Mascotas por organización"
        description="Elige una organización para consultar sus mascotas."
      />

      <InlineFilters
        search={f.search}
        onSearchChange={f.setSearch}
        searchPlaceholder="Buscar organización o ciudad..."
      />

      {error && <Alert variant="error" className="mascotas-alert">{error}</Alert>}

      {loading && items.length === 0 ? (
        <p className="mascotas-state">Cargando…</p>
      ) : items.length === 0 ? (
        <Card padded>
          <EmptyState title="No se encontraron organizaciones" description="Prueba con otra búsqueda." />
        </Card>
      ) : (
        <CardGrid className={loading ? "is-loading" : ""}>
          {items.map((o) => (
            <MediaCard
              key={o.idOrganizacion}
              fit="contain"
              image={mediaUrl(o.fotoUrl)}
              imageAlt={`Logo de ${o.nombre}`}
              placeholder={<Avatar name={o.nombre} size="lg" />}
              badge={o.estado !== "Activa" && <StatusBadge status={o.estado} />}
              title={o.nombre}
              subtitle={[o.ciudad, o.tipoOrganizacion].filter(Boolean).join(" · ")}
              meta={
                <span className="media-card__stats">
                  <strong>{plural(o.totalMascotas, "mascota")}</strong>
                  <span>{o.disponibles} disponibles</span>
                </span>
              }
              footer={
                <BaseButton to={`/mascotas/organizacion/${o.idOrganizacion}`} size="sm" fullWidth>
                  Ver mascotas
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