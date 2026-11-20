const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";
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
    throw new Error("No se pudo conectar con el servidor. Intenta de nuevo.");
  }

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    throw new Error(data.mensaje || "Ocurrió un error. Intenta de nuevo.");
  }

  return data;
}

/* ---------- Sesión ---------- */
export async function login({ email, password }) {
  const data = await post("login", { email, password });
  localStorage.setItem(TOKEN_KEY, data.token);
  localStorage.setItem(USER_KEY, JSON.stringify(data.usuario));
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

export const isAuthenticated = () => Boolean(getToken());

export const forgotPassword = (email) => post("forgot-password", { email });

export const validateResetToken = (token) => post("validate-reset-token", { token });

export const resetPassword = ({ token, nuevaPassword, confirmarPassword }) =>
  post("reset-password", { token, nuevaPassword, confirmarPassword });