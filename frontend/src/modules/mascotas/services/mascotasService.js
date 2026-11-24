import { apiFetch, buildQuery, postJson } from "../../../services/api";

const BASE = "/api/mascotas";

// Admin
export const listarOrganizacionesConMascotas = (f) => apiFetch(`${BASE}/organizaciones${buildQuery(f)}`);
export const listarMascotasDeOrganizacion = (idOrg, f) => apiFetch(`${BASE}/organizacion/${idOrg}${buildQuery(f)}`);

// Organización
export const listarMisMascotas = (f) => apiFetch(`${BASE}/mias${buildQuery(f)}`);
export const crearMascota = (formData) => apiFetch(BASE, { method: "POST", body: formData });
export const actualizarMascota = (id, formData) => apiFetch(`${BASE}/${id}`, { method: "PUT", body: formData });
export const cambiarEstadoMascota = (id, body) => postJson(`${BASE}/${id}/estado`, body);

// Ambos
export const obtenerMascota = (id) => apiFetch(`${BASE}/${id}`);

/**
 * values: campos del formulario · photos: { items, principalKey } del PhotoPicker
 * fotosOriginales: ids de fotos que tenía la mascota al abrir la edición
 */
export function buildMascotaFormData(values, { items, principalKey }, fotosOriginales = []) {
  const fd = new FormData();

  ["nombre", "especie", "raza", "sexo", "tamano", "edadAproximada", "color", "peso", "descripcion", "estadoSalud"]
    .forEach((c) => fd.append(c, String(values[c] ?? "").trim()));
  fd.append("vacunado", values.vacunado ? "true" : "false");
  fd.append("esterilizado", values.esterilizado ? "true" : "false");
  values.caracteristicas.forEach((c) => fd.append("caracteristicas", c));

  const nuevas = items.filter((i) => i.file);
  nuevas.forEach((i) => fd.append("fotos", i.file));

  const idsActuales = items.filter((i) => i.id).map((i) => i.id);
  fotosOriginales.filter((id) => !idsActuales.includes(id)).forEach((id) => fd.append("fotosEliminar", id));

  const principal = items.find((i) => i.key === principalKey);
  if (principal?.file) fd.append("indicePrincipalNueva", nuevas.indexOf(principal));
  else if (principal?.id) fd.append("idFotoPrincipal", principal.id);

  return fd;
}