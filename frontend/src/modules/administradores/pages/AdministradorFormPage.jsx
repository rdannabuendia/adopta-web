import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "../../../components/ui/Alert";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import Card from "../../../components/ui/Card";
import PageHeader from "../../../components/layout/PageHeader";
import { useAsync } from "../../../hooks/useAsync";
import {
  actualizarAdministrador,
  crearAdministrador,
  obtenerAdministrador,
} from "../services/administradoresService";
import "../../../styles/Administradores.css";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const TELEFONO_REGEX = /^\+?[0-9\s\-()]{10,20}$/;
const VACIO = { nombre: "", apellidoPaterno: "", apellidoMaterno: "", correo: "", telefono: "" };

function validate(v) {
  const e = {};
  if (!v.nombre.trim()) e.nombre = "Ingresa el nombre.";
  if (!v.apellidoPaterno.trim()) e.apellidoPaterno = "Ingresa el apellido paterno.";
  if (!v.correo.trim()) e.correo = "Ingresa el correo electrónico.";
  else if (!EMAIL_REGEX.test(v.correo.trim())) e.correo = "El correo no es válido.";
  if (v.telefono.trim() && !TELEFONO_REGEX.test(v.telefono.trim())) e.telefono = "Teléfono no válido (10 a 20 dígitos).";
  return e;
}

export default function AdministradorFormPage() {
  const { id } = useParams();
  const editando = Boolean(id);
  const navigate = useNavigate();

  const { data: admin, error: loadError } = useAsync(
    () => (editando ? obtenerAdministrador(id) : Promise.resolve(null)),
    [id]
  );

  const [values, setValues] = useState(VACIO);
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!admin) return;
    setValues({
      nombre: admin.nombrePila ?? "",
      apellidoPaterno: admin.apellidoPaterno ?? "",
      apellidoMaterno: admin.apellidoMaterno ?? "",
      correo: admin.correo ?? "",
      telefono: admin.telefono ?? "",
    });
  }, [admin]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setValues((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
    setSubmitError("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const er = validate(values);
    setErrors(er);
    if (Object.keys(er).length) return;

    const body = {
      nombre: values.nombre.trim(),
      apellidoPaterno: values.apellidoPaterno.trim(),
      apellidoMaterno: values.apellidoMaterno.trim() || null,
      correo: values.correo.trim(),
      telefono: values.telefono.trim() || null,
    };

    setSaving(true);
    setSubmitError("");
    try {
      if (editando) {
        await actualizarAdministrador(id, body);
        navigate("/administradores", { state: { mensaje: "Los datos del administrador se actualizaron." } });
      } else {
        await crearAdministrador(body);
        navigate("/administradores", {
          state: { mensaje: `Administrador registrado. Enviamos sus credenciales temporales a ${body.correo}.` },
        });
      }
    } catch (err) {
      setSubmitError(err.message);
      setSaving(false);
    }
  };

  // En edición, el formulario aparece cuando ya cargaron los datos
  const mostrarForm = !loadError && (!editando || Boolean(admin));

  // Mientras no cambie su contraseña temporal, el correo es el destino de sus credenciales: no se edita
  const correoBloqueado = editando && Boolean(admin?.pendienteCambioPassword);

  return (
    <>
      <PageHeader
        breadcrumbs={[
          { label: "Inicio", to: "/inicio" },
          { label: "Administradores", to: "/administradores" },
          { label: editando ? "Editar" : "Registrar" },
        ]}
        title={editando ? "Editar administrador" : "Registrar administrador"}
        description={
          editando
            ? "Actualiza los datos de contacto del administrador."
            : "Captura sus datos y correo: la contraseña se genera sola."
        }
      />

      {loadError && <Alert variant="error" className="admin-alert">{loadError}</Alert>}
      {submitError && <Alert variant="error" className="admin-alert">{submitError}</Alert>}
      {!editando && (
        <Alert variant="info" className="admin-alert">
          Se generará una contraseña temporal y se enviará al correo indicado. En su primer inicio de sesión
          tendrá que cambiarla antes de usar la plataforma.
        </Alert>
      )}

      {mostrarForm && (
        <form onSubmit={handleSubmit} noValidate>
          <Card padded>
            <h2 className="admin-form__title">Información del administrador</h2>

            <div className="admin-form__grid">
              <BaseInput
                label="Nombre(s)"
                name="nombre"
                maxLength={80}
                autoComplete="off"
                value={values.nombre}
                onChange={handleChange}
                error={errors.nombre}
                required
              />
              <BaseInput
                label="Apellido paterno"
                name="apellidoPaterno"
                maxLength={60}
                autoComplete="off"
                value={values.apellidoPaterno}
                onChange={handleChange}
                error={errors.apellidoPaterno}
                required
              />
              <BaseInput
                label="Apellido materno"
                name="apellidoMaterno"
                maxLength={60}
                autoComplete="off"
                value={values.apellidoMaterno}
                onChange={handleChange}
                error={errors.apellidoMaterno}
              />
              <BaseInput
                label="Teléfono"
                name="telefono"
                type="tel"
                maxLength={20}
                autoComplete="off"
                value={values.telefono}
                onChange={handleChange}
                error={errors.telefono}
              />
              <BaseInput
                label="Correo electrónico"
                name="correo"
                type="email"
                maxLength={120}
                autoComplete="off"
                value={values.correo}
                onChange={handleChange}
                error={errors.correo}
                readOnly={correoBloqueado}
                hint={correoBloqueado ? "No se puede cambiar hasta que defina su contraseña." : undefined}
                required
              />
            </div>

            <div className="admin-form__actions">
              <BaseButton type="submit" loading={saving}>
                {editando ? "Guardar cambios" : "Registrar y enviar credenciales"}
              </BaseButton>
              <BaseButton variant="secondary" to="/administradores" disabled={saving}>
                Cancelar
              </BaseButton>
            </div>
          </Card>
        </form>
      )}
    </>
  );
}
