using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class VistaCatalogoMascota
{
    public int IdMascota { get; set; }

    public string Nombre { get; set; } = null!;

    public string Especie { get; set; } = null!;

    public string? Raza { get; set; }

    public string Sexo { get; set; } = null!;

    public string? EdadAproximada { get; set; }

    public string? Tamano { get; set; }

    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public string? Comportamiento { get; set; }

    public string? EstadoSalud { get; set; }

    public bool Vacunado { get; set; }

    public bool Esterilizado { get; set; }

    public string EstadoMascota { get; set; } = null!;

    public string EstadoAdaptacion { get; set; } = null!;

    public int IdOrganizacion { get; set; }

    public string Organizacion { get; set; } = null!;

    public string? FotoPrincipal { get; set; }
}
