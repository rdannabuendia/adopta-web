using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class DocumentosOrganizacion
{
    public int IdDocumento { get; set; }

    public int IdOrganizacion { get; set; }

    public string TipoDocumento { get; set; } = null!;

    public string NombreOriginal { get; set; } = null!;

    public string RutaRelativa { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public uint TamanoBytes { get; set; }

    public string HashSha256 { get; set; } = null!;

    public DateTime FechaCarga { get; set; }

    public virtual Organizacione IdOrganizacionNavigation { get; set; } = null!;
}
