using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class TiposDocumento
{
    public int IdTipoDocumento { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string FormatosPermitidos { get; set; } = null!;

    public byte TamanoMaxMb { get; set; }

    public byte MinArchivos { get; set; }

    public byte MaxArchivos { get; set; }

    public bool RequiereFechaEmision { get; set; }

    public byte? VigenciaMeses { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<RequisitosSolicitud> RequisitosSolicituds { get; set; } = new List<RequisitosSolicitud>();
}
