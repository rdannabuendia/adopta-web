import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import AvatarUpload from "../../../components/ui/AvatarUpload";
import BackLink from "../../../components/ui/BackLink";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import BaseSelect from "../../../components/ui/BaseSelect";
import BaseTextarea from "../../../components/ui/BaseTextarea";
import Breadcrumbs from "../../../components/ui/Breadcrumbs";
import Card from "../../../components/ui/Card";
import EmptyState from "../../../components/ui/EmptyState";
import { TIPOS_ORGANIZACION } from "../../../constants/organizacion";
import { mediaUrl } from "../../../utils/media";
import { updateStoredUser } from "../../auth/services/authService";
import { actualizarPerfil, obtenerPerfil } from "../services/perfilService";
import "../../../styles/Perfil.css";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_REGEX = /^\+?[0-9\s\-()]{10,20}$/;
const CP_REGEX = /^\d{5}$/;
const URL_REGEX = /^https?:\/\/\S+$/i;

function toValues(perfil) {
  const o = perfil.organizacion;
  if (o) {
    return {
      nombre: o.nombre ?? "",
      tipoOrganizacion: o.tipoOrganizacion ?? "",
      descripcion: o.descripcion ?? "",
      direccion: o.direccion ?? "",
      ciudad: o.ciudad ?? "",
      codigoPostal: o.codigoPostal ?? "",
      telefono: o.telefono ?? "",
      correoContacto: o.correoContacto ?? "",
      sitioWeb: o.sitioWeb ?? "",
    };
  }
  return {
    nombre: perfil.nombre ?? "",
    apellidoPaterno: perfil.apellidoPaterno ?? "",
    apellidoMaterno: perfil.apellidoMaterno ?? "",
    telefono: perfil.telefono ?? "",
  };
}

function validate(esOrg, v) {
  const e = {};
  if (!v.nombre.trim()) {
    e.nombre = esOrg ? "Ingresa el nombre de la organización." : "Ingresa tu nombre.";
  }

  if (esOrg) {
    if (!v.tipoOrganizacion) e.tipoOrganizacion = "Selecciona el tipo.";
    if (!v.direccion.trim()) e.direccion = "Ingresa la dirección.";
    if (!v.ciudad.trim()) e.ciudad = "Ingresa la ciudad.";
    if (v.codigoPostal.trim() && !CP_REGEX.test(v.codigoPostal.trim())) {
      e.codigoPostal = "Deben ser 5 dígitos.";
    }
    if (!v.telefono.trim()) {
      e.telefono = "Ingresa el teléfono.";
    } else if (!PHONE_REGEX.test(v.telefono.trim())) {
      e.telefono = "Teléfono no válido.";
    }
    if (v.correoContacto.trim() && !EMAIL_REGEX.test(v.correoContacto.trim())) {
      e.correoContacto = "El correo no es válido.";
    }
    if (v.sitioWeb.trim() && !URL_REGEX.test(v.sitioWeb.trim())) {
      e.sitioWeb = "Debe iniciar con http:// o https://";
    }
  } else {
    if (!v.apellidoPaterno.trim()) {
      e.apellidoPaterno = "Ingresa tu apellido paterno.";
    }
    if (v.telefono.trim() && !PHONE_REGEX.test(v.telefono.trim())) {
      e.telefono = "Teléfono no válido.";
    }
  }
  return e;
}

