using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

public class OrganizacionesQueryDto
{
    public string? Buscar { get; set; }
    public string? Estado { get; set; }
    public string? Tipo { get; set; }
    public string? Ciudad { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public int TotalPaginas { get; set; }
}

public class OrganizacionListaDto
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public string? Ubicacion { get; set; }
    public string? TipoOrganizacion { get; set; }
    public int TotalMascotas { get; set; }
    public string Estado { get; set; } = null!;
}

public class BajaInfoDto
{
    public string Motivo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public DateTime FechaRevision { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class OrganizacionDetalleDto
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = null!;
    public string? FotoUrl { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? TipoOrganizacion { get; set; }
    public string? Descripcion { get; set; }
    public string? SitioWeb { get; set; }
    public string? HorarioAtencion { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? Youtube { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaRegistro { get; set; }
    public int TotalMascotas { get; set; }
    public List<DocumentoOrganizacionDto> Documentos { get; set; } = new();
    public BajaInfoDto? Baja { get; set; }
}

public class MotivoBajaDto
{
    public int IdMotivoBaja { get; set; }
    public string Nombre { get; set; } = null!;
}

public class BajaOrganizacionRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un motivo.")]
    public int IdMotivoBaja { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [MaxLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de revisión es obligatoria.")]
    public DateTime FechaRevision { get; set; }
}

public class SolicitudRegistroListaDto
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoOrganizacion { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int TotalDocumentos { get; set; }
    public string Estado { get; set; } = null!;
}

public class RechazarSolicitudRequestDto
{
    [Required(ErrorMessage = "El motivo es obligatorio.")]
    public string Motivo { get; set; } = string.Empty;
}