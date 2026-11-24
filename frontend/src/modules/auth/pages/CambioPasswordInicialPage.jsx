import { useState } from "react";
import { useNavigate } from "react-router-dom";
import AuthLayout from "../components/AuthLayout";
import BaseButton from "../../../components/ui/BaseButton";
import PasswordInput from "../../../components/ui/PasswordInput";
import { changeInitialPassword, getUser, logout } from "../services/authService";

const PASSWORD_REGEX = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;

function validate({ passwordActual, nuevaPassword, confirmarPassword }) {
  const errors = {};
  if (!passwordActual) errors.passwordActual = "Ingresa la contraseña temporal que recibiste por correo.";

  if (!nuevaPassword) errors.nuevaPassword = "Ingresa tu nueva contraseña.";
  else if (!PASSWORD_REGEX.test(nuevaPassword))
    errors.nuevaPassword = "Mínimo 8 caracteres, con al menos una letra y un número.";
  else if (nuevaPassword === passwordActual)
    errors.nuevaPassword = "Debe ser distinta a la contraseña temporal.";

  if (!confirmarPassword) errors.confirmarPassword = "Confirma tu contraseña.";
  else if (confirmarPassword !== nuevaPassword)
    errors.confirmarPassword = "Las contraseñas no coinciden.";
  return errors;
}

/** Primer inicio de sesión de un administrador: obligatorio crear una contraseña propia. */
export default function CambioPasswordInicialPage() {
  const navigate = useNavigate();
  const user = getUser();
  const [values, setValues] = useState({ passwordActual: "", nuevaPassword: "", confirmarPassword: "" });
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setValues((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
    setServerError("");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const validation = validate(values);
    setErrors(validation);
    if (Object.keys(validation).length > 0) return;

    setLoading(true);
    try {
      await changeInitialPassword(values);
      navigate("/inicio", {
        replace: true,
        state: { mensaje: "Tu contraseña se actualizó. ¡Bienvenido a Adopta+!" },
      });
    } catch (err) {
      setServerError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <AuthLayout>
      <header className="login-page__header login-page__header--center">
        <h1>Crea tu contraseña</h1>
        <p>
          Por seguridad, debes reemplazar la contraseña temporal que recibiste por correo antes de
          usar la plataforma.
        </p>
        {user?.email && <strong className="login-page__email-mask">{user.email}</strong>}
      </header>

      <form className="login-form" onSubmit={handleSubmit} noValidate>
        <PasswordInput
          label="Contraseña temporal"
          name="passwordActual"
          autoComplete="current-password"
          value={values.passwordActual}
          onChange={handleChange}
          error={errors.passwordActual}
        />

        <PasswordInput
          label="Nueva contraseña"
          name="nuevaPassword"
          autoComplete="new-password"
          value={values.nuevaPassword}
          onChange={handleChange}
          error={errors.nuevaPassword}
        />

        <PasswordInput
          label="Confirmar contraseña"
          name="confirmarPassword"
          autoComplete="new-password"
          value={values.confirmarPassword}
          onChange={handleChange}
          error={errors.confirmarPassword}
        />

        {serverError && (
          <p className="login-form__error" role="alert">
            {serverError}
          </p>
        )}

        <BaseButton type="submit" size="lg" fullWidth loading={loading}>
          Guardar contraseña
        </BaseButton>

        <p className="login-form__legal">
          <BaseButton variant="link" size="xs" onClick={handleLogout}>
            Cerrar sesión
          </BaseButton>
        </p>
      </form>
    </AuthLayout>
  );
}
