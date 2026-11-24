const TIPOS = {
  SolicitudOrganizacion: { tono: "info", destino: () => "/organizaciones/solicitudes" },
  NuevaSolicitud: { tono: "success", destino: (n) => (n.idSolicitud ? `/solicitudes/${n.idSolicitud}` : "/solicitudes") },
  DocumentosRecibidos: { tono: "success", destino: (n) => (n.idSolicitud ? `/solicitudes/${n.idSolicitud}` : "/solicitudes") },
  ReporteSeguimiento: { tono: "info", destino: () => "/seguimiento" },
  SolicitudDevolucion: { tono: "warning", destino: () => "/seguimiento/devoluciones" },
};

const POR_DEFECTO = { tono: "info", destino: () => null };

export const infoNotificacion = (n) => TIPOS[n?.tipo] ?? POR_DEFECTO;
