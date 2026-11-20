const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

/**
 * POST /api/organizaciones/solicitud (multipart/form-data)
 * No fijes Content-Type: el navegador agrega el boundary solo.
 */
export async function enviarSolicitudOrganizacion(values, files) {
  const body = new FormData();
  body.append("nombre", values.nombre.trim());
  body.append("email", values.email.trim());
  body.append("tipoOrganizacion", values.tipoOrganizacion);
  body.append("telefono", values.telefono.trim());
  body.append("direccion", values.direccion.trim());
  body.append("aceptaTerminos", String(values.aceptaTerminos));
  body.append("existenciaLegal", files.existenciaLegal);
  body.append("comprobanteDomicilio", files.comprobanteDomicilio);
  body.append("identificacionResponsable", files.identificacionResponsable);

  let response;
  try {
    response = await fetch(`${API_URL}/api/organizaciones/solicitud`, {
      method: "POST",
      body,
    });
  } catch {
    throw new Error("No se pudo conectar con el servidor. Intenta de nuevo.");
  }

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    const error = new Error(data.mensaje || "No se pudo enviar la solicitud.");
    error.status = response.status;
    throw error;
  }

  return data;
}