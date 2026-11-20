import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import AuthLayout from "../components/AuthLayout";
import BaseButton from "../../../components/ui/BaseButton";
import BaseInput from "../../../components/ui/BaseInput";
import { MailIcon } from "../../../components/ui/Icons";
import { forgotPassword } from "../services/authService";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const RESEND_SECONDS = 30;

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const [seconds, setSeconds] = useState(0);

  useEffect(() => {
    if (seconds <= 0) return;
    const t = setTimeout(() => setSeconds((s) => s - 1), 1000);
    return () => clearTimeout(t);
  }, [seconds]);

  const send = async () => {
    if (!email.trim()) return setError("Ingresa tu correo electrónico.");
    if (!EMAIL_REGEX.test(email)) return setError("El correo no es válido.");

    setLoading(true);
    setError("");
    try {
      await forgotPassword(email.trim());
      setSent(true);
      setSeconds(RESEND_SECONDS);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    send();
  };

  return (
    <AuthLayout>
      <header className="login-page__header">
        <h1>Reestablecer Contraseña</h1>
        <p>
          Ingresa el correo, se te enviará un enlace para reestablecer la nueva
          contraseña.
        </p>
      </header>

      <form className="login-form" onSubmit={handleSubmit} noValidate>
        <BaseInput
          label="Correo Electrónico"
          name="email"
          type="email"
          autoComplete="email"
          icon={<MailIcon />}
          value={email}
          onChange={(e) => {
            setEmail(e.target.value);
            setError("");
          }}
          error={error}
        />

        {sent && (
          <>
            <p className="login-form__success" role="status">
              Si el correo está registrado, recibirás un enlace para restablecer
              tu contraseña. Revisa también tu carpeta de spam.
            </p>
            <div className="login-form__resend">
              <BaseButton
                variant="link"
                size="xs"
                onClick={send}
                disabled={seconds > 0 || loading}
              >
                {seconds > 0
                  ? `Volver a enviar ${seconds} seg.`
                  : "Volver a enviar"}
              </BaseButton>
            </div>
          </>
        )}

        <BaseButton
          type="submit"
          size="lg"
          fullWidth
          loading={loading}
          disabled={sent}
        >
          Continuar
        </BaseButton>

        <div className="login-form__register login-form__center">
          <Link to="/login" className="login-form__link">
            Regresar al inicio de sesión
          </Link>
        </div>
      </form>
    </AuthLayout>
  );
}
