import { useState } from "react";
import { Link } from "react-router-dom";
import BaseButton from "../../../components/ui/BaseButton";
import BaseCheckbox from "../../../components/ui/BaseCheckbox";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import FileUpload from "../../../components/ui/FileUpload";
import {
  BuildingIcon,
  CheckCircleIcon,
  FileTextIcon,
  PawIcon,
} from "../../../components/ui/Icons";
import { enviarSolicitudOrganizacion } from "../services/registroOrganizacionService";
import "../../../styles/LoginPage.css";
import "../../../styles/RegistroOrganizacion.css";
import { TIPOS_ORGANIZACION } from "../../../constants/organizacion";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const INITIAL_VALUES = {
  nombre: "",
  email: "",
  tipoOrganizacion: "",
  telefono: "",
  direccion: "",
  aceptaTerminos: false,
};

const INITIAL_FILES = {
  existenciaLegal: null,
  comprobanteDomicilio: null,
  identificacionResponsable: null,
};

function validate(values, files) {
  const errors = {};
  const digitos = values.telefono.replace(/\D/g, "");

  if (!values.nombre.trim()) errors.nombre = "Ingresa el nombre de la organización.";
  if (!values.email.trim()) errors.email = "Ingresa el correo electrónico.";
  else if (!EMAIL_REGEX.test(values.email)) errors.email = "El correo no es válido.";
  if (!values.tipoOrganizacion) errors.tipoOrganizacion = "Selecciona el tipo.";
  if (!values.telefono.trim()) errors.telefono = "Ingresa el teléfono.";
  else if (digitos.length < 10 || digitos.length > 15) errors.telefono = "Teléfono no válido.";
  if (!values.direccion.trim()) errors.direccion = "Ingresa la dirección.";

  if (!files.existenciaLegal) errors.existenciaLegal = "Documento obligatorio.";
  if (!files.comprobanteDomicilio) errors.comprobanteDomicilio = "Documento obligatorio.";
  if (!files.identificacionResponsable) errors.identificacionResponsable = "Documento obligatorio.";

  return errors;
}

export default function RegistroOrganizacionPage() {
  const [values, setValues] = useState(INITIAL_VALUES);
  const [files, setFiles] = useState(INITIAL_FILES);
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setValues((prev) => ({ ...prev, [name]: type === "checkbox" ? checked : value }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
    setServerError("");
  };

  const handleFile = (name) => (file) => {
    setFiles((prev) => ({ ...prev, [name]: file }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
    setServerError("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const validation = validate(values, files);
    setErrors(validation);
    if (Object.keys(validation).length > 0) return;

    setLoading(true);
    try {
      await enviarSolicitudOrganizacion(values, files);
      setSent(true);
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (err) {
      if (err.status === 409) setErrors({ email: err.message });
      else setServerError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="registro-page">
      <PawIcon className="registro-page__deco registro-page__deco--tl" />
      <PawIcon className="registro-page__deco registro-page__deco--tr" />
      <PawIcon className="registro-page__deco registro-page__deco--bl1" />
      <PawIcon className="registro-page__deco registro-page__deco--bl2" />
      <PawIcon className="registro-page__deco registro-page__deco--br" />

      <section className="registro-card">
        {sent ? (
          <div className="registro-success" role="status">
            <span className="registro-success__icon">
              <CheckCircleIcon />
            </span>
            <h1>¡Solicitud enviada!</h1>
            <p>
              Recibimos la información de <strong>{values.nombre}</strong>. Nuestro equipo
              revisará tus documentos y te avisaremos al correo{" "}
              <strong>{values.email}</strong> cuando tu cuenta sea aprobada.
            </p>
            <p className="registro-success__note">
              Hasta entonces no podrás iniciar sesión.
            </p>
            <BaseButton to="/login" size="lg">
              Volver al inicio de sesión
            </BaseButton>
          </div>
        ) : (
          <>
            <header className="registro-card__header">
              <h1>
                <PawIcon className="registro-card__paw" />
                Solicitud de Registro de Organización
              </h1>
              <p>Completa la información de tu organización para comenzar.</p>
            </header>

            <form className="registro-form" onSubmit={handleSubmit} noValidate>
              {/* ---------- Información básica ---------- */}
              <h2 className="registro-section__title">
                <span className="registro-section__icon">
                  <BuildingIcon />
                </span>
                Información básica
              </h2>

              <div className="registro-grid registro-grid--2">
                <BaseInput
                  label="Nombre de la Organización"
                  name="nombre"
                  required
                  maxLength={120}
                  autoComplete="organization"
                  value={values.nombre}
                  onChange={handleChange}
                  error={errors.nombre}
                />
                <BaseInput
                  label="Correo Electrónico"
                  name="email"
                  type="email"
                  required
                  maxLength={120}
                  autoComplete="email"
                  value={values.email}
                  onChange={handleChange}
                  error={errors.email}
                />
              </div>

              <div className="registro-grid registro-grid--3">
                <BaseSelect
                  label="Tipo de Organización"
                  name="tipoOrganizacion"
                  required
                  options={TIPOS_ORGANIZACION}
                  placeholder=""
                  value={values.tipoOrganizacion}
                  onChange={handleChange}
                  error={errors.tipoOrganizacion}
                />
                <BaseInput
                  label="Teléfono"
                  name="telefono"
                  type="tel"
                  required
                  maxLength={20}
                  autoComplete="tel"
                  value={values.telefono}
                  onChange={handleChange}
                  error={errors.telefono}
                />
                <BaseInput
                  label="Dirección"
                  name="direccion"
                  required
                  maxLength={200}
                  autoComplete="street-address"
                  value={values.direccion}
                  onChange={handleChange}
                  error={errors.direccion}
                />
              </div>

              {/* ---------- Documentos ---------- */}
              <h2 className="registro-section__title">
                <span className="registro-section__icon">
                  <FileTextIcon width={18} height={18} />
                </span>
                Documentos
              </h2>

              <div className="registro-docs">
                <FileUpload
                  label="Comprobante existencia legal"
                  required
                  value={files.existenciaLegal}
                  onChange={handleFile("existenciaLegal")}
                  error={errors.existenciaLegal}
                />
                <FileUpload
                  label="Comprobante de domicilio"
                  required
                  value={files.comprobanteDomicilio}
                  onChange={handleFile("comprobanteDomicilio")}
                  error={errors.comprobanteDomicilio}
                />
                <FileUpload
                  label="Identificación de responsable"
                  required
                  value={files.identificacionResponsable}
                  onChange={handleFile("identificacionResponsable")}
                  error={errors.identificacionResponsable}
                />
              </div>

              {serverError && (
                <p className="login-form__error" role="alert">
                  {serverError}
                </p>
              )}

              {/* ---------- Pie ---------- */}
              <div className="registro-footer">
                <BaseCheckbox
                  name="aceptaTerminos"
                  checked={values.aceptaTerminos}
                  onChange={handleChange}
                >
                  Aceptar{" "}
                  {/* TODO: ruta real de términos */}
                  <Link to="/terminos" target="_blank" className="auth-link">
                    términos y condiciones
                  </Link>
                  .
                </BaseCheckbox>

                <BaseButton
                  type="submit"
                  size="md"
                  loading={loading}
                  disabled={!values.aceptaTerminos}
                  className="registro-form__submit"
                >
                  Enviar Solicitud
                </BaseButton>
              </div>
            </form>
          </>
        )}
      </section>
    </main>
  );
}