using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class SolicitudesDevolucion
{
    public int IdDevolucion { get; set; }

    public int IdAdopcion { get; set; }

    public string Explicacion { get; set; } = null!;

    public DateTime? FechaSolicitud { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaRevision { get; set; }

    public string? ObservacionesOrganizacion { get; set; }

    public virtual ICollection<DetalleMotivosDevolucion> DetalleMotivosDevolucions { get; set; } = new List<DetalleMotivosDevolucion>();

    public virtual Adopcione IdAdopcionNavigation { get; set; } = null!;
}
