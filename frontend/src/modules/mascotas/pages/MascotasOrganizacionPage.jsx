import { useParams } from "react-router-dom";
import BackLink from "../../../components/ui/BackLink";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import { obtenerOrganizacion } from "../../organizaciones/services/organizacionesService";
import MascotasExplorer from "../components/MascotasExplorer";
import { listarMascotasDeOrganizacion } from "../services/mascotasService";
import "../../../styles/Mascotas.css";

export default function MascotasOrganizacionPage() {
  const { idOrganizacion } = useParams();
  const { data: org } = useAsync(() => obtenerOrganizacion(idOrganizacion), [idOrganizacion]);

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Mascotas", to: "/mascotas" },
          { label: org?.nombre ?? "Organización" },
        ]}
        title={org ? `Mascotas de ${org.nombre}` : "Mascotas"}
        description="Consulta, busca y filtra las mascotas de esta organización."
        actions={<BackLink to="/mascotas">Todas las organizaciones</BackLink>}
      />

      <MascotasExplorer
        fetcher={(p) => listarMascotasDeOrganizacion(idOrganizacion, p)}
        fetchKey={idOrganizacion}
      />
    </>
  );
}