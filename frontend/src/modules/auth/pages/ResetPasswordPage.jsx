import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import AuthLayout from "../components/AuthLayout";
import BaseButton from "../../../components/ui/BaseButton";
import PasswordInput from "../../../components/ui/PasswordInput";
import { resetPassword, validateResetToken } from "../services/authService";

const PASSWORD_REGEX = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;

function validate({ nuevaPassword, confirmarPassword }) {
  const errors = {};
  if (!nuevaPassword) errors.nuevaPassword = "Ingresa tu nueva contraseña.";
  else if (!PASSWORD_REGEX.test(nuevaPassword))
    errors.nuevaPassword = "Mínimo 8 caracteres, con al menos una letra y un número.";

  if (!confirmarPassword) errors.confirmarPassword = "Confirma tu contraseña.";
  else if (confirmarPassword !== nuevaPassword)
    errors.confirmarPassword = "Las contraseñas no coinciden.";
  return errors;
}

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get("token");

  const [status, setStatus] = useState("checking"); // checking | valid | invalid
  const [emailMask, setEmailMask] = useState("");
  const [values, setValues] = useState({ nuevaPassword: "", confirmarPassword: "" });
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  // Valida el enlace al abrir la página
  useEffect(() => {
    if (!token) {
      setStatus("invalid");
      return;
    }
    let cancelled = false;
    validateResetToken(token)
      .then((data) => {
        if (cancelled) return;
        setEmailMask(data.emailEnmascarado);
        setStatus("valid");
      })
      .catch(() => !cancelled && setStatus("invalid"));
    return () => {
      cancelled = true;
    };
  }, [token]);

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
      await resetPassword({ token, ...values });
      navigate("/login", {
        replace: true,
        state: { mensaje: "Tu contraseña se actualizó. Ya puedes iniciar sesión." },
      });
    } catch (err) {
      setServerError(err.message);
    } finally {
      setLoading(false);
    }
  };

  if (status === "checking") {
    return (
      <AuthLayout>
        <p className="login-page__notice">Verificando enlace…</p>
      </AuthLayout>
    );
  }

  if (status === "invalid") {
    return (
      <AuthLayout>
        <header className="login-page__header login-page__header--center">
          <h1>Enlace no válido</h1>
          <p>El enlace expiró o ya fue utilizado. Solicita uno nuevo.</p>
        </header>
        <BaseButton to="/recuperar-password" size="lg" fullWidth>
          Solicitar nuevo enlace
        </BaseButton>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout>
      <header className="login-page__header login-page__header--center">
        <h1>Cambiar Contraseña</h1>
        <p>Ingresa tu nueva contraseña.</p>
        <strong className="login-page__email-mask">{emailMask}</strong>
      </header>

      <form className="login-form" onSubmit={handleSubmit} noValidate>
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
          Guardar Contraseña
        </BaseButton>

        <p className="login-form__legal">
          {/* TODO: rutas reales */}
          <BaseButton to="/terminos" variant="link" size="xs">
            Términos y condiciones
          </BaseButton>
          {" / "}
          <BaseButton to="/privacidad" variant="link" size="xs">
            Políticas de privacidad
          </BaseButton>
        </p>
      </form>
    </AuthLayout>
  );
}