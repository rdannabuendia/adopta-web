import { apiFetch, buildQuery, postJson } from "../../../services/api";

const BASE = "/api/notificaciones";

export const listarNotificaciones = (f) => apiFetch(`${BASE}${buildQuery(f)}`);
export const contarNoLeidas = () => apiFetch(`${BASE}/contador`);
export const marcarLeida = (id) => postJson(`${BASE}/${id}/leer`);
export const marcarTodasLeidas = () => postJson(`${BASE}/leer-todas`);
