import { apiFetch, buildQuery, postJson, sendJson } from "../../../services/api";

const BASE = "/api/administradores";

export const listarAdministradores = (filtros) => apiFetch(`${BASE}${buildQuery(filtros)}`);
export const obtenerAdministrador = (id) => apiFetch(`${BASE}/${id}`);
export const crearAdministrador = (body) => postJson(BASE, body);
export const actualizarAdministrador = (id, body) => sendJson("PUT", `${BASE}/${id}`, body);
export const cambiarEstadoAdministrador = (id, estado) =>
  sendJson("PATCH", `${BASE}/${id}/estado`, { estado });
export const reenviarCredenciales = (id) => postJson(`${BASE}/${id}/reenviar-credenciales`);
