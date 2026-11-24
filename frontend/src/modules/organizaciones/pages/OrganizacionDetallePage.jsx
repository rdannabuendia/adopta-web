import { useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import { obtenerOrganizacion } from "../services/organizacionesService";
import OrganizacionDetalleView from "./OrganizacionDetalleView";
import "../../../styles/Organizaciones.css";

export default function OrganizacionDetallePage() {
  const { id } = useParams();
  const { data: org, loading, error } = useAsync(() => obtenerOrganizacion(id), [id]);

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Organizaciones", to: "/organizaciones" },
          { label: "Detalle" },
        ]}
        title="Detalle de organización"
        description="Información general, actividad y contacto."
      />

      {error && <Alert variant="error" className="org-alert">{error}</Alert>}
      {loading && !org && <p className="org-state">Cargando…</p>}

      {org && (
        <OrganizacionDetalleView
          org={org}
          actions={
            org.estado === "Activa" && (
              <BaseButton to={`/organizaciones/${org.idOrganizacion}/baja`}>
                Dar de baja por incumplimiento
              </BaseButton>
            )
          }
        />
      )}
    </>
  );
}