import { apiFetch, buildQuery, openProtectedFile, postJson } from "../../../services/api";

const BASE = "/api/solicitudes";

export const listarSolicitudes = (f) => apiFetch(`${BASE}${buildQuery(f)}`);
export const listarEstadosSolicitud = () => apiFetch(`${BASE}/estados`);
export const obtenerSolicitud = (id) => apiFetch(`${BASE}/${id}`);

export const listarDocumentosSolicitud = (id) => apiFetch(`${BASE}/${id}/documentos`);
export const abrirArchivoSolicitud = (idArchivo) => openProtectedFile(`${BASE}/archivos/${idArchivo}`);

export const solicitarInformacion = (id, body) => postJson(`${BASE}/${id}/informacion-adicional`, body);
export const resolverSolicitud = (id, body) => postJson(`${BASE}/${id}/resolver`, body);
