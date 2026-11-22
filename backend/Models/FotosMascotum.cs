using System;
using System.Collections.Generic;

namespace Adopta.Api.Models;

public partial class FotosMascotum
{
    public int IdFoto { get; set; }

    public int IdMascota { get; set; }

    public string RutaFoto { get; set; } = null!;

    public bool EsPrincipal { get; set; }

    public virtual Mascota IdMascotaNavigation { get; set; } = null!;
}
