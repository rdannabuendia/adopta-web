using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class EstadosMascotum
{
    public int IdEstadoMascota { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Mascota> Mascota { get; set; } = new List<Mascota>();
}
