import { Navigate, Outlet, useLocation } from "react-router-dom";
import { getUser, isAuthenticated } from "../services/authService";

/** Sin sesión → /login. Con `roles`, además exige que el rol coincida. */
export default function ProtectedRoute({ roles }) {
  const location = useLocation();

  if (!isAuthenticated()) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  const user = getUser();
  if (roles && !roles.includes(user?.rol)) {
    return <Navigate to="/inicio" replace />;
  }

  return <Outlet />;
}