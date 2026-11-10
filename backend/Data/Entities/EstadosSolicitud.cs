using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class EstadosSolicitud
{
    public int IdEstadoSolicitud { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<HistorialSolicitud> HistorialSolicituds { get; set; } = new List<HistorialSolicitud>();

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();
}
