using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class VistaSolicitudesAdopcion
{
    public int IdSolicitud { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public int IdMascota { get; set; }

    public string Mascota { get; set; } = null!;

    public int IdAdoptante { get; set; }

    public string? Adoptante { get; set; }

    public string Correo { get; set; } = null!;

    public string EstadoSolicitud { get; set; } = null!;

    public bool DocumentacionCompleta { get; set; }

    public DateTime? FechaLimiteDocumentos { get; set; }

    public int IdOrganizacion { get; set; }

    public string Organizacion { get; set; } = null!;
}
