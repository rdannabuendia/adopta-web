using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class RequisitosSolicitud
{
    public int IdRequisito { get; set; }

    public int IdSolicitud { get; set; }

    public int IdTipoDocumento { get; set; }

    public bool? Obligatorio { get; set; }

    public string Estado { get; set; } = null!;

    public string? MotivoRechazo { get; set; }

    public int? IdUsuarioRevisor { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime? FechaRevision { get; set; }

    public virtual ICollection<ArchivosRequisito> ArchivosRequisitos { get; set; } = new List<ArchivosRequisito>();

    public virtual SolicitudesAdopcion IdSolicitudNavigation { get; set; } = null!;

    public virtual TiposDocumento IdTipoDocumentoNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioRevisorNavigation { get; set; }
}
