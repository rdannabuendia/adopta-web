using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class PerfilesAdoptante
{
    public int IdPerfil { get; set; }

    public int IdUsuario { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public string? Direccion { get; set; }

    public string? Ciudad { get; set; }

    public string? CodigoPostal { get; set; }

    public string? TipoVivienda { get; set; }

    public bool? TieneMascotas { get; set; }

    public string? ExperienciaMascotas { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
