import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import LandingPage from "./modules/landing/pages/LandingPage";
import LoginPage from "./modules/auth/pages/LoginPage";
import ForgotPasswordPage from "./modules/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "./modules/auth/pages/ResetPasswordPage";
import RegistroOrganizacionPage from "./modules/auth/pages/RegistroOrganizacionPage";
import AppLayout from "./components/layout/AppLayout";
import PlaceholderPage from "./components/layout/PlaceholderPage";
import ProtectedRoute from "./modules/auth/components/ProtectedRoute";
import DashboardPage from "./modules/dashboard/pages/DashboardPage";

const ADMIN = ["Administrador"];
const ORG = ["Organizacion"];
const ADMIN_ORG = ["Administrador", "Organizacion"];

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Rutas públicas */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/recuperar-password" element={<ForgotPasswordPage />} />
        <Route path="/cambiar-password" element={<ResetPasswordPage />} />
        <Route path="/registro" element={<RegistroOrganizacionPage />} />

        {/* Rutas protegidas dentro del layout con sidebar */}
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/inicio" element={<DashboardPage />} />
            <Route
              path="/perfil"
              element={<PlaceholderPage title="Mi perfil" />}
            />

            {/* Solo administrador */}
            <Route element={<ProtectedRoute roles={ADMIN} />}>
              <Route
                path="/reportes"
                element={
                  <PlaceholderPage title="Panel de estadísticas generales" />
                }
              />
              <Route
                path="/reportes/adopciones"
                element={<PlaceholderPage title="Reporte de adopciones" />}
              />
              <Route
                path="/reportes/solicitudes"
                element={<PlaceholderPage title="Reporte de solicitudes" />}
              />
              <Route
                path="/reportes/mascotas"
                element={<PlaceholderPage title="Estadísticas de mascotas" />}
              />
              <Route
                path="/reportes/exportar"
                element={
                  <PlaceholderPage title="Generar y exportar reportes" />
                }
              />

              <Route
                path="/organizaciones"
                element={<PlaceholderPage title="Organizaciones" />}
              />
              <Route
                path="/organizaciones/solicitudes"
                element={<PlaceholderPage title="Solicitudes de registro" />}
              />
              <Route
                path="/organizaciones/:id"
                element={<PlaceholderPage title="Detalle de organización" />}
              />
            </Route>

            {/* Administrador y organización */}
            <Route element={<ProtectedRoute roles={ADMIN_ORG} />}>
              <Route
                path="/mascotas"
                element={<PlaceholderPage title="Mascotas" />}
              />
              <Route
                path="/mascotas/:id"
                element={<PlaceholderPage title="Detalle de mascota" />}
              />

              <Route
                path="/seguimiento"
                element={<PlaceholderPage title="Gestión de seguimiento" />}
              />
              <Route
                path="/seguimiento/devoluciones"
                element={<PlaceholderPage title="Solicitudes de devolución" />}
              />
              <Route
                path="/seguimiento/:id"
                element={<PlaceholderPage title="Detalle de seguimiento" />}
              />

              <Route
                path="/solicitudes"
                element={<PlaceholderPage title="Solicitudes de adopción" />}
              />
              <Route
                path="/solicitudes/:id"
                element={<PlaceholderPage title="Detalle de solicitud" />}
              />
            </Route>

            {/* Solo organización */}
            <Route element={<ProtectedRoute roles={ORG} />}>
              <Route
                path="/mascotas/nueva"
                element={<PlaceholderPage title="Registrar mascota" />}
              />
              <Route
                path="/mascotas/:id/editar"
                element={<PlaceholderPage title="Editar mascota" />}
              />
            </Route>
          </Route>
        </Route>

        {/* Cualquier otra ruta */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}