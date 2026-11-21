import { Navigate, Outlet, useLocation } from "react-router-dom";
import { getUser, isAuthenticated, logout } from "../services/authService";

const WEB_ROLES = ["Administrador", "Organizacion"];

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

  if (roles && !roles.includes(rol)) {
    return <Navigate to="/inicio" replace />;
  }

  return <Outlet />;
}