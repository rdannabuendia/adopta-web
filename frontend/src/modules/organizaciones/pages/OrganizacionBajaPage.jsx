import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import { darDeBaja, listarMotivosBaja, obtenerOrganizacion } from "../services/organizacionesService";
import "../../../styles/Organizaciones.css";

const hoy = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
};

export default function OrganizacionBajaPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: org, error: loadError } = useAsync(() => obtenerOrganizacion(id), [id]);
  const { data: motivos } = useAsync(listarMotivosBaja, []);

  const [form, setForm] = useState({ idMotivoBaja: "", descripcion: "", fechaRevision: hoy() });
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((f) => ({ ...f, [name]: value }));
    setErrors((er) => ({ ...er, [name]: undefined }));
  };

  const validate = () => {
    const er = {};
    if (!form.idMotivoBaja) er.idMotivoBaja = "Selecciona un motivo.";
    if (form.descripcion.trim().length < 10) er.descripcion = "Describe los hechos (mínimo 10 caracteres).";
    if (!form.fechaRevision) er.fechaRevision = "Indica la fecha de revisión.";
    else if (form.fechaRevision > hoy()) er.fechaRevision = "La fecha no puede ser futura.";
    return er;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitError("");
    const er = validate();
    setErrors(er);
    if (Object.keys(er).length) return;

    setSaving(true);
    try {
      const res = await darDeBaja(id, {
        idMotivoBaja: Number(form.idMotivoBaja),
        descripcion: form.descripcion.trim(),
        fechaRevision: form.fechaRevision,
      });
      navigate("/organizaciones", { state: { mensaje: res.mensaje } });
    } catch (err) {
      setSubmitError(err.message);
      setSaving(false);
    }
  };

  const volver = `/organizaciones/${id}`;
  const noActiva = org && org.estado !== "Activa";

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Organizaciones", to: "/organizaciones" },
          { label: "Dar de baja" },
        ]}
        title="Dar de baja por incumplimiento"
        description="Registra el incumplimiento antes de suspender una organización."
      />

      {loadError && <Alert variant="error" className="org-alert">{loadError}</Alert>}
      {noActiva && (
        <Alert variant="error" className="org-alert">Solo se puede dar de baja a una organización activa.</Alert>
      )}
      {submitError && <Alert variant="error" className="org-alert">{submitError}</Alert>}

      {org && !noActiva && (
        <form onSubmit={handleSubmit} noValidate>
          <Card padded>
            <h2 className="org-form__title">Información y detalles</h2>

            <div className="org-form__grid">
              <BaseInput label="Organización" value={org.nombre} readOnly />
              <BaseSelect
                label="Motivo"
                name="idMotivoBaja"
                placeholder="Selecciona un motivo"
                options={(motivos ?? []).map((m) => ({ value: m.idMotivoBaja, label: m.nombre }))}
                value={form.idMotivoBaja}
                onChange={handleChange}
                error={errors.idMotivoBaja}
                required
              />
              <BaseTextarea
                label="Descripción"
                name="descripcion"
                placeholder="Describe los hechos y evidencias"
                rows={4}
                maxLength={2000}
                showCount
                value={form.descripcion}
                onChange={handleChange}
                error={errors.descripcion}
                required
              />
              <BaseInput
                label="Fecha de revisión"
                name="fechaRevision"
                type="date"
                max={hoy()}
                value={form.fechaRevision}
                onChange={handleChange}
                error={errors.fechaRevision}
                required
              />
            </div>

            <div className="org-form__actions">
              <BaseButton type="submit" disabled={saving}>
                {saving ? "Guardando…" : "Confirmar baja"}
              </BaseButton>
              <BaseButton variant="secondary" to={volver}>Cancelar</BaseButton>
            </div>
          </Card>
        </form>
      )}
    </>
  );
}