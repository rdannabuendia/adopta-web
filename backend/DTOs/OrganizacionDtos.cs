using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

public class SolicitudOrganizacionRequestDto
{
    [Required(ErrorMessage = "El nombre de la organización es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede exceder 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(120, ErrorMessage = "El correo no puede exceder 120 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de organización es obligatorio.")]
    [StringLength(60)]
    public string TipoOrganizacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\+?[0-9\s\-()]{10,20}$", ErrorMessage = "El teléfono no es válido.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adjunta el comprobante de existencia legal.")]
    public IFormFile ExistenciaLegal { get; set; } = null!;

    [Required(ErrorMessage = "Adjunta el comprobante de domicilio.")]
    public IFormFile ComprobanteDomicilio { get; set; } = null!;

    [Required(ErrorMessage = "Adjunta la identificación del responsable.")]
    public IFormFile IdentificacionResponsable { get; set; } = null!;

    public bool AceptaTerminos { get; set; }
}

public class SolicitudOrganizacionResponseDto
{
    public int IdOrganizacion { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

// ---- Para la tabla de solicitudes del módulo de organizaciones (admin) ----
public class DocumentoOrganizacionDto
{
    public int IdDocumento { get; set; }
    public string Tipo { get; set; } = string.Empty;   // ExistenciaLegal | ComprobanteDomicilio | IdentificacionResponsable
    public string NombreOriginal { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
}

public class OrganizacionSolicitudDto
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? TipoOrganizacion { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<DocumentoOrganizacionDto> Documentos { get; set; } = new();
}