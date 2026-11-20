const RAW_API_URL = import.meta.env.VITE_API_URL || "";
const API_URL = RAW_API_URL.replace(/\/+$/, "");
const TOKEN_KEY = "adopta_token";
const USER_KEY = "adopta_user";

async function post(path, body) {
  let response;

  try {
    response = await fetch(`${API_URL}/api/auth/${path}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
  } catch {
    throw new Error("No se pudo conectar con el servidor. Verifica que la API esté encendida.");
  }

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    throw new Error(data.mensaje || data.message || "Ocurrió un error. Intenta de nuevo.");
  }

  return data;
}

/* ---------- Sesión ---------- */
export async function login({ email, password }) {
  const data = await post("login", { email, password });

  const token = data.token ?? data.accessToken;
  const usuario = data.usuario ?? data.user;

  if (!token || !usuario) {
    throw new Error("Respuesta inválida del servidor al iniciar sesión.");
  }

  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(usuario));
  return data;
}

export function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export const getToken = () => localStorage.getItem(TOKEN_KEY);

export function getUser() {
  try {
    return JSON.parse(localStorage.getItem(USER_KEY));
  } catch {
    return null;
  }
}

function tokenExpired(token) {
  try {
    const payload = JSON.parse(
      atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/"))
    );
    if (!payload.exp) return false;
    return payload.exp * 1000 <= Date.now();
  } catch {
    return true;
  }
}

export function isAuthenticated() {
  const token = getToken();
  if (!token) return false;
  if (tokenExpired(token)) {
    logout();
    return false;
  }
  return true;
}

export const forgotPassword = (email) => post("forgot-password", { email });

export const validateResetToken = (token) =>
  post("validate-reset-token", { token });

export const resetPassword = ({ token, nuevaPassword, confirmarPassword }) =>
  post("reset-password", { token, nuevaPassword, confirmarPassword });