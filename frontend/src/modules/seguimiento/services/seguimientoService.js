import { apiFetch, buildQuery, postJson } from "../../../services/api";

const BASE = "/api/seguimiento";

// Adopciones en seguimiento (el id de las rutas es el id de la adopción)
export const listarSeguimientos = (f) => apiFetch(`${BASE}${buildQuery(f)}`);
export const obtenerSeguimiento = (idAdopcion) => apiFetch(`${BASE}/${idAdopcion}`);
export const listarEstadosAdaptacion = () => apiFetch(`${BASE}/estados`);
export const actualizarAdaptacion = (idAdopcion, body) => postJson(`${BASE}/${idAdopcion}/adaptacion`, body);

// Solicitudes de devolución
export const listarDevoluciones = (f) => apiFetch(`${BASE}/devoluciones${buildQuery(f)}`);
export const obtenerDevolucion = (id) => apiFetch(`${BASE}/devoluciones/${id}`);
export const aceptarDevolucion = (id, body) => postJson(`${BASE}/devoluciones/${id}/aceptar`, body);
