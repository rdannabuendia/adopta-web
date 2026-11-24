import { getToken, logout } from "../../auth/services/authService";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

const CAMPOS_ORGANIZACION = [
  "nombre", "tipoOrganizacion", "descripcion", "direccion", "ciudad",
  "codigoPostal", "telefono", "correoContacto", "sitioWeb",
];
const CAMPOS_USUARIO = ["nombre", "apellidoPaterno", "apellidoMaterno", "telefono"];

async function request(path, options = {}) {
  let response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      ...options,
      headers: { Authorization: `Bearer ${getToken()}`, ...options.headers },
    });
  } catch {
    throw new Error("No se pudo conectar con el servidor. Intenta de nuevo.");
  }

  if (response.status === 401) {
    logout();
    window.location.assign("/login");
    throw new Error("Tu sesión expiró. Inicia sesión de nuevo.");
  }

  const data = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(data.mensaje || "No se pudo completar la solicitud.");
  return data;
}

export const obtenerPerfil = () => request("/api/perfil");

export function actualizarPerfil(esOrganizacion, values, { foto, eliminarFoto }) {
  const body = new FormData();
  (esOrganizacion ? CAMPOS_ORGANIZACION : CAMPOS_USUARIO).forEach((campo) =>
    body.append(campo, (values[campo] ?? "").trim())
  );
  if (foto) body.append("foto", foto);
  body.append("eliminarFoto", String(Boolean(eliminarFoto)));

  return request("/api/perfil", { method: "PUT", body });
}