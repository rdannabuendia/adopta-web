using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class ArchivosRequisito
{
    public int IdArchivo { get; set; }

    public int IdRequisito { get; set; }

    public int IdUsuarioCarga { get; set; }

    public string NombreOriginal { get; set; } = null!;

    public string NombreAlmacenado { get; set; } = null!;

    public string RutaRelativa { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public uint TamanoBytes { get; set; }

    public string HashSha256 { get; set; } = null!;

    public DateTime? FechaEmisionDocumento { get; set; }

    public string Origen { get; set; } = null!;

    public bool? EsVigente { get; set; }

    public DateTime FechaCarga { get; set; }

    public virtual RequisitosSolicitud IdRequisitoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioCargaNavigation { get; set; } = null!;
}
