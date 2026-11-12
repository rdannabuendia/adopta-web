using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class VistaReportesSeguimiento
{
    public int IdReporte { get; set; }

    public DateTime FechaReporte { get; set; }

    public string? Comportamiento { get; set; }

    public string? Salud { get; set; }

    public string? Alimentacion { get; set; }

    public string? Adaptacion { get; set; }

    public string? Observaciones { get; set; }

    public int IdAdopcion { get; set; }

    public int IdMascota { get; set; }

    public string Mascota { get; set; } = null!;

    public int IdOrganizacion { get; set; }

    public string Organizacion { get; set; } = null!;

    public int IdAdoptante { get; set; }

    public string? Adoptante { get; set; }
}
