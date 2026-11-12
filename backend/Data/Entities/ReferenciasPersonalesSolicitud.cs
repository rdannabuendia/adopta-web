using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class ReferenciasPersonalesSolicitud
{
    public int IdReferencia { get; set; }

    public int IdSolicitud { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Relacion { get; set; } = null!;

    public byte? AniosConocidos { get; set; }

    public virtual SolicitudesAdopcion IdSolicitudNavigation { get; set; } = null!;
}
