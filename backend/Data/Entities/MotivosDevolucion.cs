using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class MotivosDevolucion
{
    public int IdMotivo { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<DetalleMotivosDevolucion> DetalleMotivosDevolucions { get; set; } = new List<DetalleMotivosDevolucion>();
}
