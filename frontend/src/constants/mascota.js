export const ESPECIES = ["Perro", "Gato", "Otro"];

export const TAMANOS = [
  { value: "Pequeno", label: "Pequeño" },
  { value: "Mediano", label: "Mediano" },
  { value: "Grande", label: "Grande" },
];

export const SEXOS = ["Macho", "Hembra", "Desconocido"];

export const EDADES = [
  "Menos de 1 año",
  "1 año",
  ...Array.from({ length: 13 }, (_, i) => `${i + 2} años`),
  "15 años o más",
];

export const ESTADOS_MASCOTA = [
  { value: "Disponible", label: "Disponible" },
  { value: "En proceso de adopcion", label: "En proceso de adopción" },
  { value: "Adoptada", label: "Adoptada" },
  { value: "En seguimiento", label: "En seguimiento" },
  { value: "En proceso de devolucion", label: "En proceso de devolución" },
  { value: "No disponible", label: "No disponible" },
];

export const CARACTERISTICAS_SUGERIDAS = [
  "Cariñoso",
  "Juguetón",
  "Sociable",
  "Tranquilo",
  "Activo",
  "Protector",
  "Obediente",
  "Independiente",
];

export const RAZAS_COMUNES = {
  Perro: [
    "Mestizo",
    "Labrador",
    "Beagle",
    "Pastor Alemán",
    "Chihuahua",
    "Pug",
    "Husky",
    "Poodle",
    "Bulldog",
    "Golden Retriever",
    "Pomerania",
    "Schnauzer",
  ],
  Gato: ["Mestizo", "Siamés", "Persa", "Angora", "Maine Coon", "Bengalí", "Angora Turco"],
  Otro: [],
};

export const MAX_FOTOS = 6;

export const tamanoLabel = (v) => TAMANOS.find((t) => t.value === v)?.label ?? v;
export const estadoLabel = (v) => ESTADOS_MASCOTA.find((e) => e.value === v)?.label ?? v;

/** "Perro - Mediano - 2 años" */
export const resumenMascota = (m) =>
  [m.especie, tamanoLabel(m.tamano), m.edadAproximada].filter(Boolean).join(" - ");