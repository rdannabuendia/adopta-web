using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class PreferenciasUsuario
{
    public int IdPreferencia { get; set; }

    public int IdUsuario { get; set; }

    public string? Especie { get; set; }

    public string? Tamano { get; set; }

    public string? EdadPreferida { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
