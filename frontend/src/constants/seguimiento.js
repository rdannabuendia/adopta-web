export const ESTADOS_DEVOLUCION = [
  { value: "Pendiente", label: "Pendiente" },
  { value: "Aceptada", label: "Aceptada" },
];

/** "Luna · Perro" */
export const tituloMascota = (nombre, especie) => [nombre, especie].filter(Boolean).join(" · ");

/** "Luna · Perro · 3 años" */
export const resumenSeguimiento = (d) => [d.mascota, d.especie, d.edadAproximada].filter(Boolean).join(" · ");

export const textoEstadoDevolucion = (estado) =>
  estado === "Pendiente" ? "Pendiente de revisión" : estado;
