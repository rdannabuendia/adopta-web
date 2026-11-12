using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Notificacione
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public string? Tipo { get; set; }

    public bool Leida { get; set; }

    public DateTime FechaEnvio { get; set; }

    public int? IdSolicitud { get; set; }

    public virtual SolicitudesAdopcion? IdSolicitudNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
