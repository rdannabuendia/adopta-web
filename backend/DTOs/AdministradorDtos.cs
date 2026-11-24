using System.ComponentModel.DataAnnotations;

namespace Adopta.Api.DTOs;

public class AdministradoresQueryDto
{
    public string? Buscar { get; set; }
    public string? Estado { get; set; }   // Activo | Inactivo
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
}

public class AdministradorRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre admite máximo 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(60, ErrorMessage = "El apellido paterno admite máximo 60 caracteres.")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "El apellido materno admite máximo 60 caracteres.")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(120, ErrorMessage = "El correo admite máximo 120 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [RegularExpression(@"^$|^\+?[0-9\s\-()]{10,20}$", ErrorMessage = "El teléfono no es válido (10 a 20 dígitos).")]
    public string? Telefono { get; set; }
}

public class CambiarEstadoAdministradorDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;   // Activo | Inactivo
}

public class AdministradorListaDto
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string? Telefono { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaRegistro { get; set; }
    public bool PendienteCambioPassword { get; set; }
    public bool EsUsuarioActual { get; set; }
}

public class AdministradorDetalleDto : AdministradorListaDto
{
    public string NombrePila { get; set; } = null!;
    public string ApellidoPaterno { get; set; } = null!;
    public string? ApellidoMaterno { get; set; }
}
