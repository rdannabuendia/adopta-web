using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class SolicitudesAdopcion
{
    public int IdSolicitud { get; set; }

    public int IdMascota { get; set; }

    public int IdAdoptante { get; set; }

    public DateTime? FechaSolicitud { get; set; }

    public string? MotivoAdopcion { get; set; }

    public string? ExperienciaPrevia { get; set; }

    public string? CondicionesVivienda { get; set; }

    public int IdEstadoSolicitud { get; set; }

    public string? ObservacionesOrganizacion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public virtual Adopcione? Adopcione { get; set; }

    public virtual ICollection<HistorialSolicitud> HistorialSolicituds { get; set; } = new List<HistorialSolicitud>();

    public virtual Usuario IdAdoptanteNavigation { get; set; } = null!;

    public virtual EstadosSolicitud IdEstadoSolicitudNavigation { get; set; } = null!;

    public virtual Mascota IdMascotaNavigation { get; set; } = null!;
}
