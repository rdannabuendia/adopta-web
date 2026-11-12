import http from "../../../api/http";

// Servicios de la landing (ajusta los endpoints a tu backend)
export const landingService = {
  // Ejemplo: mascotas destacadas para mostrar en la landing
  getFeaturedPets: async () => {
    const { data } = await http.get("/pets/featured");
    return data;
  },

  // Formulario de contacto
  sendContactMessage: async (payload) => {
    const { data } = await http.post("/contact", payload);
    return data;
  },
};

export default landingService;
