import { getToken, logout, updateStoredUser } from "../modules/auth/services/authService";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export function buildQuery(params = {}) {
  const qs = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && String(v).trim() !== "") qs.append(k, v);
  });
  const s = qs.toString();
  return s ? `?${s}` : "";
}

export async function apiFetch(path, options = {}) {
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

  // La API solo permite cambiar la contraseña mientras se use la temporal
  if (response.status === 403 && data.codigo === "CAMBIO_PASSWORD_REQUERIDO") {
    updateStoredUser({ debeCambiarPassword: true });
    window.location.assign("/primer-cambio-password");
    throw new Error(data.mensaje);
  }

  if (!response.ok) throw new Error(data.mensaje || "No se pudo completar la solicitud.");
  return data;
}

export const postJson = (path, body) =>
  apiFetch(path, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

export const sendJson = (method, path, body) =>
  apiFetch(path, {
    method,
    headers: { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

export async function openProtectedFile(path) {
  const win = window.open("", "_blank"); 
  try {
    const response = await fetch(`${API_URL}${path}`, { headers: { Authorization: `Bearer ${getToken()}` } });
    if (!response.ok) {
      const data = await response.json().catch(() => ({}));
      throw new Error(data.mensaje || "No se pudo abrir el documento.");
    }
    const blob = await response.blob();
    if (win) win.location.href = URL.createObjectURL(blob);
  } catch (err) {
    win?.close();
    throw err;
  }
}

/** Descarga un archivo protegido (PDF, Excel…) con el token de sesión y lo guarda con `filename`. */
export async function downloadFile(path, filename) {
  let response;
  try {
    response = await fetch(`${API_URL}${path}`, { headers: { Authorization: `Bearer ${getToken()}` } });
  } catch {
    throw new Error("No se pudo conectar con el servidor. Intenta de nuevo.");
  }

  if (response.status === 401) {
    logout();
    window.location.assign("/login");
    throw new Error("Tu sesión expiró. Inicia sesión de nuevo.");
  }
  if (!response.ok) {
    const data = await response.json().catch(() => ({}));
    throw new Error(data.mensaje || "No se pudo generar el archivo.");
  }

  const url = URL.createObjectURL(await response.blob());
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 10_000);
}
