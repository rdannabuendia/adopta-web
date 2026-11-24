import { useState } from "react";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseCheckbox from "../../../components/ui/BaseCheckbox";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Card from "../../../components/ui/Card";
import PhotoPicker from "../../../components/ui/PhotoPicker";
import TagInput from "../../../components/ui/TagInput";
import {
  CARACTERISTICAS_SUGERIDAS, EDADES, ESPECIES, MAX_FOTOS, RAZAS_COMUNES, SEXOS, TAMANOS,
} from "../../../constants/mascota";
import "../../../styles/Mascotas.css";

export const EMPTY_MASCOTA = {
  nombre: "", especie: "", raza: "", sexo: "Desconocido", tamano: "", edadAproximada: "",
  color: "", peso: "", descripcion: "", estadoSalud: "",
  vacunado: false, esterilizado: false, caracteristicas: [],
};

export default function MascotaForm({
  initialValues = EMPTY_MASCOTA,
  initialPhotos = { items: [], principalKey: null },
  submitLabel = "Guardar mascota",
  saving = false,
  error = "",
  cancelTo,
  onSubmit,
}) {
  const [v, setV] = useState({ ...EMPTY_MASCOTA, ...initialValues });
  const [photos, setPhotos] = useState(initialPhotos);
  const [errors, setErrors] = useState({});

  const set = (name, value) => {
    setV((x) => ({ ...x, [name]: value }));
    setErrors((e) => ({ ...e, [name]: undefined }));
  };
  const onChange = (e) => {
    const { name, type, value, checked } = e.target;
    set(name, type === "checkbox" ? checked : value);
  };

  const validate = () => {
    const er = {};
    if (!v.nombre.trim()) er.nombre = "Ingresa el nombre.";
    if (!v.especie) er.especie = "Selecciona la especie.";
    if (!v.raza.trim()) er.raza = "Ingresa la raza (puedes usar “Mestizo”).";
    if (!v.tamano) er.tamano = "Selecciona el tamaño.";
    if (!v.edadAproximada) er.edadAproximada = "Selecciona la edad.";
    if (!v.descripcion.trim()) er.descripcion = "Añade una descripción.";
    if (v.peso !== "" && !(Number(v.peso) > 0 && Number(v.peso) <= 150)) er.peso = "Debe estar entre 0.1 y 150 kg.";
    if (photos.items.length === 0) er.fotos = "Agrega al menos una fotografía.";
    return er;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const er = validate();
    setErrors(er);
    if (Object.keys(er).length) {
      document.querySelector(".has-error, .photo-picker__error")?.scrollIntoView({ behavior: "smooth", block: "center" });
      return;
    }
    onSubmit({ values: v, photos });
  };

  const razas = RAZAS_COMUNES[v.especie] ?? [];

  return (
    <form onSubmit={handleSubmit} noValidate>
      <Card padded className="pet-form">
        {error && <Alert variant="error" className="mascotas-alert">{error}</Alert>}

        <div className="pet-form__layout">
          <aside className="pet-form__photos">
            <PhotoPicker
              label="Fotografías"
              required
              max={MAX_FOTOS}
              items={photos.items}
              principalKey={photos.principalKey}
              onChange={setPhotos}
              error={errors.fotos}
            />
          </aside>

          <div className="pet-form__fields">
            <section className="pet-form__section">
              <h2>Datos básicos</h2>
              <div className="pet-form__grid">
                <BaseInput label="Nombre" name="nombre" maxLength={80} value={v.nombre} onChange={onChange} error={errors.nombre} required />
                <BaseSelect label="Especie" name="especie" options={ESPECIES} value={v.especie} onChange={onChange} error={errors.especie} required />
                <BaseInput
                  label="Raza" name="raza" maxLength={80} list="razas-sugeridas"
                  value={v.raza} onChange={onChange} error={errors.raza} required
                />
                <datalist id="razas-sugeridas">{razas.map((r) => <option key={r} value={r} />)}</datalist>
                <BaseSelect label="Sexo" name="sexo" options={SEXOS} value={v.sexo} onChange={onChange} />
              </div>
            </section>

            <section className="pet-form__section">
              <h2>Características físicas</h2>
              <div className="pet-form__grid pet-form__grid--3">
                <BaseSelect label="Tamaño" name="tamano" options={TAMANOS} value={v.tamano} onChange={onChange} error={errors.tamano} required />
                <BaseSelect label="Edad" name="edadAproximada" options={EDADES} value={v.edadAproximada} onChange={onChange} error={errors.edadAproximada} required />
                <BaseInput label="Color" name="color" maxLength={60} value={v.color} onChange={onChange} />
                <BaseInput
                  label="Peso (kg)" name="peso" type="number" step="0.1" min="0.1" max="150"
                  value={v.peso} onChange={onChange} error={errors.peso}
                />
              </div>
            </section>

            <section className="pet-form__section">
              <h2>Salud</h2>
              <div className="pet-form__checks">
                <BaseCheckbox name="vacunado" checked={v.vacunado} onChange={onChange}>Vacunas al día</BaseCheckbox>
                <BaseCheckbox name="esterilizado" checked={v.esterilizado} onChange={onChange}>Esterilizado/a</BaseCheckbox>
              </div>
              <BaseTextarea
                label="Estado de salud" name="estadoSalud" rows={2} maxLength={1000} showCount
                placeholder="Tratamientos, condiciones, cuidados especiales…"
                value={v.estadoSalud} onChange={onChange}
              />
            </section>

            <section className="pet-form__section">
              <h2>Sobre la mascota</h2>
              <BaseTextarea
                label="Descripción" name="descripcion" rows={5} maxLength={2000} showCount
                placeholder="Añade descripción…"
                value={v.descripcion} onChange={onChange} error={errors.descripcion} required
              />
              <TagInput
                label="Características"
                value={v.caracteristicas}
                onChange={(tags) => set("caracteristicas", tags)}
                suggestions={CARACTERISTICAS_SUGERIDAS}
                hint="Hasta 8 rasgos de personalidad (cariñoso, juguetón…)."
              />
            </section>
          </div>
        </div>

        <div className="pet-form__actions">
          <BaseButton type="submit" loading={saving}>{submitLabel}</BaseButton>
          <BaseButton variant="secondary" to={cancelTo} disabled={saving}>Cancelar</BaseButton>
        </div>
      </Card>
    </form>
  );
}