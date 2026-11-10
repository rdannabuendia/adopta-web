using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class DetalleMotivosDevolucion
{
    public int IdDetalle { get; set; }

    public int IdDevolucion { get; set; }

    public int IdMotivo { get; set; }

    public virtual SolicitudesDevolucion IdDevolucionNavigation { get; set; } = null!;

    public virtual MotivosDevolucion IdMotivoNavigation { get; set; } = null!;
}
