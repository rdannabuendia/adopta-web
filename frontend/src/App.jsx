import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import LandingPage from "./modules/landing/pages/LandingPage";
import LoginPage from "./modules/auth/pages/LoginPage";
import ForgotPasswordPage from "./modules/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "./modules/auth/pages/ResetPasswordPage";
import RegistroOrganizacionPage from "./modules/auth/pages/RegistroOrganizacionPage";
import AppLayout from "./components/layout/AppLayout";
import ProtectedRoute from "./modules/auth/components/ProtectedRoute";
import DashboardPage from "./modules/dashboard/pages/DashboardPage";
import PerfilPage from "./modules/auth/pages/PerfilPage";
import EditarPerfilPage from "./modules/auth/pages/EditarPerfilPage";
import CambioPasswordInicialPage from "./modules/auth/pages/CambioPasswordInicialPage";
import AdministradoresPage from "./modules/administradores/pages/AdministradoresPage";
import AdministradorFormPage from "./modules/administradores/pages/AdministradorFormPage";
import OrganizacionesPage from "./modules/organizaciones/pages/OrganizacionesPage";
import OrganizacionDetallePage from "./modules/organizaciones/pages/OrganizacionDetallePage";
import OrganizacionBajaPage from "./modules/organizaciones/pages/OrganizacionBajaPage";
import SolicitudesRegistroPage from "./modules/organizaciones/pages/SolicitudesRegistroPage";
import SolicitudDetallePage from "./modules/organizaciones/pages/SolicitudDetallePage";
import MascotasHomePage from "./modules/mascotas/pages/MascotasHomePage";
import MascotaFormPage from "./modules/mascotas/pages/MascotaFormPage";
import MascotaDetallePage from "./modules/mascotas/pages/MascotaDetallePage";
import MascotaEstatusPage from "./modules/mascotas/pages/MascotaEstatusPage";
import MascotasOrganizacionPage from "./modules/mascotas/pages/MascotasOrganizacionPage";
import SeguimientoListaPage from "./modules/seguimiento/pages/SeguimientoListaPage";
import SeguimientoDetallePage from "./modules/seguimiento/pages/SeguimientoDetallePage";
import SeguimientoActualizarPage from "./modules/seguimiento/pages/SeguimientoActualizarPage";
import DevolucionesListaPage from "./modules/seguimiento/pages/DevolucionesListaPage";
import DevolucionDetallePage from "./modules/seguimiento/pages/DevolucionDetallePage";
import PanelGeneralPage from "./modules/reportes/pages/PanelGeneralPage";
import ReporteAdopcionesPage from "./modules/reportes/pages/ReporteAdopcionesPage";
import ReporteSolicitudesPage from "./modules/reportes/pages/ReporteSolicitudesPage";
import ReporteMascotasPage from "./modules/reportes/pages/ReporteMascotasPage";
import ExportarReportePage from "./modules/reportes/pages/ExportarReportePage";
import SolicitudesAdopcionListaPage from "./modules/solicitudes/pages/SolicitudesAdopcionListaPage";
import SolicitudAdopcionDetallePage from "./modules/solicitudes/pages/SolicitudAdopcionDetallePage";
import SolicitudAdopcionResolverPage from "./modules/solicitudes/pages/SolicitudAdopcionResolverPage";
import SolicitudAdopcionInfoPage from "./modules/solicitudes/pages/SolicitudAdopcionInfoPage";

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
          {/* Contraseña temporal: pantalla sin menú; ProtectedRoute obliga a pasar por aquí */}
          <Route path="/primer-cambio-password" element={<CambioPasswordInicialPage />} />

          <Route element={<AppLayout />}>
            <Route path="/inicio" element={<DashboardPage />} />
            <Route path="/perfil" element={<PerfilPage />} />
            <Route path="/perfil/editar" element={<EditarPerfilPage />} />

            {/* Solo administrador */}
            <Route element={<ProtectedRoute roles={ADMIN} />}>
              <Route path="/reportes" element={<PanelGeneralPage />} />
              <Route path="/reportes/adopciones" element={<ReporteAdopcionesPage />} />
              <Route path="/reportes/solicitudes" element={<ReporteSolicitudesPage />} />
              <Route path="/reportes/mascotas" element={<ReporteMascotasPage />} />
              <Route path="/reportes/exportar" element={<ExportarReportePage />} />

              <Route path="/administradores" element={<AdministradoresPage />} />
              <Route path="/administradores/nuevo" element={<AdministradorFormPage />} />
              <Route path="/administradores/:id/editar" element={<AdministradorFormPage />} />

              <Route path="/organizaciones" element={<OrganizacionesPage />} />
              <Route
                path="/organizaciones/solicitudes"
                element={<SolicitudesRegistroPage />}
              />
              <Route
                path="/organizaciones/solicitudes/:id"
                element={<SolicitudDetallePage />}
              />
              <Route
                path="/organizaciones/:id"
                element={<OrganizacionDetallePage />}
              />
              <Route
                path="/organizaciones/:id/baja"
                element={<OrganizacionBajaPage />}
              />
              <Route
                path="/mascotas/organizacion/:idOrganizacion"
                element={<MascotasOrganizacionPage />}
              />
            </Route>

            {/* Administrador y organización */}
            <Route element={<ProtectedRoute roles={ADMIN_ORG} />}>
              <Route path="/mascotas" element={<MascotasHomePage />} />
              <Route path="/mascotas/:id" element={<MascotaDetallePage />} />
            </Route>

            {/* Solo organización */}
            <Route element={<ProtectedRoute roles={ORG} />}>
              {/* Seguimiento de mascotas: solo organización */}
              <Route path="/seguimiento" element={<SeguimientoListaPage />} />
              <Route
                path="/seguimiento/devoluciones"
                element={<DevolucionesListaPage />}
              />
              <Route
                path="/seguimiento/devoluciones/:id"
                element={<DevolucionDetallePage />}
              />
              <Route path="/seguimiento/:id" element={<SeguimientoDetallePage />} />

              <Route path="/mascotas/nueva" element={<MascotaFormPage />} />
              <Route path="/mascotas/:id/editar" element={<MascotaFormPage />} />
              <Route
                path="/mascotas/:id/estatus"
                element={<MascotaEstatusPage />}
              />
              <Route
                path="/seguimiento/:id/actualizar"
                element={<SeguimientoActualizarPage />}
              />

              {/* Solicitudes de adopción: las gestiona la organización dueña de la mascota */}
              <Route path="/solicitudes" element={<SolicitudesAdopcionListaPage />} />
              <Route path="/solicitudes/:id" element={<SolicitudAdopcionDetallePage />} />
              <Route
                path="/solicitudes/:id/informacion"
                element={<SolicitudAdopcionInfoPage />}
              />
              <Route
                path="/solicitudes/:id/resolver"
                element={<SolicitudAdopcionResolverPage />}
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