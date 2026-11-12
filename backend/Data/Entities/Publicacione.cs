using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Publicacione
{
    public int IdPublicacion { get; set; }

    public int IdUsuario { get; set; }

    public string Contenido { get; set; } = null!;

    public string? Imagen { get; set; }

    public DateTime FechaPublicacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<LikesPublicacion> LikesPublicacions { get; set; } = new List<LikesPublicacion>();
}
