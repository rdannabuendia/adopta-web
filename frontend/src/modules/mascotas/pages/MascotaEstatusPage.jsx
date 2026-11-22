import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BackLink from "../../../components/ui/BackLink";
import BaseButton from "../../../components/ui/BaseButton";
import BaseSelect from "../../../components/ui/BaseSelect";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import StatusBadge from "../../../components/ui/StatusBadge";
import PageHeader from "../../../components/layout/PageHeader";
import { estadoLabel, resumenMascota } from "../../../constants/mascota";
import { useAsync } from "../../../hooks/useAsync";
import { mediaUrl } from "../../../utils/media";
import { cambiarEstadoMascota, obtenerMascota } from "../services/mascotasService";
import "../../../styles/Mascotas.css";

export default function MascotaEstatusPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: m, error: loadError } = useAsync(() => obtenerMascota(id), [id]);

  const [estado, setEstado] = useState("");
  const [motivo, setMotivo] = useState("");
  const [fieldError, setFieldError] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const volver = `/mascotas/${id}`;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!estado) return setFieldError("Selecciona el nuevo estatus.");

    setSaving(true);
    try {
      const res = await cambiarEstadoMascota(id, { estado, motivo: motivo.trim() });
      navigate(volver, { state: { mensaje: res.mensaje } });
    } catch (err) {
      setError(err.message);
      setSaving(false);
    }
  };

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Mascotas", to: "/mascotas" },
          { label: "Cambiar estatus" },
        ]}
        title="Cambiar estatus"
        description="Selecciona el nuevo estado de la mascota para actualizar su disponibilidad en la plataforma."
      />

      {loadError && <Alert variant="error" className="mascotas-alert">{loadError}</Alert>}
      {error && <Alert variant="error" className="mascotas-alert">{error}</Alert>}

      {m && (
        <Card padded className="pet-detail">
          <BackLink to={volver} />

          {m.estadosDestino.length === 0 ? (
            <Alert variant="info">
              Esta mascota está en un proceso de adopción; su estatus cambia desde las solicitudes.
            </Alert>
          ) : (
            <form className="pet-detail__grid" onSubmit={handleSubmit} noValidate>
              <div className="pet-status__photo">
                {m.fotos[0] ? <img src={mediaUrl(m.fotos[0].url)} alt={`Foto de ${m.nombre}`} /> : null}
              </div>

              <div className="pet-detail__info">
                <h2 className="pet-detail__name">{m.nombre}</h2>
                <p className="pet-detail__summary">{resumenMascota(m)}</p>

                <section>
                  <h3>Estatus actual</h3>
                  <StatusBadge status={m.estado} />
                </section>

                <BaseSelect
                  label="Nuevo estatus"
                  name="estado"
                  placeholder="Selecciona una opción…"
                  options={m.estadosDestino.map((e) => ({ value: e, label: estadoLabel(e) }))}
                  value={estado}
                  onChange={(e) => { setEstado(e.target.value); setFieldError(""); }}
                  error={fieldError}
                  required
                />

                <BaseTextarea
                  label="Descripción (motivo opcional)"
                  placeholder="Escribe aquí…"
                  rows={4}
                  maxLength={500}
                  showCount
                  value={motivo}
                  onChange={(e) => setMotivo(e.target.value)}
                />

                <div className="pet-form__actions pet-form__actions--left">
                  <BaseButton type="submit" loading={saving}>Cambiar estatus</BaseButton>
                  <BaseButton variant="secondary" to={volver} disabled={saving}>Cancelar</BaseButton>
                </div>
              </div>
            </form>
          )}
        </Card>
      )}
    </>
  );
}