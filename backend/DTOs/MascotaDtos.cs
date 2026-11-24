using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

public class MascotasQueryDto
{
    public string? Buscar { get; set; }
    public string? Especie { get; set; }
    public string? Tamano { get; set; }
    public string? Edad { get; set; }
    public string? Estado { get; set; }
    public int Pagina { get; set; } = 1;
    public int PorPagina { get; set; } = 9;
}

public class MascotaListaDto
{
    public int IdMascota { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public string Sexo { get; set; } = null!;
    public string? Tamano { get; set; }
    public string? EdadAproximada { get; set; }
    public string Estado { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public int IdOrganizacion { get; set; }
    public string Organizacion { get; set; } = null!;
    public string? OrganizacionFotoUrl { get; set; }
}

public class FotoMascotaDto
{
    public int IdFoto { get; set; }
    public string Url { get; set; } = null!;
    public bool EsPrincipal { get; set; }
}

public class MascotaDetalleDto
{
    public int IdMascota { get; set; }
    public int IdOrganizacion { get; set; }
    public string Organizacion { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public string Sexo { get; set; } = null!;
    public string? Tamano { get; set; }
    public string? EdadAproximada { get; set; }
    public string? Color { get; set; }
    public decimal? Peso { get; set; }
    public string? Descripcion { get; set; }
    public string? EstadoSalud { get; set; }
    public bool Vacunado { get; set; }
    public bool Esterilizado { get; set; }
    public List<string> Caracteristicas { get; set; } = new();
    public string Estado { get; set; } = null!;
    public DateTime FechaRegistro { get; set; }
    public List<FotoMascotaDto> Fotos { get; set; } = new();
    /// <summary>Estatus a los que la organización puede cambiarla manualmente (vacío = no se puede).</summary>
    public List<string> EstadosDestino { get; set; } = new();
}

/// <summary>Se recibe como multipart/form-data. Todo es nullable: la validación (con mensajes en español) la hace el servicio.</summary>
public class MascotaRequestDto
{
    public string? Nombre { get; set; }
    public string? Especie { get; set; }
    public string? Raza { get; set; }
    public string? Sexo { get; set; }
    public string? Tamano { get; set; }
    public string? EdadAproximada { get; set; }
    public string? Color { get; set; }
    public string? Peso { get; set; }            // texto: se parsea con cultura invariante
    public string? Descripcion { get; set; }
    public string? EstadoSalud { get; set; }
    public bool Vacunado { get; set; }
    public bool Esterilizado { get; set; }
    public List<string>? Caracteristicas { get; set; }

    public List<IFormFile>? Fotos { get; set; }          // fotos nuevas
    public List<int>? FotosEliminar { get; set; }        // ids de fotos existentes a quitar (solo edición)
    public int? IdFotoPrincipal { get; set; }            // foto existente que será la principal
    public int? IndicePrincipalNueva { get; set; }       // o la posición (en Fotos) de una nueva
}

public class MascotaGuardadaResponseDto
{
    public int IdMascota { get; set; }
    public string Mensaje { get; set; } = null!;
}

public class CambiarEstadoMascotaRequestDto
{
    [Required(ErrorMessage = "Selecciona el nuevo estatus.")]
    public string Estado { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

public class OrganizacionMascotasDto
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public string? Ciudad { get; set; }
    public string? TipoOrganizacion { get; set; }
    public string Estado { get; set; } = null!;
    public int TotalMascotas { get; set; }
    public int Disponibles { get; set; }
}