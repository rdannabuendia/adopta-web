import { apiFetch, buildQuery, openProtectedFile, postJson,} from "../../../services/api";

const BASE = "/api/organizaciones";

export const listarOrganizaciones = (filtros) =>
  apiFetch(`${BASE}${buildQuery(filtros)}`);
export const obtenerCiudades = () => apiFetch(`${BASE}/ciudades`);
export const obtenerOrganizacion = (id) => apiFetch(`${BASE}/${id}`);

export const listarMotivosBaja = () => apiFetch(`${BASE}/motivos-baja`);
export const darDeBaja = (id, body) => postJson(`${BASE}/${id}/baja`, body);

export const listarSolicitudes = (filtros) =>
  apiFetch(`${BASE}/solicitudes${buildQuery(filtros)}`);
export const aprobarSolicitud = (id) => postJson(`${BASE}/${id}/aprobar`);
export const rechazarSolicitud = (id, motivo) =>
  postJson(`${BASE}/${id}/rechazar`, { motivo });

export const abrirDocumento = (idDocumento) =>
  openProtectedFile(`${BASE}/documentos/${idDocumento}`);
