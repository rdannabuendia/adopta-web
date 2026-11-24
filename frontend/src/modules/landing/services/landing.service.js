const API_URL = (import.meta.env.VITE_API_URL || "").replace(/\/+$/, "");

async function request(path, options) {
  let response;

  try {
    response = await fetch(`${API_URL}${path}`, options);
  } catch {
    throw new Error("No se pudo conectar con el servidor. Inténtalo de nuevo en unos minutos.");
  }

  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    throw new Error(data.mensaje || data.message || "Ocurrió un error. Inténtalo de nuevo.");
  }

  return data;
}

// Servicios públicos de la landing (no requieren sesión)
export const landingService = {
  // Mascotas disponibles en orden aleatorio para el carrusel
  getFeaturedPets: (cantidad = 10) =>
    request(`/api/publico/mascotas?cantidad=${encodeURIComponent(cantidad)}`),

  // Formulario de contacto → llega a adoptasoporte@gmail.com
  sendContactMessage: (payload) =>
    request("/api/contacto", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    }),
};

export default landingService;
