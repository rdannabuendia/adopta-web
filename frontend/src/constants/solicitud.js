const TAMANOS = { Pequeno: "Pequeño", Mediano: "Mediano", Grande: "Grande" };

export const etiquetaTamano = (tamano) => TAMANOS[tamano] ?? tamano;

export const resumenMascotaSolicitud = (m) =>
  [m.especie, etiquetaTamano(m.tamano), m.edadAproximada].filter(Boolean).join(" · ");

export const DICTAMENES = [
  {
    value: "Aceptada",
    label: "Aceptar solicitud",
    description: "Se registra la adopción y se avisa al adoptante.",
    tone: "success",
  },
  {
    value: "Rechazada",
    label: "Rechazar solicitud",
    description: "Se avisa al adoptante con el motivo que escribas.",
    tone: "danger",
  },
];

export const MIN_TEXTO = 10;
export const MAX_TEXTO = 1000;

export const siNo = (v) => (v === null || v === undefined ? null : v ? "Sí" : "No");
