import { ROLES } from "../../../components/layout/navigation";
import { getUser } from "../../auth/services/authService";
import MisMascotasPage from "./MisMascotasPage";
import OrganizacionesMascotasPage from "./OrganizacionesMascotasPage";

export default function MascotasHomePage() {
  return getUser()?.rol === ROLES.ADMIN ? <OrganizacionesMascotasPage /> : <MisMascotasPage />;
}