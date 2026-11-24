import Badge from "./Badge";

const TONES = {
  Activa: "success", Activo: "success", Aprobada: "success", Disponible: "success",
  Pendiente: "warning", "En revision": "warning", "Documentacion pendiente": "warning",
  "En proceso de adopcion": "warning", "En proceso de devolucion": "warning",
  Adoptada: "teal", "En seguimiento": "teal",
  Inactiva: "neutral", Inactivo: "neutral", Cancelada: "neutral", "No disponible": "neutral",
  Bloqueado: "danger", Rechazada: "danger",
  // Seguimiento (adaptación) y devoluciones
  Aceptada: "success",
  "En adaptacion": "warning", "En adaptación": "warning",
  Estable: "success",
  "Requiere atencion": "danger", "Requiere atención": "danger",
  "Sin seguimiento": "neutral",
  // Solicitudes de adopción (variantes de nombre en el catálogo)
  Aprobado: "success", Aceptado: "success", Rechazado: "danger", Cancelado: "neutral",
  "En proceso": "warning", "En revisión": "warning", "Documentación pendiente": "warning",
};

const LABELS = {
  "En proceso de adopcion": "En proceso de adopción",
  "En proceso de devolucion": "En proceso de devolución",
  "En revision": "En revisión",
  "Documentacion pendiente": "Documentación pendiente",
  "En adaptacion": "En adaptación",
  "Requiere atencion": "Requiere atención",
};

export const statusLabel = (status) => LABELS[status] ?? status;

export default function StatusBadge({ status, tone, className }) {
  return (
    <Badge size="sm" tone={tone ?? TONES[status] ?? "neutral"} className={className}>
      {statusLabel(status)}
    </Badge>
  );
}