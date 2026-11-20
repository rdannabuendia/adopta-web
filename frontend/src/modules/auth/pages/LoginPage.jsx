import { useLocation } from "react-router-dom";
import AuthLayout from "../components/AuthLayout";
import LoginForm from "../components/LoginForm";

export default function LoginPage() {
  const { state } = useLocation();

  return (
    <AuthLayout>
      <header className="login-page__header">
        <h1>Bienvenido de nuevo</h1>
        <p>Ingresa tus credenciales para acceder al sistema.</p>
      </header>

      {state?.mensaje && (
        <p className="login-form__success" role="status">
          {state.mensaje}
        </p>
      )}

      <LoginForm />
    </AuthLayout>
  );
}