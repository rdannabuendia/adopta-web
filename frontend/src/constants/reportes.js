import { ChartIcon, ClipboardIcon, PawIcon, TimelineIcon } from "../components/ui/Icons";

/** Fuente única de las pantallas de reportes: la usan el menú interno, el panel y el formulario de exportación. */
export const REPORTES = [
  { tipo: "general", to: "/reportes", end: true, label: "Panel general", icon: ChartIcon,
    description: "Resumen de la actividad de Adopta+." },
  { tipo: "adopciones", to: "/reportes/adopciones", label: "Adopciones", icon: TimelineIcon,
    description: "Adopciones realizadas durante un periodo." },
  { tipo: "solicitudes", to: "/reportes/solicitudes", label: "Solicitudes", icon: ClipboardIcon,
    description: "Estado y volumen de solicitudes." },
  { tipo: "mascotas", to: "/reportes/mascotas", label: "Mascotas", icon: PawIcon,
    description: "Distribución por disponibilidad y estado." },
];

export const TIPOS_REPORTE = [
  { value: "general", label: "Estadísticas generales" },
  { value: "adopciones", label: "Reporte de adopciones" },
  { value: "solicitudes", label: "Reporte de solicitudes" },
  { value: "mascotas", label: "Estadísticas de mascotas" },
];

export const FORMATOS_REPORTE = [
  { value: "pdf", label: "PDF", extension: "pdf" },
  { value: "excel", label: "Excel", extension: "xlsx" },
];

export const REPORTES_BREADCRUMB = [
  { label: "Inicio", to: "/inicio" },
  { label: "Reportes y estadísticas · Admin", to: "/reportes" },
];
