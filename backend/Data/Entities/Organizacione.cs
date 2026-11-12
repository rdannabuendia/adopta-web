using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Organizacione
{
    public int IdOrganizacion { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string? CodigoPostal { get; set; }

    public string? Telefono { get; set; }

    public string? CorreoContacto { get; set; }

    public string? SitioWeb { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Mascota> Mascota { get; set; } = new List<Mascota>();
}
