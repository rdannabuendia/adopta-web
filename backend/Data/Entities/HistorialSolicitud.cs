using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class HistorialSolicitud
{
    public int IdHistorial { get; set; }

    public int IdSolicitud { get; set; }

    public int IdEstadoSolicitud { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaCambio { get; set; }

    public virtual EstadosSolicitud IdEstadoSolicitudNavigation { get; set; } = null!;

    public virtual SolicitudesAdopcion IdSolicitudNavigation { get; set; } = null!;
}
