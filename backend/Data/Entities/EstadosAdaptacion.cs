using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class EstadosAdaptacion
{
    public int IdEstadoAdaptacion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<HistorialAdaptacion> HistorialAdaptacions { get; set; } = new List<HistorialAdaptacion>();

    public virtual ICollection<Mascota> Mascota { get; set; } = new List<Mascota>();
}
