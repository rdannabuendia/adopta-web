using System;
using System.Collections.Generic;

namespace Adopta.Api.Models;

public partial class VerificacionesCorreo
{
    public int IdVerificacion { get; set; }

    public int IdUsuario { get; set; }

    public string CodigoHash { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    public int Intentos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
