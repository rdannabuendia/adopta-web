import Badge from "./Badge";

const TONES = {
  Activa: "success", Activo: "success", Aprobada: "success", Disponible: "success",
  Pendiente: "warning", "En revision": "warning", "Documentacion pendiente": "warning",
  "En proceso de adopcion": "warning", "En proceso de devolucion": "warning",
  Adoptada: "teal", "En seguimiento": "teal",
  Inactiva: "neutral", Inactivo: "neutral", Cancelada: "neutral", "No disponible": "neutral",
  Bloqueado: "danger", Rechazada: "danger",
};

const LABELS = {
  "En proceso de adopcion": "En proceso de adopción",
  "En proceso de devolucion": "En proceso de devolución",
  "En revision": "En revisión",
  "Documentacion pendiente": "Documentación pendiente",
};

export default function StatusBadge({ status, tone, className }) {
  return (
    <Badge size="sm" tone={tone ?? TONES[status] ?? "neutral"} className={className}>
      {LABELS[status] ?? status}
    </Badge>
  );
}