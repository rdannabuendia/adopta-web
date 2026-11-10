using System;
using System.Collections.Generic;

namespace Adopta.Api.Data.Entities;

public partial class Mascota
{
    public int IdMascota { get; set; }

    public int IdOrganizacion { get; set; }

    public int IdEstadoMascota { get; set; }

    public int IdEstadoAdaptacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Especie { get; set; } = null!;

    public string? Raza { get; set; }

    public string? Sexo { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public string? EdadAproximada { get; set; }

    public string? Tamano { get; set; }

    public string? Color { get; set; }

    public decimal? Peso { get; set; }

    public string? Descripcion { get; set; }

    public string? Comportamiento { get; set; }

    public string? EstadoSalud { get; set; }

    public bool? Vacunado { get; set; }

    public bool? Esterilizado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<FotosMascotum> FotosMascota { get; set; } = new List<FotosMascotum>();

    public virtual ICollection<HistorialAdaptacion> HistorialAdaptacions { get; set; } = new List<HistorialAdaptacion>();

    public virtual EstadosAdaptacion IdEstadoAdaptacionNavigation { get; set; } = null!;

    public virtual EstadosMascotum IdEstadoMascotaNavigation { get; set; } = null!;

    public virtual Organizacione IdOrganizacionNavigation { get; set; } = null!;

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();
}
