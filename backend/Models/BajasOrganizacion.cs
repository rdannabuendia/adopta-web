using System;
using System.Collections.Generic;

namespace Adopta.Api.Models;

public partial class BajasOrganizacion
{
    public int IdBaja { get; set; }

    public int IdOrganizacion { get; set; }

    public int IdMotivoBaja { get; set; }

    public int IdUsuarioAdmin { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime FechaRevision { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual MotivosBaja IdMotivoBajaNavigation { get; set; } = null!;

    public virtual Organizacione IdOrganizacionNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioAdminNavigation { get; set; } = null!;
}
