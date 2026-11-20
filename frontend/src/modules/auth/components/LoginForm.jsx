import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import PasswordInput from "../../../components/ui/PasswordInput";
import { MailIcon } from "../../../components/ui/Icons";
import { login } from "../services/authService";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function validate({ email, password }) {
  const errors = {};
  if (!email.trim()) errors.email = "Ingresa tu correo electrónico.";
  else if (!EMAIL_REGEX.test(email)) errors.email = "El correo no es válido.";
  if (!password) errors.password = "Ingresa tu contraseña.";
  return errors;
}

export default function LoginForm() {
  const navigate = useNavigate();
  const [values, setValues] = useState({ email: "", password: "" });
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
      await login({ email: values.email.trim(), password: values.password });
      navigate("/inicio", { replace: true });
    } catch (err) {
      setServerError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form className="login-form" onSubmit={handleSubmit} noValidate>
      <BaseInput
        label="Correo Electrónico"
        name="email"
        type="email"
        autoComplete="email"
        icon={<MailIcon />}
        value={values.email}
        onChange={handleChange}
        error={errors.email}
      />

      <PasswordInput
        label="Contraseña"
        name="password"
        autoComplete="current-password"
        value={values.password}
        onChange={handleChange}
        error={errors.password}
      />

      <div className="login-form__forgot">
        <Link
          to="/recuperar-password"
          className="login-form__link login-form__link--xs"
        >
          ¿Olvidaste tu contraseña?
        </Link>
      </div>

      {serverError && (
        <p className="login-form__error" role="alert">
          {serverError}
        </p>
      )}

      <BaseButton type="submit" size="lg" fullWidth loading={loading}>
        Iniciar Sesión
      </BaseButton>

      <p className="login-form__register">
        ¿No tienes cuenta?{" "}
        <Link to="/registro" className="login-form__link">
          Solicite registrarse
        </Link>
      </p>
    </form>
  );
}