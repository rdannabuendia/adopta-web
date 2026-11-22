import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BackLink from "../../../components/ui/BackLink";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import { mediaUrl } from "../../../utils/media";
import MascotaForm from "../components/MascotaForm";
import {
  actualizarMascota, buildMascotaFormData, crearMascota, obtenerMascota,
} from "../services/mascotasService";
import "../../../styles/Mascotas.css";

export default function MascotaFormPage() {
  const { id } = useParams();
  const editando = Boolean(id);
  const navigate = useNavigate();

  const { data: m, loading, error: loadError } = useAsync(
    () => (editando ? obtenerMascota(id) : Promise.resolve(null)),
    [id]
  );
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const volver = editando ? `/mascotas/${id}` : "/mascotas";

  const handleSubmit = async ({ values, photos }) => {
    setError("");
    setSaving(true);
    try {
      const fd = buildMascotaFormData(values, photos, m?.fotos.map((f) => f.idFoto) ?? []);
      const res = editando ? await actualizarMascota(id, fd) : await crearMascota(fd);
      navigate(volver, { state: { mensaje: res.mensaje } });
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  const listo = !loading && (!editando || m);

  const initialValues = m && {
    nombre: m.nombre, especie: m.especie, raza: m.raza ?? "", sexo: m.sexo, tamano: m.tamano ?? "",
    edadAproximada: m.edadAproximada ?? "", color: m.color ?? "", peso: m.peso != null ? String(m.peso) : "",
    descripcion: m.descripcion ?? "", estadoSalud: m.estadoSalud ?? "",
    vacunado: m.vacunado, esterilizado: m.esterilizado, caracteristicas: m.caracteristicas,
  };
  const initialPhotos = m
    ? {
        items: m.fotos.map((f) => ({ key: `f${f.idFoto}`, id: f.idFoto, src: mediaUrl(f.url) })),
        principalKey: m.fotos.find((f) => f.esPrincipal) ? `f${m.fotos.find((f) => f.esPrincipal).idFoto}` : null,
      }
    : undefined;

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Mascotas", to: "/mascotas" },
          { label: editando ? "Editar mascota" : "Nueva mascota" },
        ]}
        title={editando ? "Editar mascota" : "Nueva mascota"}
        description={editando ? "Edita los datos de la mascota." : "Llena los campos para registrar una nueva mascota."}
        actions={<BackLink to={volver}>Volver</BackLink>}
      />

      {loadError && <Alert variant="error" className="mascotas-alert">{loadError}</Alert>}
      {loading && <p className="mascotas-state">Cargando…</p>}

      {listo && (
        <MascotaForm
          initialValues={initialValues}
          initialPhotos={initialPhotos}
          submitLabel={editando ? "Guardar cambios" : "Guardar mascota"}
          saving={saving}
          error={error}
          cancelTo={volver}
          onSubmit={handleSubmit}
        />
      )}
    </>
  );
}