using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class HistorialAdaptacion
{
    public int IdHistorialAdaptacion { get; set; }

    public int IdMascota { get; set; }

    public int IdEstadoAdaptacion { get; set; }

    public int IdUsuarioResponsable { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCambio { get; set; }

    public virtual EstadosAdaptacion IdEstadoAdaptacionNavigation { get; set; } = null!;

    public virtual Mascota IdMascotaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioResponsableNavigation { get; set; } = null!;
}
