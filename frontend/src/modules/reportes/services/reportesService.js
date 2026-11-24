import { apiFetch, buildQuery, downloadFile } from "../../../services/api";

const BASE = "/api/reportes";

export const obtenerPanel = () => apiFetch(`${BASE}/panel`);
export const obtenerReporteSolicitudes = () => apiFetch(`${BASE}/solicitudes`);
export const obtenerReporteMascotas = () => apiFetch(`${BASE}/mascotas`);
export const obtenerReporteAdopciones = () => apiFetch(`${BASE}/adopciones`);

/** tipo: general | adopciones | solicitudes | mascotas · formato: pdf | excel */
export const exportarReporte = ({ tipo, formato, desde, hasta, nombre }, nombreArchivo) =>
  downloadFile(`${BASE}/exportar${buildQuery({ tipo, formato, desde, hasta, nombre })}`, nombreArchivo);
