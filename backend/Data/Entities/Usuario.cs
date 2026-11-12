using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public string Correo { get; set; } = null!;

    /// <summary>
    /// Guardar hash BCrypt o Argon2, nunca texto plano
    /// </summary>
    public string Contrasena { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? FotoPerfil { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<ArchivosRequisito> ArchivosRequisitos { get; set; } = new List<ArchivosRequisito>();

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<HistorialAdaptacion> HistorialAdaptacions { get; set; } = new List<HistorialAdaptacion>();

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual ICollection<LikesPublicacion> LikesPublicacions { get; set; } = new List<LikesPublicacion>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual Organizacione? Organizacione { get; set; }

    public virtual PerfilesAdoptante? PerfilesAdoptante { get; set; }

    public virtual ICollection<PreferenciasUsuario> PreferenciasUsuarios { get; set; } = new List<PreferenciasUsuario>();

    public virtual ICollection<Publicacione> Publicaciones { get; set; } = new List<Publicacione>();

    public virtual ICollection<RequisitosSolicitud> RequisitosSolicituds { get; set; } = new List<RequisitosSolicitud>();

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();

    public virtual ICollection<SolicitudesDevolucion> SolicitudesDevolucions { get; set; } = new List<SolicitudesDevolucion>();
}
