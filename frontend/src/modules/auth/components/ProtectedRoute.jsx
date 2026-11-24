import { Navigate, Outlet, useLocation } from "react-router-dom";
import { getUser, isAuthenticated, logout, mustChangePassword } from "../services/authService";

const WEB_ROLES = ["Administrador", "Organizacion"];
export const CAMBIO_PASSWORD_PATH = "/primer-cambio-password";

export default function ProtectedRoute({ roles }) {
  const location = useLocation();

  if (!isAuthenticated()) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  const rol = getUser()?.rol;

  if (!WEB_ROLES.includes(rol)) {
    logout(); 
    return <Navigate to="/login" replace />;
  }

  // Contraseña temporal (administrador recién registrado): no puede usar nada más hasta cambiarla
  const enCambioPassword = location.pathname === CAMBIO_PASSWORD_PATH;
  if (mustChangePassword() && !enCambioPassword) {
    return <Navigate to={CAMBIO_PASSWORD_PATH} replace />;
  }
  if (!mustChangePassword() && enCambioPassword) {
    return <Navigate to="/inicio" replace />;
  }

  if (roles && !roles.includes(rol)) {
    return <Navigate to="/inicio" replace />;
  }

  return <Outlet />;
}