export default function EditarPerfilPage() {
  const navigate = useNavigate();
  const [perfil, setPerfil] = useState(null);
  const [values, setValues] = useState({});
  const [errors, setErrors] = useState({});
  const [foto, setFoto] = useState(null);
  const [eliminarFoto, setEliminarFoto] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [serverError, setServerError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;
    obtenerPerfil()
      .then((data) => {
        if (cancelled) return;
        setPerfil(data);
        setValues(toValues(data));
      })
      .catch((err) => !cancelled && setLoadError(err.message));
    return () => {
      cancelled = true;
    };
  }, []);

  const crumbs = [
    { label: "Inicio", to: "/inicio" },
    { label: "Ver perfil", to: "/perfil" },
    { label: "Editar perfil" },
  ];

  if (loadError) {
    return (
      <div className="perfil-page">
        <Breadcrumbs items={crumbs} className="perfil-crumbs" />
        <Alert variant="error">{loadError}</Alert>
      </div>
    );
  }

  if (!perfil) {
    return (
      <div className="perfil-page">
        <Breadcrumbs items={crumbs} className="perfil-crumbs" />
        <EmptyState title="Cargando perfil…" />
      </div>
    );
  }

  const esOrg = Boolean(perfil.organizacion);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setValues((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
    setServerError("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const validation = validate(esOrg, values);
    setErrors(validation);
    if (Object.keys(validation).length > 0) return;

    setSaving(true);
    setServerError("");
    try {
      const actualizado = await actualizarPerfil(esOrg, values, { foto, eliminarFoto });
      updateStoredUser({
        nombre: actualizado.nombreCompleto,
        fotoUrl: actualizado.fotoUrl,
      });
      navigate("/perfil", {
        replace: true,
        state: { mensaje: "Tus cambios se guardaron correctamente." },
      });
    } catch (err) {
      setServerError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="perfil-page">
      <Breadcrumbs items={crumbs} className="perfil-crumbs" />

      <Card padded className="perfil-form-card">
        <div className="perfil-form__topbar">
          <BackLink to="/perfil" />
          <span className="perfil-form__role-tag">
            {esOrg ? "Cuenta de Organización" : `Cuenta de ${perfil.rol}`}
          </span>
        </div>

        <div className="perfil-form__header">
          <h1 className="perfil-form__title">Editar perfil</h1>
          <p className="perfil-form__subtitle">
            {esOrg
              ? "Actualiza la información pública, ubicación y medios de contacto de tu organización."
              : "Mantén actualizados tus datos personales y de contacto."}
          </p>
        </div>

        <form className="perfil-form" onSubmit={handleSubmit} noValidate>
          <div className="perfil-form__body">
            <aside className="perfil-form__aside">
              <AvatarUpload
                name={perfil.nombreCompleto}
                currentUrl={eliminarFoto ? null : mediaUrl(perfil.fotoUrl)}
                file={foto}
                onChange={(file) => {
                  setFoto(file);
                  setEliminarFoto(false);
                }}
                onRemove={() => {
                  setFoto(null);
                  setEliminarFoto(true);
                }}
                disabled={saving}
              />
              <p className="perfil-form__aside-note">
                JPG o PNG. Una imagen clara ayuda a identificar tu cuenta en el sistema.
              </p>
            </aside>

            <div className="perfil-form__fields">
              <div className="perfil-form__section-group">
                <h2 className="perfil-form__section">
                  {esOrg ? "Datos de la organización" : "Datos personales"}
                </h2>

                <BaseInput
                  label={esOrg ? "Nombre de la organización" : "Nombre"}
                  name="nombre"
                  required
                  maxLength={esOrg ? 120 : 80}
                  value={values.nombre}
                  onChange={handleChange}
                  error={errors.nombre}
                />

                {!esOrg && (
                  <div className="perfil-form__row">
                    <BaseInput
                      label="Apellido paterno"
                      name="apellidoPaterno"
                      required
                      maxLength={60}
                      value={values.apellidoPaterno}
                      onChange={handleChange}
                      error={errors.apellidoPaterno}
                    />
                    <BaseInput
                      label="Apellido materno"
                      name="apellidoMaterno"
                      maxLength={60}
                      value={values.apellidoMaterno}
                      onChange={handleChange}
                    />
                  </div>
                )}

                {esOrg && (
                  <>
                    <BaseSelect
                      label="Tipo de organización"
                      name="tipoOrganizacion"
                      required
                      options={TIPOS_ORGANIZACION}
                      placeholder="Selecciona una opción…"
                      value={values.tipoOrganizacion}
                      onChange={handleChange}
                      error={errors.tipoOrganizacion}
                    />

                    <BaseTextarea
                      label="Descripción"
                      name="descripcion"
                      rows={4}
                      maxLength={1000}
                      showCount
                      placeholder="Cuéntales a los adoptantes quiénes son y cómo trabajan."
                      value={values.descripcion}
                      onChange={handleChange}
                    />
                  </>
                )}

                <BaseInput
                  label="Correo de acceso"
                  name="correo"
                  value={perfil.correo}
                  disabled
                  hint="Es el correo con el que inicias sesión y no se puede cambiar aquí."
                />
              </div>

              {esOrg && (
                <div className="perfil-form__section-group">
                  <h2 className="perfil-form__section">Ubicación</h2>

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

                  <div className="perfil-form__row">
                    <BaseInput
                      label="Ciudad"
                      name="ciudad"
                      required
                      maxLength={80}
                      value={values.ciudad}
                      onChange={handleChange}
                      error={errors.ciudad}
                    />
                    <BaseInput
                      label="Código postal"
                      name="codigoPostal"
                      inputMode="numeric"
                      maxLength={5}
                      value={values.codigoPostal}
                      onChange={handleChange}
                      error={errors.codigoPostal}
                    />
                  </div>
                </div>
              )}

              <div className="perfil-form__section-group">
                <h2 className="perfil-form__section">Contacto</h2>

                <div className={esOrg ? "perfil-form__row" : undefined}>
                  <BaseInput
                    label="Teléfono"
                    name="telefono"
                    type="tel"
                    required={esOrg}
                    maxLength={20}
                    autoComplete="tel"
                    value={values.telefono}
                    onChange={handleChange}
                    error={errors.telefono}
                  />

                  {esOrg && (
                    <BaseInput
                      label="Correo de contacto"
                      name="correoContacto"
                      type="email"
                      maxLength={120}
                      hint="Opcional. Correo público para que te contacten."
                      value={values.correoContacto}
                      onChange={handleChange}
                      error={errors.correoContacto}
                    />
                  )}
                </div>

                {esOrg && (
                  <BaseInput
                    label="Sitio web"
                    name="sitioWeb"
                    type="url"
                    maxLength={255}
                    placeholder="https://"
                    value={values.sitioWeb}
                    onChange={handleChange}
                    error={errors.sitioWeb}
                  />
                )}
              </div>

              {serverError && <Alert variant="error">{serverError}</Alert>}
            </div>
          </div>

          <div className="perfil-form__footer">
            <BaseButton
              variant="secondary"
              onClick={() => navigate("/perfil")}
              disabled={saving}
            >
              Cancelar
            </BaseButton>
            <BaseButton type="submit" loading={saving}>
              Guardar Cambios
            </BaseButton>
          </div>
        </form>
      </Card>
    </div>
  );
}