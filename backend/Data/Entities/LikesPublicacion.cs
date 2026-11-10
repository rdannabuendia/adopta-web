using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class LikesPublicacion
{
    public int IdLike { get; set; }

    public int IdPublicacion { get; set; }

    public int IdUsuario { get; set; }

    public DateTime? FechaLike { get; set; }

    public virtual Publicacione IdPublicacionNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
