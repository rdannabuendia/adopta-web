import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import PageHeader from "../../../components/layout/PageHeader";
import { useFlashMessage } from "../../../hooks/useFlashMessage";
import MascotasExplorer from "../components/MascotasExplorer";
import { listarMisMascotas } from "../services/mascotasService";
import "../../../styles/Mascotas.css";

export default function MisMascotasPage() {
  const mensaje = useFlashMessage();

  return (
    <>
      <PageHeader
        breadcrumbs={[{ label: "Inicio", to: "/inicio" }, { label: "Mascotas" }]}
        title="Mis mascotas"
        description="Consulta y administra tus mascotas registradas."
        actions={<BaseButton to="/mascotas/nueva">+ Nueva mascota</BaseButton>}
      />

      {mensaje && <Alert variant="success" className="mascotas-alert">{mensaje}</Alert>}

      <MascotasExplorer
        fetcher={listarMisMascotas}
        fetchKey="mias"
        emptyAction={<BaseButton to="/mascotas/nueva">Registrar mi primera mascota</BaseButton>}
      />
    </>
  );
}