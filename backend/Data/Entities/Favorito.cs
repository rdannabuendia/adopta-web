using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Favorito
{
    public int IdFavorito { get; set; }

    public int IdUsuario { get; set; }

    public int IdMascota { get; set; }

    public DateTime? FechaAgregado { get; set; }

    public virtual Mascota IdMascotaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
