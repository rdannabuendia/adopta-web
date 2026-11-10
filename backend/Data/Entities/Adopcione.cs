using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Adopcione
{
    public int IdAdopcion { get; set; }

    public int IdSolicitud { get; set; }

    public DateTime FechaAdopcion { get; set; }

    public string? ContratoUrl { get; set; }

    public string? Observaciones { get; set; }

    public virtual SolicitudesAdopcion IdSolicitudNavigation { get; set; } = null!;

    public virtual ICollection<ReportesSeguimiento> ReportesSeguimientos { get; set; } = new List<ReportesSeguimiento>();

    public virtual ICollection<SolicitudesDevolucion> SolicitudesDevolucions { get; set; } = new List<SolicitudesDevolucion>();
}
