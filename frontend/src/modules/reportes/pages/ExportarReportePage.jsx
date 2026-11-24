import { useState } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import Card from "../../../components/ui/Card";
import PageHeader from "../../../components/layout/PageHeader";
import { FORMATOS_REPORTE, REPORTES_BREADCRUMB, TIPOS_REPORTE } from "../../../constants/reportes";
import ReportesNav from "../components/ReportesNav";
import { exportarReporte } from "../services/reportesService";
import "../../../styles/Reportes.css";

const INICIAL = { tipo: "", desde: "", hasta: "", formato: "", nombre: "" };

/** Mismo criterio que el servidor: letras, números, guion y guion bajo. */
const limpiarNombre = (n) =>
  n.trim().replace(/\.(pdf|xlsx?)$/i, "").replace(/[^A-Za-z0-9_-]+/g, "_").replace(/^_+|_+$/g, "").slice(0, 80);

export default function ExportarReportePage() {
  const [form, setForm] = useState(INICIAL);
  const [errors, setErrors] = useState({});
  const [error, setError] = useState("");
  const [exito, setExito] = useState("");
  const [loading, setLoading] = useState(false);

  const set = (campo) => (e) => {
    setForm((f) => ({ ...f, [campo]: e.target.value }));
    setErrors((er) => ({ ...er, [campo]: undefined }));
  };

  const validar = () => {
    const er = {};
    if (!form.tipo) er.tipo = "Selecciona el tipo de reporte.";
    if (!form.formato) er.formato = "Selecciona el formato.";
    if (form.desde && form.hasta && form.desde > form.hasta)
      er.hasta = "La fecha de fin no puede ser anterior a la de inicio.";
    setErrors(er);
    return Object.keys(er).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setExito("");
    if (!validar()) return;

    const formato = FORMATOS_REPORTE.find((f) => f.value === form.formato);
    const base = limpiarNombre(form.nombre) || `reporte_${form.tipo}_${new Date().toISOString().slice(0, 10)}`;

    setLoading(true);
    try {
      await exportarReporte({ ...form, nombre: base }, `${base}.${formato.extension}`);
      setExito(`Se descargó ${base}.${formato.extension}.`);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader
        breadcrumbs={REPORTES_BREADCRUMB}
        title="Generar y exportar reportes"
        description="Selecciona el tipo de reporte, periodo y formato."
      />
      <ReportesNav />

      {error && <Alert variant="error" className="reportes-alert">{error}</Alert>}
      {exito && <Alert variant="success" className="reportes-alert">{exito}</Alert>}

      <Card padded className="reportes-export">
        <h2 className="reportes-export__title">Información y detalles</h2>

        <form onSubmit={handleSubmit} noValidate>
          <div className="reportes-export__grid">
            <BaseSelect label="Tipo de reporte" required placeholder="Selecciona un reporte" options={TIPOS_REPORTE}
              value={form.tipo} onChange={set("tipo")} error={errors.tipo} />
            <BaseInput label="Fecha de inicio" type="date" value={form.desde} onChange={set("desde")}
              hint="Opcional. Vacía = desde el principio." />
            <BaseInput label="Fecha de fin" type="date" value={form.hasta} onChange={set("hasta")}
              error={errors.hasta} hint="Opcional. Vacía = hasta hoy." />
            <BaseSelect label="Formato" required placeholder="PDF / Excel" options={FORMATOS_REPORTE}
              value={form.formato} onChange={set("formato")} error={errors.formato} />
            <BaseInput label="Nombre del archivo" placeholder="reporte_adopta_plus_octubre" maxLength={80}
              value={form.nombre} onChange={set("nombre")} hint="Sin extensión. Si lo dejas vacío se genera uno." />
          </div>

          <div className="reportes-export__actions">
            <BaseButton type="submit" loading={loading}>Generar reporte</BaseButton>
            <BaseButton variant="secondary" to="/reportes" disabled={loading}>Cancelar</BaseButton>
          </div>
        </form>
      </Card>
    </>
  );
}
