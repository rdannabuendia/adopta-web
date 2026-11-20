import { BrowserRouter, Routes, Route } from "react-router-dom";
import LandingPage from "./modules/landing/pages/LandingPage";
import LoginPage from "./modules/auth/pages/LoginPage";
import ForgotPasswordPage from "./modules/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "./modules/auth/pages/ResetPasswordPage";
import RegistroOrganizacionPage from "./modules/auth/pages/RegistroOrganizacionPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/recuperar-password" element={<ForgotPasswordPage />} />
        <Route path="/cambiar-password" element={<ResetPasswordPage />} />
        <Route path="/registro" element={<RegistroOrganizacionPage />} />
      </Routes>
    </BrowserRouter>
  );
}
