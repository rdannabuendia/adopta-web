using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class FotosSeguimiento
{
    public int IdFotoSeguimiento { get; set; }

    public int IdReporte { get; set; }

    public string RutaFoto { get; set; } = null!;

    public virtual ReportesSeguimiento IdReporteNavigation { get; set; } = null!;
}
