using System;
using System.Collections.Generic;

namespace Adopta.Api.Models;

public partial class MotivosBaja
{
    public int IdMotivoBaja { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<BajasOrganizacion> BajasOrganizacions { get; set; } = new List<BajasOrganizacion>();
}
