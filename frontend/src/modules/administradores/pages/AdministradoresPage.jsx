import { useState } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import Badge from "../../../components/ui/Badge";
import ConfirmDialog from "../../../components/ui/ConfirmDialog";
import DataTable from "../../../components/layout/DataTable";
import EntityCell from "../../../components/ui/EntityCell";
import FilterPanel from "../../../components/ui/FilterPanel";
import ListToolbar from "../../../components/ui/ListToolbar";
import Pagination from "../../../components/layout/Pagination";
import PageHeader from "../../../components/layout/PageHeader";
import StatusBadge from "../../../components/ui/StatusBadge";
import { useAsync } from "../../../hooks/useAsync";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import { useListFilters } from "../../../hooks/useListFilters";
import {
  cambiarEstadoAdministrador,
  listarAdministradores,
  reenviarCredenciales,
} from "../services/administradoresService";
import "../../../styles/Administradores.css";

const PAGE_SIZE = 10;
const EMPTY_FILTERS = { buscar: "", estado: "" };
const ESTADOS = ["Activo", "Inactivo"];

const fecha = (iso) =>
  iso ? new Date(iso).toLocaleDateString("es-MX", { day: "2-digit", month: "short", year: "numeric" }) : "—";

export default function AdministradoresPage() {
  const flash = useFlashMessage();
  const f = useListFilters(EMPTY_FILTERS);
  const [aviso, setAviso] = useState({ tipo: "", texto: "" });
  const [accion, setAccion] = useState(null); // { tipo: "estado" | "reenviar", admin }
  const [busy, setBusy] = useState(false);

  const { data, loading, error, reload } = useAsync(
    () => listarAdministradores({ ...f.filters, pagina: f.page, tamano: PAGE_SIZE }),
    [f.filters, f.page]
  );

  const confirmar = async () => {
    if (!accion) return;
    const { tipo, admin } = accion;
    setBusy(true);
    try {
      const res =
        tipo === "reenviar"
          ? await reenviarCredenciales(admin.idUsuario)
          : await cambiarEstadoAdministrador(admin.idUsuario, admin.estado === "Activo" ? "Inactivo" : "Activo");
      setAviso({ tipo: "success", texto: res.mensaje });
      reload();
    } catch (err) {
      setAviso({ tipo: "error", texto: err.message });
    } finally {
      setBusy(false);
      setAccion(null);
    }
  };

  const columns = [
    {
      key: "administrador",
      header: "Administrador",
      render: (a) => (
        <EntityCell name={a.nombre} subtitle={a.esUsuarioActual ? `${a.correo} · Tú` : a.correo} />
      ),
    },
    { key: "telefono", header: "Teléfono" },
    { key: "fechaRegistro", header: "Registro", render: (a) => fecha(a.fechaRegistro) },
    {
      key: "estado",
      header: "Estado",
      align: "center",
      render: (a) => (
        <div className="admin-status">
          <StatusBadge status={a.estado} />
          {a.pendienteCambioPassword && <Badge size="sm" tone="warning">Contraseña pendiente</Badge>}
        </div>
      ),
    },
    {
      key: "acciones",
      header: "Acciones",
      align: "center",
      render: (a) => (
        <div className="admin-actions">
          <BaseButton to={`/administradores/${a.idUsuario}/editar`} variant="secondary" size="xs">
            Editar
          </BaseButton>
          {a.pendienteCambioPassword && a.estado === "Activo" && (
            <BaseButton variant="secondary" size="xs" onClick={() => setAccion({ tipo: "reenviar", admin: a })}>
              Reenviar credenciales
            </BaseButton>
          )}
          {!a.esUsuarioActual && a.estado !== "Bloqueado" && (
            <BaseButton variant="secondary" size="xs" onClick={() => setAccion({ tipo: "estado", admin: a })}>
              {a.estado === "Activo" ? "Desactivar" : "Reactivar"}
            </BaseButton>
          )}
        </div>
      ),
    },
  ];

  const modoFiltros = f.showFilters;
  const desactivando = accion?.tipo === "estado" && accion.admin.estado === "Activo";

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio", to: "/inicio" }, { label: "Administradores" }]}
        title={modoFiltros ? "Filtrar y buscar administradores" : "Administradores"}
        description={
          modoFiltros
            ? "Encuentra administradores por nombre, correo o estado."
            : "Registra a otras personas con acceso de administrador."
        }
        actions={<BaseButton to="/administradores/nuevo">Registrar administrador</BaseButton>}
      />

      {flash && <Alert variant="success" className="admin-alert">{flash}</Alert>}
      {aviso.texto && <Alert variant={aviso.tipo} className="admin-alert">{aviso.texto}</Alert>}

      {modoFiltros ? (
        <FilterPanel onApply={f.apply} onClear={f.clear} onClose={f.closeFilters}>
          <BaseInput
            label="Nombre o correo"
            name="buscar"
            placeholder="Escribe para buscar"
            value={f.draft.buscar}
            onChange={f.handleDraftChange}
          />
          <BaseSelect
            label="Estado"
            name="estado"
            placeholder="Todos"
            options={ESTADOS}
            value={f.draft.estado}
            onChange={f.handleDraftChange}
          />
        </FilterPanel>
      ) : (
        <ListToolbar
          search={f.search}
          onSearchChange={f.setSearch}
          searchPlaceholder="Buscar administrador..."
          onToggleFilters={f.openFilters}
          activeFilters={f.activeCount}
        />
      )}

      {error && <Alert variant="error" className="admin-alert">{error}</Alert>}

      <DataTable
        caption="Administradores registrados"
        columns={columns}
        rows={data?.items ?? []}
        rowKey="idUsuario"
        loading={loading}
        empty={{
          title: "No se encontraron administradores",
          description: f.hasFilters ? "Prueba con otros criterios de búsqueda." : "Aún no hay administradores registrados.",
          action: f.hasFilters && <BaseButton variant="secondary" onClick={f.clear}>Limpiar filtros</BaseButton>,
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

      <ConfirmDialog
        open={Boolean(accion)}
        title={
          accion?.tipo === "reenviar"
            ? "Reenviar credenciales"
            : desactivando
              ? "Desactivar administrador"
              : "Reactivar administrador"
        }
        confirmLabel={accion?.tipo === "reenviar" ? "Reenviar" : desactivando ? "Desactivar" : "Reactivar"}
        busy={busy}
        onConfirm={confirmar}
        onClose={() => setAccion(null)}
      >
        {accion?.tipo === "reenviar" && (
          <p>
            Se generará una <strong>nueva contraseña temporal</strong> para <strong>{accion.admin.nombre}</strong> y
            se enviará a {accion.admin.correo}. La anterior dejará de funcionar.
          </p>
        )}
        {accion?.tipo === "estado" && desactivando && (
          <p>
            <strong>{accion.admin.nombre}</strong> ya no podrá iniciar sesión. Su historial se conserva y puedes
            reactivarlo cuando quieras.
          </p>
        )}
        {accion?.tipo === "estado" && !desactivando && (
          <p><strong>{accion.admin.nombre}</strong> podrá volver a iniciar sesión.</p>
        )}
      </ConfirmDialog>
    </>
  );
}
