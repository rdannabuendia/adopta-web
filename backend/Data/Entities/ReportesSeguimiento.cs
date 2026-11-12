using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class ReportesSeguimiento
{
    public int IdReporte { get; set; }

    public int IdAdopcion { get; set; }

    public string? Comportamiento { get; set; }

    public string? Salud { get; set; }

    public string? Alimentacion { get; set; }

    public string? Adaptacion { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaReporte { get; set; }

    public virtual ICollection<FotosSeguimiento> FotosSeguimientos { get; set; } = new List<FotosSeguimiento>();

    public virtual Adopcione IdAdopcionNavigation { get; set; } = null!;
}
